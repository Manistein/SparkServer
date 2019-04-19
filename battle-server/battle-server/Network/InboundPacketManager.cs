// author:manistein
// since: 2019.03.15
// desc:  Process inbound packets

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BattleServer.Network
{
    public class InboundPacketManager
    {
        // inbound buffer、outbound buffer and memory buffer is 64k
        private int BUFFER_SIZE = 64 * 1024;

        private Queue<Buffer> m_readBuffers = new Queue<Buffer>();
        private Buffer m_lastBuffer;

        private int m_packetSize = 0;
        private int m_readNum = 0;
        private int m_readHeader = 0;

        private BufferPool m_bufferPool;
        public byte[] InboundBuffer { get; set; }
        public byte[] MemoryBuffer { get; set; }

        private ReadCompleteHandle m_onReadPacketComplete;
        private SessionErrorHandle m_onSessionError;
        private long m_sessionId = 0;

        public InboundPacketManager()
        {
            InboundBuffer = new byte[BUFFER_SIZE];
            MemoryBuffer = new byte[BUFFER_SIZE];
        }

        public void Init(long sessionId, BufferPool bufferPool, ReadCompleteHandle readCallback, SessionErrorHandle errorCallback)
        {
            m_sessionId = sessionId;
            m_bufferPool = bufferPool;
            m_onReadPacketComplete = readCallback;
            m_onSessionError = errorCallback;
        }

        public void ProcessPacket(byte[] inboundBytes, int transferedBytes)
        {
            int sourceIndex = 0;
            while(transferedBytes > 0)
            {
                if (transferedBytes == 1 && m_readNum == 0 && m_packetSize == 0)
                {
                    m_readHeader = inboundBytes[sourceIndex];
                    m_readNum = -1;
                    transferedBytes -= 1;
                    sourceIndex++;
                }
                else
                {
                    if (m_readNum == -1)
                    {
                        m_packetSize = m_readHeader << 8 | inboundBytes[sourceIndex];
                        m_readNum = 0;
                        transferedBytes -= 1;
                        sourceIndex++;

                        if (m_packetSize > Session.MaxPacketSize)
                        {
                            m_onSessionError(m_sessionId, (int)SocketError.Disconnecting, "May be this is a illegal connection");
                            break;
                        }
                    }
                    else if (m_readNum == 0)
                    {
                        if (m_packetSize == 0)
                        {
                            m_packetSize = inboundBytes[sourceIndex] << 8 | inboundBytes[sourceIndex + 1];
                            transferedBytes -= 2;
                            sourceIndex += 2;

                            if (m_packetSize > Session.MaxPacketSize)
                            {
                                m_onSessionError(m_sessionId, (int)SocketError.Disconnecting, "May be this is a illegal connection");
                                break;
                            }
                        }

                        if (transferedBytes >= m_packetSize)
                        {
                            // a packet read complete
                            Array.Copy(inboundBytes, sourceIndex, MemoryBuffer, 0, m_packetSize);

                            byte[] transferBuffer = new byte[m_packetSize];
                            Array.Copy(MemoryBuffer, 0, transferBuffer, 0, m_packetSize);
                            m_onReadPacketComplete(m_sessionId, transferBuffer, m_packetSize);

                            transferedBytes -= m_packetSize;
                            sourceIndex += m_packetSize;

                            m_readHeader = 0;
                            m_readNum = 0;
                            m_packetSize = 0;
                        }
                        else
                        {
                            ProcessUncomplete(ref inboundBytes, ref transferedBytes, ref sourceIndex);
                        }
                    }
                    else
                    {
                        if (m_readNum + transferedBytes >= m_packetSize)
                        {
                            int leftBytes = m_packetSize - m_readNum;
                            Buffer head = m_readBuffers.Dequeue();
                            int destIndex = 0;
                            while(head != null)
                            {
                                Array.Copy(head.Memory, head.Begin, MemoryBuffer, destIndex, head.End - head.Begin);
                                destIndex += head.End - head.Begin;
                                m_bufferPool.Push(head);

                                if (m_readBuffers.Count > 0)
                                {
                                    head = m_readBuffers.Dequeue();
                                }
                                else
                                {
                                    head = null;
                                }
                            }

                            Array.Copy(inboundBytes, sourceIndex, MemoryBuffer, destIndex, leftBytes);

                            byte[] transferBuffer = new byte[m_packetSize];
                            Array.Copy(MemoryBuffer, 0, transferBuffer, 0, m_packetSize);
                            m_onReadPacketComplete(m_sessionId, transferBuffer, m_packetSize);

                            transferedBytes -= leftBytes;
                            sourceIndex += leftBytes;

                            m_packetSize = 0;
                            m_readHeader = 0;
                            m_readNum = 0;
                        }
                        else
                        {
                            int freeBytes = m_lastBuffer.Memory.Length - m_lastBuffer.End;
                            if (freeBytes > 0)
                            {
                                int fillBytes = Math.Min(transferedBytes, freeBytes);
                                Array.Copy(inboundBytes, sourceIndex, m_lastBuffer.Memory, m_lastBuffer.End, fillBytes);

                                m_lastBuffer.End += fillBytes;
                                m_readNum += fillBytes;

                                sourceIndex += fillBytes;
                                transferedBytes -= fillBytes;
                            }

                            if (transferedBytes > 0)
                            {
                                ProcessUncomplete(ref inboundBytes, ref transferedBytes, ref sourceIndex);
                            }
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            if (m_readBuffers.Count <= 0)
                return;

            Buffer buf = m_readBuffers.Dequeue();
            while (buf != null)
            {
                m_bufferPool.Push(buf);

                if (m_readBuffers.Count > 0)
                    buf = m_readBuffers.Dequeue();
                else
                    buf = null;
            }
        }

        private void ProcessUncomplete(ref byte[] inboundBytes, ref int transferedBytes, ref int sourceIndex)
        {
            int needBlocks = (int)Math.Ceiling((float)transferedBytes / (float)BUFFER_SIZE);
            for (int i = 0; i < needBlocks; i++)
            {
                Buffer buf = m_bufferPool.Pop();
                int copyBlockSize = Math.Min(transferedBytes, buf.Memory.Length);
                Array.Copy(inboundBytes, sourceIndex, buf.Memory, buf.End, copyBlockSize);
                buf.End += copyBlockSize;

                m_readBuffers.Enqueue(buf);
                m_lastBuffer = buf;
                m_readNum += copyBlockSize;

                transferedBytes -= copyBlockSize;
                sourceIndex += copyBlockSize;
            }
        }
    }
}

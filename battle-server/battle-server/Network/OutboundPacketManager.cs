// author:manistein
// since: 2019.03.15
// desc:  Process outbound packets

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleServer.Network
{
    public class OutboundPacketManager
    {
        private Queue<Buffer> m_outboundBuffers = new Queue<Buffer>();
        private Buffer m_lastBuffer;
        private BufferPool m_bufferPool;

        public Buffer HeadBuffer { get; set; }

        public void Init(BufferPool buffer)
        {
            m_bufferPool = buffer;
        }

        public void ProcessPacket(byte[] buffer, int transferedBytes)
        {
            int sourceIndex = 0;
            while (transferedBytes > 0)
            {
                if (m_lastBuffer == null || m_lastBuffer.End >= m_lastBuffer.Memory.Length)
                {
                    Buffer newBuffer = m_bufferPool.Pop();
                    m_outboundBuffers.Enqueue(newBuffer);
                    m_lastBuffer = newBuffer;
                }

                // free bytes are from End index to Length - 1
                if (transferedBytes > (m_lastBuffer.Memory.Length - m_lastBuffer.End))
                {
                    Array.Copy(buffer, sourceIndex, m_lastBuffer.Memory, m_lastBuffer.End, m_lastBuffer.Memory.Length - m_lastBuffer.End);
                    sourceIndex += m_lastBuffer.Memory.Length - m_lastBuffer.End;
                    transferedBytes -= m_lastBuffer.Memory.Length - m_lastBuffer.End;
                    m_lastBuffer.End = m_lastBuffer.Memory.Length;
                }
                else
                {
                    Array.Copy(buffer, sourceIndex, m_lastBuffer.Memory, m_lastBuffer.End, transferedBytes);
                    sourceIndex += transferedBytes;
                    m_lastBuffer.End += transferedBytes;
                    transferedBytes = 0;
                }
            }
        }

        public void NextBuffer()
        {
            Buffer oldBuffer = HeadBuffer;

            if (m_outboundBuffers.Count > 0)
            {
                HeadBuffer = m_outboundBuffers.Dequeue();
                // m_lastbuffer will be wrote in tcp thread
                // HeadBuffer will be wrote in system socket thread
                // so, we must avoid different threads write data in the same buffer
                if (HeadBuffer == m_lastBuffer)
                {
                    m_lastBuffer = null;
                }
            }
            else
            {
                HeadBuffer = null;
                m_lastBuffer = null;
            }

            if (oldBuffer != null)
            {
                m_bufferPool.Push(oldBuffer);
            }
        }

        public void Stop()
        {
            if (m_outboundBuffers.Count <= 0)
                return;

            Buffer buf = m_outboundBuffers.Dequeue();
            while (buf != null)
            {
                m_bufferPool.Push(buf);

                if (m_outboundBuffers.Count > 0)
                    buf = m_outboundBuffers.Dequeue();
                else
                    buf = null;
            }
        }
    }
}

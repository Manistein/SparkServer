using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SparkServer.Framework.MessageQueue
{
    enum SocketMessageType
    {
        Connect     = 1,
        Disconnect  = 2,
        DATA        = 3,
    }

    class SocketMessage
    {
        public SocketMessageType Type { get; set; }
        public int TcpObjectId { get; set; }
    }

    class ConnectMessage : SocketMessage
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }

    class DisconnectMessage : SocketMessage
    {
        public long ConnectionId { get; set; }
    }

    class NetworkPacket : SocketMessage
    {
        public long ConnectionId { get; set; }
        public List<byte[]> Buffers { get; set; }
    }

    class NetworkPacketQueue
    {
        public static NetworkPacketQueue m_instance;
        private ConcurrentQueue<SocketMessage> m_netpackQueue = new ConcurrentQueue<SocketMessage>();

        // We must call this function first in main thread
        public static NetworkPacketQueue GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new NetworkPacketQueue();
            }

            return m_instance;
        }

        public void Push(SocketMessage socketMessage)
        {
            m_netpackQueue.Enqueue(socketMessage);
        }

        public SocketMessage Pop()
        {
            SocketMessage socketMessage = null;
            m_netpackQueue.TryDequeue(out socketMessage);
            return socketMessage;
        }
    }
}

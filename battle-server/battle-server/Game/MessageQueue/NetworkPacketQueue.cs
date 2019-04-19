using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace BattleServer.Game.MessageQueue
{
    class NetworkPacket
    {
        public long ConnectionId { get; set; }
        public int RPCSession { get; set; }
        public int ProtoId { get; set; }
        public byte[] Buffer { get; set; }
    }

    class NetworkPacketQueue
    {
        public static NetworkPacketQueue m_instance;
        private ConcurrentQueue<NetworkPacket> m_netpackQueue = new ConcurrentQueue<NetworkPacket>();

        // We must call this function first in main thread
        public static NetworkPacketQueue GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new NetworkPacketQueue();
            }

            return m_instance;
        }

        public void Push(long connectionId, int protoId, byte[] buffer, int rpcSession)
        {
            NetworkPacket networkPacket = new NetworkPacket();
            networkPacket.ConnectionId = connectionId;
            networkPacket.ProtoId = protoId;
            networkPacket.Buffer = buffer;
            networkPacket.RPCSession = rpcSession;
            m_netpackQueue.Enqueue(networkPacket);
        }

        public NetworkPacket Pop()
        {
            NetworkPacket networkPacket = null;
            m_netpackQueue.TryDequeue(out networkPacket);
            return networkPacket;
        }
    }
}

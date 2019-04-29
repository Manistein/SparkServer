using SparkServer.Framework.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Framework.Timer
{
    class SSTimerNode
    {
        public int Opaque { get; set; }
        public long TimeoutTimestamp { get; set; }
        public int Session { get; set; }
    }

    class SSTimer
    {
        public static SSTimer m_instance;
        private ConcurrentQueue<SSTimerNode> m_timerNodeQueue = new ConcurrentQueue<SSTimerNode>();

        // We should call this function in main thread first
        public static SSTimer GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new SSTimer();
            }

            return m_instance;
        }

        public void Loop()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int count = m_timerNodeQueue.Count;
            for (int i = 0; i < count; i ++)
            {
                SSTimerNode timerNode = null;
                if (m_timerNodeQueue.TryDequeue(out timerNode))
                {
                    if (timestamp >= timerNode.TimeoutTimestamp)
                    {
                        Message msg = new Message();
                        msg.Source = 0;
                        msg.Destination = timerNode.Opaque;
                        msg.Method = "";
                        msg.Data = null;
                        msg.RPCSession = timerNode.Session;
                        msg.Type = MessageType.Timer;

                        ServiceBase service = ServiceSlots.GetInstance().Get(timerNode.Opaque);
                        service.Push(msg);
                    }
                    else
                    {
                        m_timerNodeQueue.Enqueue(timerNode);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public void Add(SSTimerNode node)
        {
            m_timerNodeQueue.Enqueue(node);
        }
    }
}
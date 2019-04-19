using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace BattleServer.Game.MessageQueue
{
    class GlobalMQ
    {
        private static GlobalMQ m_instance;
        private ConcurrentQueue<int> m_serviceQueue = new ConcurrentQueue<int>();

        // We should call GetInstance first in main thread
        public static GlobalMQ GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new GlobalMQ();
            }

            return m_instance;
        }

        public void Push(int serviceId)
        {
            m_serviceQueue.Enqueue(serviceId);
        }

        public int Pop()
        {
            int serviceId = 0;
            if (!m_serviceQueue.TryDequeue(out serviceId))
            {
                return 0;
            }
            return serviceId;
        }
    }
}

// author:manistein
// since: 2019.03.15
// desc:  This class is for guarantee thread safe of calling io complete callback by system socket threads

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SparkServer.Network
{
    public class TCPSynchronizeContext : SynchronizationContext
    {
        private static TCPSynchronizeContext m_instance;
        private int m_threadId = Thread.CurrentThread.ManagedThreadId;

        private ConcurrentQueue<Action> m_concurrentQueue = new ConcurrentQueue<Action>();
        private Action action;

        public TCPSynchronizeContext()
        {

        }

        // this function must call in tcp thread first
        public static TCPSynchronizeContext GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = new TCPSynchronizeContext();
            }
            return m_instance;
        }

        public void Loop()
        {
            while (true)
            {
                if (m_concurrentQueue.TryDequeue(out action))
                {
                    action();
                }
                else
                {
                    break;
                }
            }

        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            if (m_threadId == Thread.CurrentThread.ManagedThreadId)
            {
                callback(state);
            }
            else
            {
                m_concurrentQueue.Enqueue(() => { callback(state); });
            }
        }
    }
}

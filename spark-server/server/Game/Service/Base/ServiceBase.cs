using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SparkServer.Game.MessageQueue;

namespace SparkServer.Game.Service
{
    enum MessageType
    {
        Socket  = 1,
        Service = 2,
    }

    class Message
    {
        public int ProtoId { get; set; }
        public byte[] Data { get; set; }
        public int Source { get; set; }
        public int Destination { get; set; }
        public long ConnectionId { get; set; }
        public int RPCSession { get; set; }
    }

    class ServiceBase
    {
        private Queue<Message> m_messageQueue = new Queue<Message>();
        private SpinLock m_spinlock = new SpinLock();
        private bool m_isInGlobal = false;

        protected int m_loggerId = 0;
        protected int m_serviceId = 0;

        public virtual void Init(int loggerId)
        {
            m_loggerId = loggerId;
        }

        public virtual void Callback(Message msg)
        {

        }

        public Message Pop()
        {
            bool isLock = false;
            Message result = null;
            try
            {
                m_spinlock.Enter(ref isLock);
                if (m_messageQueue.Count > 0)
                {
                    result = m_messageQueue.Dequeue();
                }
                else
                {
                    m_isInGlobal = false;
                }
            }
            finally
            {
                if (isLock)
                    m_spinlock.Exit();
            }
            return result;
        }

        public void Push(Message msg)
        {
            bool isLock = false;
            try
            {
                m_spinlock.Enter(ref isLock);
                m_messageQueue.Enqueue(msg);
                if (!m_isInGlobal)
                {
                    GlobalMQ.GetInstance().Push(m_serviceId);
                    m_isInGlobal = true;
                }
            }
            finally
            {
                if (isLock)
                    m_spinlock.Exit();
            }
        }

        public void SetId(int id)
        {
            m_serviceId = id;
        }

        public int GetId()
        {
            return m_serviceId;
        }
    }
}

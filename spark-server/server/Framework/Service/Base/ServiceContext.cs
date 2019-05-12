using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SparkServer.Framework.MessageQueue;
using SparkServer.Framework.Utility;
using NetSprotoType;
using SparkServer.Framework.Timer;

namespace SparkServer.Framework.Service
{
    delegate void Method(int source, int session, string method, byte[] param);
    delegate void RPCCallback(SSContext context, string method, byte[] param, RPCError error);
    delegate void TimeoutCallback(SSContext context, long currentTime);

    enum RPCError
    {
        OK                    = 0,
        MethodNotExist        = 1,
        SocketDisconnected    = 2,
        RemoteError           = 3,
        ServiceRuntimeError   = 4,
    }

    // Spark Server context
    class SSContext
    {
        public Dictionary<string, int>    IntegerDict = new Dictionary<string, int>();
        public Dictionary<string, long>   LongDict    = new Dictionary<string, long>();
        public Dictionary<string, float>  FloatDict   = new Dictionary<string, float>();
        public Dictionary<string, double> DoubleDict  = new Dictionary<string, double>();
        public Dictionary<string, string> StringDict  = new Dictionary<string, string>();
        public Dictionary<string, bool>   BooleanDict = new Dictionary<string, bool>();
        public Dictionary<string, object> ObjectDict  = new Dictionary<string, object>();
    }

    class RPCResponseContext
    {
        public SSContext Context { get; set; }
        public RPCCallback Callback { get; set; }
    }

    class TimeoutContext
    {
        public TimeoutCallback Callback { get; set; }
        public SSContext Context { get; set; }
    }

    enum MessageType
    {
        Socket          = 1,
        ServiceRequest  = 2,
        ServiceResponse = 3,
        Error           = 4,
        Timer           = 5,
    }

    class Message
    {
        public string Method { get; set; }
        public byte[] Data { get; set; }
        public int Source { get; set; }
        public int Destination { get; set; }
        public int RPCSession { get; set; }
        public MessageType Type { get; set; }
    }

    class ServiceContext
    {
        private Queue<Message> m_messageQueue = new Queue<Message>();
        private SpinLock m_spinlock = new SpinLock();
        private bool m_isInGlobal = false;

        protected int m_loggerId = 0;
        protected int m_serviceAddress = 0;
        protected int m_totalServiceSession = 0;

        private Dictionary<string, Method> m_serviceMethods = new Dictionary<string, Method>();
        private Dictionary<int, RPCResponseContext> m_responseCallbacks = new Dictionary<int, RPCResponseContext>();
        private Dictionary<int, TimeoutContext> m_timeoutCallbacks = new Dictionary<int, TimeoutContext>();

        public ServiceContext()
        {
            RegisterServiceMethods("Init", OnInit);
        }

        protected void OnInit(int source, int session, string method, byte[] param)
        {
            Init();
        }

        protected virtual void Init()
        {

        }

        public virtual void Callback(Message msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case MessageType.ServiceRequest:
                        {
                            OnRequest(msg);
                        }
                        break;
                    case MessageType.ServiceResponse:
                        {
                            OnResponse(msg);
                        }
                        break;
                    case MessageType.Error:
                        {
                            OnError(msg);
                        }
                        break;
                    case MessageType.Socket:
                        {
                            OnSocketCommand(msg);
                        }
                        break;
                    case MessageType.Timer:
                        {
                            OnTimer(msg);
                        }
                        break;
                    default: break;
                }
            }
            catch(Exception e)
            {
                if (msg.Source > 0 && msg.RPCSession > 0)
                {
                    DoError(msg.Source, msg.RPCSession, RPCError.ServiceRuntimeError, "");
                }
                LoggerHelper.Info(m_serviceAddress, e.ToString());
            }
        }

        private void OnRequest(Message msg)
        {
            Method method = null;
            bool isExist = m_serviceMethods.TryGetValue(msg.Method, out method);
            if (isExist)
            {
                method(msg.Source, msg.RPCSession, msg.Method, msg.Data);
            }
            else
            {
                string text = string.Format("Service:{0} has not method {1}", m_serviceAddress, msg.Method);
                LoggerHelper.Info(m_serviceAddress, text);
                DoError(msg.Source, msg.RPCSession, RPCError.MethodNotExist, text);
            }
        }

        private void OnResponse(Message msg)
        {
            RPCResponseContext responseCallback = null;
            bool isExist = m_responseCallbacks.TryGetValue(msg.RPCSession, out responseCallback);
            if (isExist)
            {
                responseCallback.Callback(responseCallback.Context, msg.Method, msg.Data, RPCError.OK);
                m_responseCallbacks.Remove(msg.RPCSession);
            }
            else
            {
                LoggerHelper.Info(m_serviceAddress, string.Format("Service:{0} session:{1} has not response", m_serviceAddress, msg.RPCSession));
            }
        }

        private void OnError(Message msg)
        {
            NetProtocol instance = NetProtocol.GetInstance();
            int tag = instance.GetTag("Error");
            Error.response sprotoError = (Error.response)instance.Protocol.GenResponse(tag, msg.Data);

            RPCResponseContext responseCallback = null;
            bool isExist = m_responseCallbacks.TryGetValue(msg.RPCSession, out responseCallback);
            if (isExist)
            {
                responseCallback.Callback(responseCallback.Context, msg.Method, Encoding.ASCII.GetBytes(sprotoError.errorText), (RPCError)sprotoError.errorCode);
                m_responseCallbacks.Remove(msg.RPCSession);
            }
            else
            {
                LoggerHelper.Info(m_serviceAddress, string.Format("Service:{0} session:{1} get error:{2}; error text is {3}", 
                    m_serviceAddress, msg.RPCSession, sprotoError.errorCode, sprotoError.errorText));
            }
        }

        protected virtual void OnSocketCommand(Message msg)
        {

        }

        protected virtual void OnTimer(Message msg)
        {
            TimeoutContext context = null;
            bool isExist = m_timeoutCallbacks.TryGetValue(msg.RPCSession, out context);
            if (isExist)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                context.Callback(context.Context, timestamp);

                m_timeoutCallbacks.Remove(msg.RPCSession);
            }
        }

        private void PushToService(int destination, string method, byte[] param, MessageType type, int session)
        {
            Message msg = new Message();
            msg.Source = m_serviceAddress;
            msg.Destination = destination;
            msg.Method = method;
            msg.Data = param;
            msg.RPCSession = session;
            msg.Type = type;

            ServiceContext targetService = ServiceSlots.GetInstance().Get(destination);
            targetService.Push(msg);
        }

        protected void Send(int destination, string method, byte[] param)
        {
            PushToService(destination, method, param, MessageType.ServiceRequest, 0);
        }

        protected void Send(string destination, string method, byte[] param)
        {
            int serviceId = ServiceSlots.GetInstance().Name2Id(destination);
            Send(serviceId, method, param);
        }

        protected void Call(int destination, string method, byte[] param, SSContext context, RPCCallback cb)
        {
            if (m_totalServiceSession >= Int32.MaxValue)
            {
                m_totalServiceSession = 0;
            }

            int session = ++m_totalServiceSession;
            PushToService(destination, method, param, MessageType.ServiceRequest, session);

            RPCResponseContext responseCallback = new RPCResponseContext();
            responseCallback.Context = context;
            responseCallback.Callback = cb;

            m_responseCallbacks.Add(session, responseCallback);
        }

        protected void Call(string destination, string method, byte[] param, SSContext context, RPCCallback cb)
        {
            int serviceId = ServiceSlots.GetInstance().Name2Id(destination);
            Call(serviceId, method, param, context, cb);
        }

        protected void RemoteSend(string remoteNode, string service, string method, byte[] param)
        {
            ClusterClientRequest request = new ClusterClientRequest();
            request.remoteNode = remoteNode;
            request.remoteService = service;
            request.method = method;
            request.param = Convert.ToBase64String(param);

            Call("clusterClient", "Request", request.encode(), null, RemoteSendDummyCallback);
        }

        private void RemoteSendDummyCallback(SSContext context, string method, byte[] param, RPCError error)
        {
            // Nothing to do
        }

        protected void RemoteCall(string remoteNode, string service, string method, byte[] param, SSContext context, RPCCallback cb)
        {
            ClusterClientRequest request = new ClusterClientRequest();
            request.remoteNode = remoteNode;
            request.remoteService = service;
            request.method = method;
            request.param = Convert.ToBase64String(param);

            Call("clusterClient", "Request", request.encode(), context, cb);
        }

        protected void DoResponse(int destination, string method, byte[] param, int session)
        {
            PushToService(destination, method, param, MessageType.ServiceResponse, session);
        }

        protected void DoError(int destination, int session, RPCError errorCode, string errorText)
        {
            Error.response error = new Error.response();
            error.errorCode = (int)errorCode;
            error.errorText = errorText;
            PushToService(destination, "OnError", error.encode(), MessageType.Error, session);
        }

        protected void RegisterServiceMethods(string methodName, Method method)
        {
            m_serviceMethods.Add(methodName, method);
        }

        protected void Timeout(SSContext context, long timeout, TimeoutCallback callback)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (timeout <= 0)
            {
                callback(context, timestamp);
            }
            else
            {
                if (m_totalServiceSession >= Int32.MaxValue)
                {
                    m_totalServiceSession = 0;
                }

                SSTimerNode timerNode = new SSTimerNode();
                timerNode.Opaque = m_serviceAddress;
                timerNode.Session = ++m_totalServiceSession;
                timerNode.TimeoutTimestamp = timestamp + timeout;

                SSTimer.GetInstance().Add(timerNode);

                TimeoutContext timeoutContext = new TimeoutContext();
                timeoutContext.Callback = callback;
                timeoutContext.Context = context;
                m_timeoutCallbacks.Add(timerNode.Session, timeoutContext);
            }
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
                    GlobalMQ.GetInstance().Push(m_serviceAddress);
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
            m_serviceAddress = id;
        }

        public int GetId()
        {
            return m_serviceAddress;
        }
    }
}
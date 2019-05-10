using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Framework.Service.Gateway
{
    class Gateway : ServiceContext
    {
        private Dictionary<string, Method> m_socketMethods = new Dictionary<string, Method>();
        private int m_tcpObjectId = 0;

        public override void Init()
        {
            base.Init();

            RegisterSocketMethods("SocketAccept", SocketAccept);
            RegisterSocketMethods("SocketData", SocketData);
            RegisterSocketMethods("SocketError", SocketError);
        }

        public void SetTCPObjectId(int tcpObjectId)
        {
            m_tcpObjectId = tcpObjectId;
        }

        public int GetTcpObjectId()
        {
            return m_tcpObjectId;
        }

        protected override void OnSocketCommand(Message msg)
        {
            Method method = null;
            bool isExist = m_socketMethods.TryGetValue(msg.Method, out method);
            if (isExist)
            {
                method(msg.Source, msg.RPCSession, msg.Method, msg.Data);
            }
            else
            {
                LoggerHelper.Info(m_serviceAddress, string.Format("unknow method {0}", msg.Method));
            }
        }

        protected virtual void SocketAccept(int source, int session, string method, byte[] param)
        {

        }

        protected virtual void SocketError(int source, int session, string method, byte[] param)
        {

        }

        protected virtual void SocketData(int source, int session, string method, byte[] param)
        {
            
        }

        private void RegisterSocketMethods(string methodName, Method method)
        {
            m_socketMethods.Add(methodName, method);
        }
    }
}

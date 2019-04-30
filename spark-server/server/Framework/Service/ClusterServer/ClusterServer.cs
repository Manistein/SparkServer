using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkServer.Framework.MessageQueue;
using SparkServer.Framework.Utility;
using NetSprotoType;
using static NetProtocol;

namespace SparkServer.Framework.Service.ClusterServer
{
    class ClusterServer : ServiceBase
    {
        private int m_tcpObjectId = 0;
        private SkynetPacketManager m_skynetPacketManager = new SkynetPacketManager();
        Dictionary<string, Method> m_socketMethods = new Dictionary<string, Method>();

        public override void Init()
        {
            base.Init();

            RegisterSocketMethods("SocketAccept", SocketAccept);
            RegisterSocketMethods("SocketError", SocketError);
            RegisterSocketMethods("SocketData", SocketData);
        }

        public void SetTCPObjectId(int tcpObjectId)
        {
            m_tcpObjectId = tcpObjectId;
        }

        protected override void OnSocketCommand(Message msg)
        {
            base.OnSocketCommand(msg);

            Method method = null;
            bool isExist = m_socketMethods.TryGetValue(msg.Method, out method);
            if (isExist)
            {
                method(msg.Source, msg.RPCSession, msg.Method, msg.Data);
            }
            else
            {
                LoggerHelper.Info(m_serviceId, string.Format("Unknow socket command {0}", msg.Method));
            }
        }

        private void SocketAccept(int source, int session, string method, byte[] param)
        {
            NetSprotoType.ClusterServerSocketAccept accept = new NetSprotoType.ClusterServerSocketAccept(param);
            LoggerHelper.Info(m_serviceId, 
                string.Format("ClusterServer accept new connection {ip = {0}, port = {1}, connection = {2}}", accept.ip, accept.port, accept.connection));
        }

        private void SocketError(int source, int session, string method, byte[] param)
        {
            NetSprotoType.SocketError error = new NetSprotoType.SocketError(param);
            LoggerHelper.Info(m_serviceId,
                string.Format("ClusterServer socket error connection:{0} errorCode:{1} errorText:{2}", error.connection, error.errorCode, error.errorText));
        }

        private void SocketData(int source, int session, string method, byte[] param)
        {
            NetSprotoType.SocketData socketData = new NetSprotoType.SocketData(param);
            long connectionId = socketData.connection;
            byte[] tempParam = Encoding.ASCII.GetBytes(socketData.buffer);

            SkynetClusterRequest req = m_skynetPacketManager.UnpackSkynetRequest(tempParam);
            if (req == null)
            {
                return;
            }

            NetProtocol instance = NetProtocol.GetInstance();
            int tag = instance.GetTag("RPC");
            RPCParam sprotoRequest = (RPCParam)instance.Protocol.GenRequest(tag, req.Data);

            SSContext context = new SSContext();
            context.IntegerDict["RemoteSession"] = req.Session;
            context.LongDict["ConnectionId"] = connectionId;

            byte[] targetParam = Encoding.ASCII.GetBytes(sprotoRequest.param);
            Call(req.ServiceName, sprotoRequest.method, targetParam, context, TransferCallback);
        }

        private void TransferCallback(SSContext context, string method, byte[] param, RPCError error)
        {
            if (error == RPCError.OK)
            {
                int tag = NetProtocol.GetInstance().GetTag("RPC");
                RPCParam rpcParam = new RPCParam();
                rpcParam.method = method;
                rpcParam.param = Encoding.ASCII.GetString(param);

                int remoteSession = context.IntegerDict["RemoteSession"];
                long connectionId = context.LongDict["ConnectionId"];

                List<byte[]> bufferList = m_skynetPacketManager.PackSkynetResponse(remoteSession, tag, rpcParam.encode());

                NetworkPacket rpcMessage = new NetworkPacket();
                rpcMessage.Type = SocketMessageType.DATA;
                rpcMessage.TcpObjectId = m_tcpObjectId;
                rpcMessage.Buffers = bufferList;
                rpcMessage.ConnectionId = connectionId;

                NetworkPacketQueue.GetInstance().Push(rpcMessage);
            }
            else
            {
                NetSprotoType.Error.response errorResponse = new NetSprotoType.Error.response(param);
                int remoteSession = context.IntegerDict["RemoteSession"];
                long connectionId = context.LongDict["ConnectionId"];

                List<byte[]> bufferList = m_skynetPacketManager.PackErrorResponse(remoteSession, errorResponse.errorText);

                NetworkPacket rpcMessage = new NetworkPacket();
                rpcMessage.Type = SocketMessageType.DATA;
                rpcMessage.TcpObjectId = m_tcpObjectId;
                rpcMessage.Buffers = bufferList;
                rpcMessage.ConnectionId = connectionId;

                NetworkPacketQueue.GetInstance().Push(rpcMessage);

                LoggerHelper.Info(m_serviceId, 
                    string.Format("Service:ClusterServer Method:TransferCallback errorCode:{0} errorText:{1}", (int)error, Encoding.ASCII.GetString(param)));
            }
        }

        private void RegisterSocketMethods(string methodName, Method method)
        {
            m_socketMethods.Add(methodName, method);
        }
    }
}
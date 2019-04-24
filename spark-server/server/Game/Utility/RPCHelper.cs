using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkServer.Game.Service;
using SparkServer.Game.MessageQueue;

namespace SparkServer.Game.Utility
{
    class RPCHelper
    {
        public static void Send(int serviceId, Message msg)
        {
            ServiceBase targetService = ServiceSlots.GetInstance().Get(serviceId);
            targetService.Push(msg);
        }

        public static void RemoteSend(long sessionId, int protoId, byte[] buffer, int rpcSession)
        {
            NetworkPacketQueue.GetInstance().Push(sessionId, protoId, buffer, rpcSession);
        }
    }
}

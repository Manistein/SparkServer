using NetSprotoType;
using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Game.Process.TestSender
{
    class Sender : ServiceContext
    {
        protected override void Init()
        {
            base.Init();

            Timeout(null, 0, OnSendBattleRequest);
        }

        private void OnSendBattleRequest(SSContext context, long currentTime)
        {
            SendRequest();
            Timeout(null, 1, OnSendBattleRequest);
        }

        private void SendRequest()
        {
            BattleTaskDispatcher_OnBattleRequest request = new BattleTaskDispatcher_OnBattleRequest();
            request.method = "OnBattleRequest";
            request.param = string.Format("TestSender >>>>>>>>>>>>>>>>>>>>>>>>>>>>>Request {0}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            RemoteCall("battlesvr", "BattleTaskDispatcher", "OnBattleRequest", request.encode(), null, SendRequestCallback);
        }

        private void SendRequestCallback(SSContext context, string method, byte[] param, RPCError error)
        {
            if (error == RPCError.OK)
            {
                BattleTaskConsumer_OnBattleRequestResponse response = new BattleTaskConsumer_OnBattleRequestResponse(param);
                LoggerHelper.Info(m_serviceAddress, string.Format("TestSender <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< Response"));
            }
            else
            {
                LoggerHelper.Info(m_serviceAddress, Encoding.ASCII.GetString(param));
            }
        }
    }
}

using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSprotoType;

namespace SparkServer.Game.Service
{
    class BattleTaskDispatcher : ServiceContext
    {
        private int m_index = 0;
        private List<int> m_consumerList = new List<int>();

        protected override void Init()
        {
            base.Init();
            
            for (int i = 0; i < 8; i++)
            {
                int serviceId = SparkServerUtility.NewService("SparkServer.Game.Service.BattleTaskConsumer");
                m_consumerList.Add(serviceId);
            }

            RegisterServiceMethods("OnBattleRequest", OnBattleRequest);
        }

        private void OnBattleRequest(int source, int session, string method, byte[] param)
        {
            if (m_index >= m_consumerList.Count)
            {
                m_index = 0;
            }

            BattleTaskDispatcher_OnBattleRequest dispatcherRequest = new BattleTaskDispatcher_OnBattleRequest(param);
            BattleTaskConsumer_OnBattleRequest consumerRequest = new BattleTaskConsumer_OnBattleRequest();
            consumerRequest.method = "OnBattleRequest";
            consumerRequest.param = dispatcherRequest.param;

            int serviceId = m_consumerList[m_index++];

            SSContext context = new SSContext();
            context.IntegerDict["source"] = source;
            context.IntegerDict["session"] = session;
            Call(serviceId, consumerRequest.method, consumerRequest.encode(), context, OnBattleRequestCallback);
        }

        private void OnBattleRequestCallback(SSContext context, string method, byte[] param, RPCError error)
        {
            int source = context.IntegerDict["source"];
            int session = context.IntegerDict["session"];
            if (error == RPCError.OK)
            {
                BattleTaskConsumer_OnBattleRequestResponse consumerResponse = new BattleTaskConsumer_OnBattleRequestResponse(param);
                BattleTaskDispatcher_OnBattleRequestResponse dispatcherResponse = new BattleTaskDispatcher_OnBattleRequestResponse();
                dispatcherResponse.method = "OnBattleRequest";
                dispatcherResponse.param = consumerResponse.param;

                DoResponse(source, dispatcherResponse.method, dispatcherResponse.encode(), session);
            }
            else
            {
                DoError(source, session, error, Encoding.ASCII.GetString(param));
            }
        }
    }
}
using SparkServer.Framework.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSprotoType;
using SparkServer.Framework.Utility;

namespace SparkServer.Game.Service
{
    class BattleTaskConsumer : ServiceContext
    {
        public override void Init()
        {
            base.Init();

            RegisterServiceMethods("OnBattleRequest", OnBattleRequest);
        }

        private void OnBattleRequest(int source, int session, string method, byte[] param)
        {
            BattleTaskConsumer_OnBattleRequest request = new BattleTaskConsumer_OnBattleRequest(param);

            // TODO Logic
            LoggerHelper.Info(m_serviceAddress, request.param);

            BattleTaskConsumer_OnBattleRequestResponse response = new BattleTaskConsumer_OnBattleRequestResponse();
            response.method = "OnBattleRequest";
            response.param = request.param;
            DoResponse(source, response.method, response.encode(), session);
        }
    }
}

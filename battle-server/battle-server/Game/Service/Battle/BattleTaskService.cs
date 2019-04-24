using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkServer.Game.Service.Logger;
using SparkServer.Game.Utility;

namespace SparkServer.Game.Service.Battle
{
    class BattleTaskService : ServiceBase
    {
        public override void Init(int loggerId)
        {
            base.Init(loggerId);
        }

        public override void Callback(Message msg)
        {
            base.Callback(msg);

            msg.Source = m_serviceId;
            msg.Destination = m_loggerId;
            LoggerHelper.Info(m_loggerId, m_serviceId, msg.ConnectionId, String.Format("protoId:{0} data length:{1}", msg.ProtoId, msg.Data.Length));

            if (msg.RPCSession > 0)
            {
                RPCHelper.RemoteSend(msg.ConnectionId, msg.ProtoId, msg.Data, msg.RPCSession);
            }
        }
    }
}

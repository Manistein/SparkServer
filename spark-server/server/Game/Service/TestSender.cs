using SparkServer.Framework.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSprotoType;

namespace SparkServer.Game.Service
{
    class TestSender : ServiceContext
    {
        protected override void Init()
        {
            base.Init();

            Timeout(null, 300, TimeoutCallback);
        }

        private void TimeoutCallback(SSContext context, long currentTime)
        {
            RemoteSend("battlecli", "battleagent", "test_recv", Encoding.ASCII.GetBytes("Test Send"));
            Timeout(null, 300, TimeoutCallback);
        }
    }
}

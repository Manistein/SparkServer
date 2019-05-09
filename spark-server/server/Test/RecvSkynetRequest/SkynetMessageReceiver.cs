using SparkServer.Framework.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Test.RecvSkynetRequest
{
    class SkynetMessageReceiver : ServiceContext
    {
        public override void Init()
        {
            base.Init();

            RegisterServiceMethods("OnProcessSkynetRequest", OnProcessRequest);
        }

        private void OnProcessRequest(int source, int session, string method, byte[] param)
        {

        }
    }
}

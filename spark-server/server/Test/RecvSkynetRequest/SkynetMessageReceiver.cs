using NetSprotoType;
using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
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

            RegisterServiceMethods("OnProcessRequest", OnProcessRequest);
        }

        private void OnProcessRequest(int source, int session, string method, byte[] param)
        {
            SkynetMessageReceiver_OnProcessRequest request = new SkynetMessageReceiver_OnProcessRequest(param);
            LoggerHelper.Info(m_serviceAddress, string.Format("skynet request_count:{0}", request.request_text));

            if (session > 0)
            {
                SkynetMessageReceiver_OnProcessRequestResponse response = new SkynetMessageReceiver_OnProcessRequestResponse();
                response.request_count = request.request_count;
                response.request_text = request.request_text;

                DoResponse(source, method, response.encode(), session);
            }
        }
    }
}

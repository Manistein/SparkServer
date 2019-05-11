using NetSprotoType;
using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Test.RPC.TestServer
{
    class TestServer : ServiceContext
    {
        public override void Init()
        {
            base.Init();

            RegisterServiceMethods("OnRequest", OnRequest);
        }

        private void OnRequest(int source, int session, string method, byte[] param)
        {
            TestServer_OnRequest request = new TestServer_OnRequest(param);
            LoggerHelper.Info(m_serviceAddress, string.Format("request_time:{0} request_text{1}", request.request_time, request.request_text));

            if (session > 0)
            {
                TestServer_OnRequestResponse response = new TestServer_OnRequestResponse();
                response.response_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                response.response_text = string.Format("{0}:{1}", request.request_text, "response");

                DoResponse(source, method, response.encode(), session);
            }
        }
    }
}

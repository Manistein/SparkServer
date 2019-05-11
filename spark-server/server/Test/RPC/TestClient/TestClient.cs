using NetSprotoType;
using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Test.RPC.TestClient
{
    class TestClient : ServiceContext
    {
        public override void Init()
        {
            base.Init();

            Timeout(null, 1, DoRequest);
        }

        private void DoRequest(SSContext context, long currentTime)
        {
            TestServer_OnRequest request = new TestServer_OnRequest();
            request.request_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            request.request_text = "hello my friend";

            LoggerHelper.Info(m_serviceAddress, string.Format(">>>>>>>>>>>>>>>>Request Call Time:{0} info:{1}", request.request_time, request.request_text));
            RemoteCall("testserver", "RPCTestServer", "OnRequest", request.encode(), null, DoRequestCallback);

            LoggerHelper.Info(m_serviceAddress, string.Format(">>>>>>>>>>>>>>>>Request Send Time:{0} info:{1}", request.request_time, request.request_text));
            RemoteSend("testserver", "RPCTestServer", "OnRequest", request.encode());
        }

        private void DoRequestCallback(SSContext context, string method, byte[] param, RPCError error)
        {
            if (error == RPCError.OK)
            {
                TestServer_OnRequestResponse response = new TestServer_OnRequestResponse(param);
                LoggerHelper.Info(m_serviceAddress, string.Format("<<<<<<<<<<<<<<<<Response OK Time:{0} info:{1}", response.response_time, response.response_text));
                Timeout(null, 10, DoRequest);
            }
            else
            {
                LoggerHelper.Info(m_serviceAddress, string.Format("<<<<<<<<<<<<<<<<Response Error code:{0} error text:{1}", (int)error, Encoding.ASCII.GetString(param)));
            }
        }
    }
}

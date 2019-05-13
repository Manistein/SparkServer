using NetSprotoType;
using SparkServer.Framework.Service;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Test.SendSkynetRequest
{
    class SkynetMessageSender : ServiceContext
    {
        protected override void Init()
        {
            base.Init();
            this.TestSendMsg();
            //RegisterServiceMethods("OnProcessResponse", OnProcessResponse);
        }

        private void OnProcessResponse(SSContext context, string method, byte[] param, RPCError error)
        {
            SkynetMessageSender_OnProcessRequestResponse response = new SkynetMessageSender_OnProcessRequestResponse(param);
            LoggerHelper.Info(m_serviceAddress, string.Format("skynet request_count:{0}", response.request_text));
        }
        
        public void TimeoutCallback(SSContext context, long currentTime)
        {
            SkynetMessageSender_OnProcessRequest request = new SkynetMessageSender_OnProcessRequest();
            request.request_count = 123456;
            request.request_text = "hello skynet";
            RemoteCall("testserver", ".test_send_skynet", "send_skynet", request.encode(), null, OnProcessResponse);

            this.TestSendMsg();
        }

        private void TestSendMsg()
        {
            this.Timeout(new SSContext(), 10, TimeoutCallback);
        }
    }
}

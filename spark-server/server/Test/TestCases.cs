using SparkServer.Framework;
using SparkServer.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Test
{
    delegate void StartupTestCase();

    class TestCases
    {
        private Dictionary<string, StartupTestCase> m_testCaseDict = new Dictionary<string, StartupTestCase>();

        public TestCases()
        {
            RegisterTestCase("RecvSkynetRequest", TestRecvSkynetRequest);

            RegisterTestCase("GatewayCase", GatewayCase);
            RegisterTestCase("GatewayClientCase", GatewayClientCase);

            RegisterTestCase("RPCTestServer", RPCTestServer);
            RegisterTestCase("RPCTestClient", RPCTestClient);
        }

        public void Run(string caseName)
        {
            StartupTestCase startup = null;
            bool isExist = m_testCaseDict.TryGetValue(caseName, out startup);
            if (isExist)
            {
                startup();
            }
        }

        private void RegisterTestCase(string caseName, StartupTestCase startup)
        {
            m_testCaseDict.Add(caseName, startup);
        }

        // Test receive skynet request
        private void TestRecvSkynetRequest()
        {
            BootServices boot = delegate ()
            {
                SparkServerUtility.NewService("SparkServer.Test.RecvSkynetRequest.SkynetMessageReceiver", "RecvSkynetSend");
            };
            Server server = new Server();
            server.Run("../../Test/RecvSkynetRequest/Resource/Config/Startup.json", boot);
        }

        // Gateway Test Case
        private void GatewayCase()
        {
            BootServices boot = delegate ()
            {
            };
            Server server = new Server();
            server.Run("../../Test/Gateway/Resource/Config/Startup.json", boot);
        }

        private void GatewayClientCase()
        {
            SparkServer.Test.Gateway.GatewayClientCase gatewayClient = new SparkServer.Test.Gateway.GatewayClientCase();

            gatewayClient.Run("../../Test/Gateway/Resource/Config/Startup.json");
        }

        // Test RPC
        private void RPCTestServer()
        {
            BootServices boot = delegate ()
            {
                SparkServerUtility.NewService("SparkServer.Test.RPC.TestServer.TestServer", "RPCTestServer");
            };
            Server server = new Server();
            server.Run("../../Test/RPC/Resource/Config/TestServerStartup.json", boot);
        }

        private void RPCTestClient()
        {
            BootServices boot = delegate ()
            {
                SparkServerUtility.NewService("SparkServer.Test.RPC.TestClient.TestClient", "RPCTestClient");
            };
            Server server = new Server();
            server.Run("../../Test/RPC/Resource/Config/TestClientStartup.json", boot);
        }
    }
}

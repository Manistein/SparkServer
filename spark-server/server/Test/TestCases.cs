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

        private void TestRecvSkynetRequest()
        {
            BootServices boot = delegate ()
            {
                SparkServerUtility.NewService("SparkServer.Test.RecvSkynetRequest.SkynetMessageReceiver");
            };
            Server server = new Server();
            server.Run("../../Test/RecvSkynetRequest/Resource/Config/Startup.json", boot);
        }
    }
}

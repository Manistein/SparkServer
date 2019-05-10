using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SparkServer.Network;
using Newtonsoft.Json.Linq;
using SparkServer.Framework.Utility;


namespace SparkServer.Test.Gateway
{
    class UserData
    {
        public long SessionId { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }

    class GatewayClientCase
    {
        private TCPClient m_tcpClient;
        private UserData m_userData;
        private bool m_isConnected;
        private int m_receiveCount = 0;
        private int m_hashCode = 0;

        public void Run(string bootConf)
        {
            string bootConfigText = ConfigHelper.LoadFromFile(bootConf);
            JObject bootConfig = JObject.Parse(bootConfigText);

            m_tcpClient = new TCPClient();
            m_tcpClient.Start(0, OnSessionError, OnReadPacketComplete, OnConnectComplete);

            string gatewayHost = bootConfig["Gateway"]["host"].ToString();
            string[] ipResult = gatewayHost.Split(':');
            string gateIp = ipResult[0];
            Int32 gatePort = Int32.Parse(ipResult[1]);

            m_tcpClient.Connect(gateIp, gatePort);

            m_isConnected = false;

            Console.WriteLine("Start Client... \n Please input message:");

            Random rand = new Random(DateTime.Now.Millisecond);
            int contentSize = rand.Next(5, 15);

            char[] arr = { 'H', 'e', 'l', 'l', 'o' };
            string content = "";
            for (int i = 0; i < contentSize; i++)
            {
                content += arr[i % arr.Count()];
            }
            m_hashCode = content.GetHashCode();

            byte[] buffers = Encoding.ASCII.GetBytes(content);
            
            int increaceIndex = 0;
            while (true)
            {
                if (m_isConnected)
                {
                    Session session = m_tcpClient.GetSessionBy(m_userData.SessionId);
                    if (session != null && (increaceIndex % 5000 == 0))
                    {
                        session.Write(buffers);
                    }
                }

                increaceIndex++;

                m_tcpClient.Loop();
                Thread.Sleep(1);
            }
        }

        private void OnConnectComplete(int opaque, long sessionId, string ip, int port)
        {
            if (m_userData != null)
            {
                Console.WriteLine("sessionId:{0} is already exist", sessionId);
            }
            else
            {
                m_userData = new UserData();
                m_userData.SessionId = sessionId;
                m_userData.IP = ip;
                m_userData.Port = port;

                m_isConnected = true;
               
                Console.WriteLine("new session:{0} connected, ip:{1}, port:{2}", sessionId, ip, port);
            }
        }

        private void OnSessionError(int opaque, long sessionId, int errorCode, string errorText)
        {
            m_isConnected = false;
            Console.WriteLine("OnSessionError sessionId:{0} errorCode:{1} errorText:{2}", sessionId, errorCode, errorText);
        }

        private void OnReadPacketComplete(int opaque, long sessionId, byte[] bytes, int packetSize)
        {
            m_receiveCount++;

            Console.WriteLine("OnReadPacketComplete sessionId:{0} content:{1} hashCode:{2} packetSize:{3} timestamp:{4} receiveCount:{5}", 
                sessionId, 
                Encoding.ASCII.GetString(bytes, 0, packetSize),
                Encoding.ASCII.GetString(bytes, 0, packetSize).GetHashCode() == m_hashCode, 
                packetSize, 
                DateTime.Now, 
                m_receiveCount);
        }
    }
}

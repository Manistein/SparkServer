using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BattleServer.Network;

namespace BattleServer.Examples
{
    class TCPClientExample
    {
        private TCPClient m_tcpClient;
        private UserData m_userData;
        private bool m_isConnected;
        private int m_receiveCount = 0;

        public void Run()
        {
            m_tcpClient = new TCPClient();
            m_tcpClient.Start(OnSessionError, OnReadPacketComplete, OnConnectComplete);
            m_tcpClient.Connect("127.0.0.1", 50001);

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

            int increaceIndex = 0;
            while (true)
            {
                if (m_isConnected)
                {
                    Session session = m_tcpClient.GetSessionBy(m_userData.SessionId);
                    if (session != null && (increaceIndex % 5000 == 0))
                    {
                        session.Write(Encoding.ASCII.GetBytes(content));
                    }
                }

                increaceIndex++;

                m_tcpClient.Loop();
                Thread.Sleep(1);
            }
        }

        private void OnConnectComplete(long sessionId, string ip, int port)
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
               
                Console.WriteLine("new session:{0} accepted, ip:{1}, port:{2}", sessionId, ip, port);
            }
        }

        private void OnSessionError(long sessionId, int errorCode, string errorText)
        {
            m_isConnected = false;
            Console.WriteLine("OnSessionError sessionId:{0} errorCode:{1} errorText:{2}", sessionId, errorCode, errorText);
        }

        private void OnReadPacketComplete(long sessionId, byte[] bytes, int packetSize)
        {
            m_receiveCount++;
            Console.WriteLine("OnReadPacketComplete sessionId:{0} hashCode:{1} content:{2} packetSize:{3} timestamp:{4} receiveCount:{5}", 
                sessionId, 
                Encoding.ASCII.GetString(bytes, 0, packetSize),
                Encoding.ASCII.GetString(bytes, 0, packetSize).GetHashCode(), 
                packetSize, 
                DateTime.Now, 
                m_receiveCount);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BattleServer.Network;

namespace BattleServer.Examples
{
    class UserData
    {
        public long SessionId { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }

    class TCPServerExample
    {
        private TCPServer m_tcpServer;
        private Dictionary<long, UserData> m_userDataDict;
        private int m_receiveCount = 0;

        public void Run()
        {
            string serverIP = "127.0.0.1";
            int port = 50001;
            int backlog = 100;

            m_userDataDict = new Dictionary<long, UserData>();
            m_tcpServer = new TCPServer();
            m_tcpServer.Start(serverIP, port, backlog, OnSessionError, OnReadPacketComplete, OnAcceptComplete);

            Console.WriteLine("Start server bind at {0} {1} backlog is {2}", serverIP, port, backlog);

            while(true)
            {
                m_tcpServer.Loop();
                Thread.Sleep(1);
            }
        }

        private void OnSessionError(long sessionId, int errorCode, string errorText)
        {
            m_userDataDict.Remove(sessionId);
            Console.WriteLine("OnSessionError sessionId:{0} errorCode:{1} errorText:{2}", sessionId, errorCode, errorText);
        }

        private void OnReadPacketComplete(long sessionId, byte[] bytes, int packetSize)
        {
            m_receiveCount++;

            string receiveStr = Encoding.ASCII.GetString(bytes, 0, packetSize);
            Console.WriteLine("OnReadPacketComplete sessionId:{0} hashCode:{1} content:{2} packetSize:{3} timestamp:{4} receiveCount:{5}", 
                sessionId, 
                receiveStr,
                receiveStr.GetHashCode(), 
                packetSize, 
                DateTime.Now, 
                m_receiveCount);

            Session session = m_tcpServer.GetSessionBy(sessionId);
            if (session != null)
            {
                session.Write(Encoding.ASCII.GetBytes(receiveStr));
            }
            else
            {
                Console.WriteLine("OnReadPacketComplete sessionId:{0} not exist", sessionId);
            }
        }

        private void OnAcceptComplete(long sessionId, string ip, int port)
        {
            UserData ud = null;
            bool isSuccess = m_userDataDict.TryGetValue(sessionId, out ud);
            if (isSuccess)
            {
                Console.WriteLine("sessionId:{0} is already exist", sessionId);
            }
            else
            {
                ud = new UserData();
                ud.SessionId = sessionId;
                ud.IP = ip;
                ud.Port = port;
                m_userDataDict.Add(sessionId, ud);

                Console.WriteLine("new session:{0} accepted, ip:{1}, port:{2}", sessionId, ip, port);
            }
        }
    }
}

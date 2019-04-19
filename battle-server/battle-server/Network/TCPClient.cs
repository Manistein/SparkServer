// author:manistein
// since: 2019.03.15
// desc:  TCPClient Module

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace BattleServer.Network
{
    public delegate void ConnectCompleteHandle(long sessionId, string ip, int port);

    public class TCPClient
    {
        // Connect to server sessions
        private int m_totalSessionId = 0;
        private Dictionary<long, Session> m_sessionDict = new Dictionary<long, Session>();

        // Buffer pool for session
        private BufferPool m_bufferPool = new BufferPool();

        // error callback
        private SessionErrorHandle m_onErrorHandle;

        // IO complete callback
        private ReadCompleteHandle m_onReadCompleteHandle;
        private ConnectCompleteHandle m_onConnectCompleteHandle;

        public void Start(SessionErrorHandle errorCallback, ReadCompleteHandle readCallback, ConnectCompleteHandle connectCallback)
        {
            TCPSynchronizeContext.GetInstance();

            m_onErrorHandle = errorCallback;
            m_onReadCompleteHandle = readCallback;
            m_onConnectCompleteHandle = connectCallback;
        }

        public void Stop()
        {
            foreach(KeyValuePair<long, Session> iter in m_sessionDict)
            {
                iter.Value.Stop();
            }
            m_sessionDict.Clear();
        }

        public void Connect(string serverIP, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            UserToken userToken = new UserToken();
            userToken.IP = serverIP;
            userToken.Port = port;

            m_totalSessionId++;
            Session session = new Session();
            session.StartAsClient(socket, m_totalSessionId, m_bufferPool, ipEndPoint, OnSessionError, m_onReadCompleteHandle, m_onConnectCompleteHandle, userToken);
            m_sessionDict.Add(m_totalSessionId, session);
        }

        public Session GetSessionBy(long sessionId)
        {
            Session session = null;
            m_sessionDict.TryGetValue(sessionId, out session);
            return session;
        }

        public void Loop()
        {
            TCPSynchronizeContext.GetInstance().Loop();
        }

        private void OnSessionError(long sessionId, int errorCode, string errorText)
        {
            Session session = null;
            m_sessionDict.TryGetValue(sessionId, out session);
            if (session != null)
            {
                session.Stop();
                m_sessionDict.Remove(sessionId);
            }
            m_onErrorHandle(sessionId, errorCode, "");
        }
    }
}

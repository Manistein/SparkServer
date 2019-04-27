// author:manistein
// since: 2019.03.15
// desc:  TCPServer Module

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SparkServer.Network
{
    public delegate void SessionErrorHandle(int opaque, long sessionId, int errorCode, string errorText);
    public delegate void ReadCompleteHandle(int opaque, long sessionId, byte[] bytes, int packetSize);
    public delegate void AcceptHandle(int opaque, long sessionId, string ip, int port);

    class TCPServer : TCPObject
    {
        private Socket m_listener;
        private string m_bindIP;
        private int m_bindPort;
        private SocketAsyncEventArgs m_acceptEvent = new SocketAsyncEventArgs();

        // client connections
        private long m_totalSessionId = 0;
        private Dictionary<long, Session> m_sessionDict = new Dictionary<long,Session>();

        private BufferPool m_bufferPool = new BufferPool();

        // event handler
        private SessionErrorHandle m_onErrorHandle;
        private ReadCompleteHandle m_onReadCompleteHandle;
        private AcceptHandle m_onAcceptHandle;

        public void Start(string serverIP, 
            int port, 
            int backlog,
            int opaque,
            SessionErrorHandle errorCallback,
            ReadCompleteHandle readCallback,
            AcceptHandle acceptCallback)
        {
            // we should init TCPSynchronizeContext in the TCP thread first
            TCPSynchronizeContext.GetInstance();

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), port);
            m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listener.Bind(ipEndPoint);
            m_listener.Listen(backlog);

            m_opaque = opaque;

            m_acceptEvent.Completed += IO_Complete;

            m_onErrorHandle = errorCallback;
            m_onReadCompleteHandle = readCallback;
            m_onAcceptHandle = acceptCallback;

            m_bindIP = serverIP;
            m_bindPort = port;

            BeginAccept();
        }

        public void Stop()
        {
            m_listener.Close();
            m_acceptEvent.Dispose();

            foreach(KeyValuePair<long, Session> iter in m_sessionDict)
            {
                iter.Value.Stop();
            }
            m_sessionDict.Clear();
        }

        public override void Disconnect(long sessionId)
        {
            Session session = GetSessionBy(sessionId);
            if (session != null)
            {
                session.Stop();
                m_sessionDict.Remove(sessionId);
            }
        }

        public void Loop()
        {
            TCPSynchronizeContext.GetInstance().Loop();
        }

        public override Session GetSessionBy(long sessionId)
        {
            Session session = null;
            m_sessionDict.TryGetValue(sessionId, out session);
            return session;
        }

        private void OnSessionError(int opaque, long sessionId, int errorCode, string errorText)
        {
            Session session = null;
            m_sessionDict.TryGetValue(sessionId, out session);
            if (session != null)
            {
                session.Stop();
                m_sessionDict.Remove(sessionId);
            }
            m_onErrorHandle(opaque, sessionId, errorCode, errorText);
        }

        private void BeginAccept()
        {
            // in order to reuse accept event, we should set AcceptSocket to null
            m_acceptEvent.AcceptSocket = null;
            bool willRaiseEvent = m_listener.AcceptAsync(m_acceptEvent);
            if (!willRaiseEvent)
            {
                OnAccpetComplete(m_acceptEvent);
            }
        }

        private void IO_Complete(object sender, object o)
        {
            SocketAsyncEventArgs asyncEventArgs = o as SocketAsyncEventArgs;
            if (asyncEventArgs.LastOperation == SocketAsyncOperation.Accept)
            {
                TCPSynchronizeContext.GetInstance().Post(OnAccpetComplete, asyncEventArgs);
            }
        }

        private void OnAccpetComplete(object o)
        {
            SocketAsyncEventArgs args = o as SocketAsyncEventArgs;
            if (args.SocketError == SocketError.Success)
            {
                Socket socket = args.AcceptSocket;

                try
                {
                    Session session = new Session();

                    IPEndPoint remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
                    UserToken userToken = new UserToken();
                    userToken.IP = remoteEndPoint.Address.ToString();
                    userToken.Port = remoteEndPoint.Port;

                    m_totalSessionId++;
                    session.StartAsServer(socket, m_opaque, m_totalSessionId, m_bufferPool, OnSessionError, m_onReadCompleteHandle, userToken);
                    m_sessionDict.Add(m_totalSessionId, session);

                    m_onAcceptHandle(m_opaque, m_totalSessionId, userToken.IP, userToken.Port);
                }
                catch(Exception e)
                {
                    m_onErrorHandle(m_opaque, 0, 0, e.ToString());
                }
            }
            else
            {
                m_onErrorHandle(m_opaque, 0, (int)args.SocketError, "");
            }

            BeginAccept();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SparkServer.Network;
using SparkServer.Framework.MessageQueue;
using SparkServer.Framework.Service;
using SparkServer.Framework.Service.Logger;
using SparkServer.Framework.Utility;
using SparkServer.Framework.Service.ClusterServer;
using SparkServer.Framework.Service.ClusterClient;
using NetSprotoType;
using Newtonsoft.Json.Linq;
using SparkServer.Framework.Timer;
using SparkServer.Framework.Service.Gateway;
using System.IO;

namespace SparkServer.Framework
{
    public delegate void BootServices();

    class Server
    {
        JObject m_bootConfig;

        private int m_workerNum = 8;

        private string m_clusterServerIp;
        private int m_clusterServerPort = 0;
        private string m_gateIp;
        private int m_gatePort = 0;

        private TCPServer m_clusterTCPServer;
        private TCPClient m_clusterTCPClient;
        private TCPServer m_tcpGate;
        private TCPObjectContainer m_tcpObjectContainer;

        private Gateway m_gateway;

        private GlobalMQ m_globalMQ;
        private ServiceSlots m_serviceSlots;
        private NetworkPacketQueue m_netpackQueue;
        private SSTimer m_timer;

        public void Run(string bootConf, BootServices customBoot)
        {
            InitConfig(bootConf);
            Boot(customBoot);
            Loop();
        }

        private void InitConfig(string bootConf)
        {
            string bootConfigText = ConfigHelper.LoadFromFile(bootConf);
            m_bootConfig = JObject.Parse(bootConfigText);

            if (m_bootConfig.ContainsKey("ClusterConfig"))
            {
                string clusterNamePath = m_bootConfig["ClusterConfig"].ToString();
                string clusterNameText = ConfigHelper.LoadFromFile(clusterNamePath);
                JObject clusterConfig = JObject.Parse(clusterNameText);

                string clusterName = m_bootConfig["ClusterName"].ToString();
                string ipEndpoint = clusterConfig[clusterName].ToString();

                string[] ipResult = ipEndpoint.Split(':');
                m_clusterServerIp = ipResult[0];
                m_clusterServerPort = Int32.Parse(ipResult[1]);
            }

            if (m_bootConfig.ContainsKey("Gateway"))
            {
                string gatewayEndpoint = m_bootConfig["Gateway"]["Host"].ToString();
                string[] ipResult = gatewayEndpoint.Split(':');
                m_gateIp = ipResult[0];
                m_gatePort = Int32.Parse(ipResult[1]);
            }
        }

        private void InitCluster()
        {
            int clusterServerId = SparkServerUtility.NewService("SparkServer.Framework.Service.ClusterServer.ClusterServer", "clusterServer");
            ClusterServer clusterServer = (ClusterServer)m_serviceSlots.Get(clusterServerId);

            int clusterClientId = SparkServerUtility.NewService("SparkServer.Framework.Service.ClusterClient.ClusterClient", "clusterClient");
            ClusterClient clusterClient = (ClusterClient)m_serviceSlots.Get(clusterClientId);            
            clusterClient.ParseClusterConfig(m_bootConfig["ClusterConfig"].ToString());

            m_clusterTCPServer = new TCPServer();
            m_clusterTCPServer.Start(m_clusterServerIp, m_clusterServerPort, 30, clusterServer.GetId(), OnSessionError, OnReadPacketComplete, OnAcceptComplete);
            m_tcpObjectContainer.Add(m_clusterTCPServer);

            m_clusterTCPClient = new TCPClient();
            m_clusterTCPClient.Start(clusterClient.GetId(), OnSessionError, OnReadPacketComplete, OnConnectedComplete);
            m_tcpObjectContainer.Add(m_clusterTCPClient);

            clusterServer.SetTCPObjectId(m_clusterTCPServer.GetObjectId());
            clusterClient.SetTCPObjectId(m_clusterTCPClient.GetObjectId());
        }

        private void InitGateway()
        {
            string gatewayClass = m_bootConfig["Gateway"]["Class"].ToString();
            string gatewayName = m_bootConfig["Gateway"]["Name"].ToString();

            int gatewayId = SparkServerUtility.NewService(gatewayClass, gatewayName);
            m_gateway = ServiceSlots.GetInstance().Get(gatewayId) as Gateway;

            m_tcpGate = new TCPServer();
            m_tcpGate.Start(m_gateIp, m_gatePort, 30, m_gateway.GetId(), OnSessionError, OnReadPacketComplete, OnAcceptComplete);
            m_tcpObjectContainer.Add(m_tcpGate);

            m_gateway.SetTCPObjectId(m_tcpGate.GetObjectId());
        }

        private void Boot(BootServices customBoot)
        {
            // create global instance first
            m_globalMQ = GlobalMQ.GetInstance();
            m_serviceSlots = ServiceSlots.GetInstance();
            m_netpackQueue = NetworkPacketQueue.GetInstance();
            m_timer = SSTimer.GetInstance();

            NetProtocol.GetInstance();

            // create logger service second
            int loggerId = SparkServerUtility.NewService("SparkServer.Framework.Service.Logger.LoggerService", "logger");
            LoggerService loggerService = (LoggerService)m_serviceSlots.Get(loggerId);

            if (m_bootConfig.ContainsKey("Logger") && Directory.Exists(m_bootConfig["Logger"].ToString()))
            {
                loggerService.Startup(m_bootConfig["Logger"].ToString());
            }
            else
            {
                loggerService.Startup("../");
            }

            m_tcpObjectContainer = new TCPObjectContainer();
            if (m_bootConfig.ContainsKey("ClusterConfig"))
            {
                InitCluster();
            }

            if (m_bootConfig.ContainsKey("Gateway"))
            {
                InitGateway();
            }

            customBoot();

            LoggerHelper.Info(0, "Start SparkServer Server...");

            for (int i = 0; i < m_workerNum; i++)
            {
                Thread thread = new Thread(new ThreadStart(ThreadWorker));
                thread.Start();
            }

            Thread timerThread = new Thread(new ThreadStart(ThreadTimer));
            timerThread.Start();
        }

        private void Loop()
        {
            bool isInitCluster = m_bootConfig.ContainsKey("ClusterConfig");
            bool isInitGateway = m_bootConfig.ContainsKey("Gateway");
            while (true)
            {
                if (isInitCluster)
                {
                    m_clusterTCPServer.Loop();
                    m_clusterTCPClient.Loop();
                }

                if (isInitGateway)
                {
                    m_tcpGate.Loop();
                }

                ProcessOutbound();

                Thread.Sleep(1);
            }
        }

        private void ThreadWorker()
        {
            while(true)
            {
                int serviceId = m_globalMQ.Pop();
                if (serviceId == 0)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    ServiceContext service = m_serviceSlots.Get(serviceId);
                    if (!service.IsInitFinish())
                    {
                        m_globalMQ.Push(service.GetId());
                        continue;
                    }

                    Message msg = service.Pop();
                    if (msg != null)
                    {
                        service.Callback(msg);
                        m_globalMQ.Push(service.GetId());
                    }
                }
            }
        }

        private void ThreadTimer()
        {
            while(true)
            {
                m_timer.Loop();
                Thread.Sleep(1);
            }
        }

        private void OnSessionError(int opaque, long sessionId, int errorCode, string errorText)
        {
            SocketError sprotoSocketError = new SocketError();
            sprotoSocketError.errorCode = errorCode;
            sprotoSocketError.errorText = errorText;
            sprotoSocketError.connection = sessionId;

            Message msg = new Message();
            msg.Source = 0;
            msg.Destination = opaque;
            msg.Method = "SocketError";
            msg.Data = sprotoSocketError.encode();
            msg.RPCSession = 0;
            msg.Type = MessageType.Socket;

            ServiceContext service = ServiceSlots.GetInstance().Get(opaque);
            service.Push(msg);
        }

        private void OnReadPacketComplete(int opaque, long sessionId, byte[] buffer, int packetSize)
        {
            SocketData data = new SocketData();
            data.connection = sessionId;
            data.buffer = Convert.ToBase64String(buffer);

            Message msg = new Message();
            msg.Source = 0;
            msg.Destination = opaque;
            msg.Method = "SocketData";
            msg.Data = data.encode();
            msg.RPCSession = 0;
            msg.Type = MessageType.Socket;

            ServiceContext service = ServiceSlots.GetInstance().Get(opaque);
            service.Push(msg);
        }

        private void OnAcceptComplete(int opaque, long sessionId, string ip, int port)
        {
            SocketAccept accept = new SocketAccept();
            accept.connection = sessionId;
            accept.ip = ip;
            accept.port = port;

            Message msg = new Message();
            msg.Source = 0;
            msg.Destination = opaque;
            msg.Method = "SocketAccept";
            msg.Data = accept.encode();
            msg.RPCSession = 0;
            msg.Type = MessageType.Socket;

            ServiceContext service = ServiceSlots.GetInstance().Get(opaque);
            service.Push(msg);
        }

        private void OnConnectedComplete(int opaque, long sessionId, string ip, int port)
        {
            ClusterClientSocketConnected connected = new ClusterClientSocketConnected();
            connected.connection = sessionId;
            connected.ip = ip;
            connected.port = port;

            Message msg = new Message();
            msg.Source = 0;
            msg.Destination = opaque;
            msg.Method = "SocketConnected";
            msg.Data = connected.encode();
            msg.RPCSession = 0;
            msg.Type = MessageType.Socket;

            ServiceContext service = ServiceSlots.GetInstance().Get(opaque);
            service.Push(msg);
        }

        private void ProcessOutbound()
        {
            while (true)
            {
                SocketMessage socketMessage = m_netpackQueue.Pop();
                if (socketMessage == null)
                    break;

                switch(socketMessage.Type)
                {
                    case SocketMessageType.Connect:
                        {
                            ConnectMessage conn = socketMessage as ConnectMessage;
                            TCPClient tcpClient = (TCPClient)m_tcpObjectContainer.Get(conn.TcpObjectId);
                            tcpClient.Connect(conn.IP, conn.Port);
                        } break;
                    case SocketMessageType.Disconnect:
                        {
                            DisconnectMessage conn = socketMessage as DisconnectMessage;
                            TCPObject tcpObject = m_tcpObjectContainer.Get(conn.TcpObjectId);
                            tcpObject.Disconnect(conn.ConnectionId);
                        } break;
                    case SocketMessageType.DATA:
                        {
                            NetworkPacket netpack = socketMessage as NetworkPacket;
                            TCPObject tcpObject = m_tcpObjectContainer.Get(netpack.TcpObjectId);
                            Session session = tcpObject.GetSessionBy(netpack.ConnectionId);
                            if (session != null)
                            {
                                for (int i = 0; i < netpack.Buffers.Count; i ++)
                                {
                                    session.Write(netpack.Buffers[i]);
                                }
                            }
                            else
                            {
                                OnSessionError(tcpObject.GetOpaque(), netpack.ConnectionId, (int)RPCError.SocketDisconnected, "Connection disconnected");
                            }
                        } break;
                    default: break;
                }
            }
        }
    }
}
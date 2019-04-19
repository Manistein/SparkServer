using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BattleServer.Network;
using BattleServer.Game.MessageQueue;
using BattleServer.Game.Service;
using BattleServer.Game.Service.Logger;
using BattleServer.Game.Service.Battle;
using BattleServer.Game.Utility;

namespace BattleServer.Game
{
    class Server
    {
        private int m_workerNum = 8;
        private int[] m_battleTaskComsumers;
        private int m_handleIndex = 0;
        private int m_loggerId = 0;

        private TCPServer m_tcpServer;
        private TCPClient m_tcpClient;

        private GlobalMQ m_globalMQ;
        private ServiceSlots m_serviceSlots;
        private NetworkPacketQueue m_netpackQueue;
        private SkynetPacketManager m_skynetPacketManager;

        public void Run()
        {
            m_tcpServer = new TCPServer();
            m_tcpServer.Start("127.0.0.1", 8888, 30, OnSessionError, OnReadPacketComplete, OnAcceptComplete);

            // create global instance first
            m_globalMQ = GlobalMQ.GetInstance();
            m_serviceSlots = ServiceSlots.GetInstance();
            m_netpackQueue = NetworkPacketQueue.GetInstance();

            m_battleTaskComsumers = new int[m_workerNum];

            LoggerService loggerService = new LoggerService();
            loggerService.Init(0);
            m_loggerId = m_serviceSlots.Add(loggerService);

            m_skynetPacketManager = new SkynetPacketManager();
            m_skynetPacketManager.Init(m_loggerId, 0);

            LoggerHelper.Info(m_loggerId, 0, 0, "Start Battle Server...");

            for (int i = 0; i < m_workerNum; i ++)
            {
                BattleTaskService battleTaskComsumer = new BattleTaskService();
                battleTaskComsumer.Init(m_loggerId);
                m_battleTaskComsumers[i] = m_serviceSlots.Add(battleTaskComsumer);
                LoggerHelper.Info(m_loggerId, 0, 0, String.Format("index:{0} serviceId:{1}", i, m_battleTaskComsumers[i]));

                Thread thread = new Thread(new ThreadStart(ThreadWorker));
                thread.Start();
            }

            while (true)
            {
                m_tcpServer.Loop();
                ProcessSendBuffer();

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
                    ServiceBase service = m_serviceSlots.Get(serviceId);
                    Message msg = service.Pop();
                    if (msg != null)
                    {
                        service.Callback(msg);
                        m_globalMQ.Push(service.GetId());
                    }
                }
            }
        }

        private void OnSessionError(long sessionId, int errorCode, string errorText)
        {
            LoggerHelper.Info(m_loggerId, 0, sessionId, String.Format("session error: sessionId:{0} errorCode:{1} errorText:{2}", sessionId, errorCode, errorText));
        }

        private void OnReadPacketComplete(long sessionId, byte[] buffer, int packetSize)
        {
            SkynetClusterRequest request = m_skynetPacketManager.UnpackSkynetRequest(buffer);
            if (request == null)
            {
                return;
            }

            if (m_handleIndex >= m_battleTaskComsumers.Length)
            {
                m_handleIndex = 0;
            }

            int serviceId = m_battleTaskComsumers[m_handleIndex];
            BattleTaskService battleTaskComsumer = (BattleTaskService)m_serviceSlots.Get(serviceId);

            Message msg = new Message();
            msg.ProtoId = request.ProtoId;
            msg.Data = request.Data;
            msg.RPCSession = request.Session;
            msg.Source = 0;
            msg.Destination = serviceId;
            msg.ConnectionId = sessionId;
            battleTaskComsumer.Push(msg);

            m_handleIndex++;
        }

        private void OnAcceptComplete(long sessionId, string ip, int port)
        {
            LoggerHelper.Info(m_loggerId, 0, sessionId, String.Format("session accepted: sessionId:{0} ip:{1} port:{2}", sessionId, ip, port));
        }

        private void ProcessSendBuffer()
        {
            while(true)
            {
                NetworkPacket networkPacket = m_netpackQueue.Pop();
                if (networkPacket == null)
                    break;

                Session session = m_tcpServer.GetSessionBy(networkPacket.ConnectionId);
                if (session != null)
                {
                    List<byte[]> bufferList = m_skynetPacketManager.PackSkynetResponse(networkPacket.ConnectionId, 
                        networkPacket.RPCSession, 
                        networkPacket.ProtoId,
                        networkPacket.Buffer);

                    for (int i = 0; i < bufferList.Count; i ++)
                    {
                        session.Write(bufferList[i]);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkServer.Game.Service;
using SparkServer.Game.Service.Logger;

namespace SparkServer.Game.Utility
{
    class LoggerHelper
    {
        public static void Info(int loggerId, int source, long sessionId, string msg)
        {
            LoggerService logger = (LoggerService)ServiceSlots.GetInstance().Get(loggerId);

            Message message = new Message();
            message.Data = Encoding.ASCII.GetBytes(msg);
            message.Destination = loggerId;
            message.Source = source;
            message.ConnectionId = sessionId;
            logger.Push(message);
        }
    }
}

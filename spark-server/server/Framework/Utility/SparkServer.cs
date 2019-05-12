using SparkServer.Framework.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkServer.Framework.Utility
{
    class SparkServerUtility
    {
        public static int NewService(string serviceClass)
        {
            return NewService(serviceClass, "", null);
        }

        public static int NewService(string serviceClass, byte[] param)
        {
            return NewService(serviceClass, "", param);
        }

        public static int NewService(string serviceClass, string serviceName)
        {
            return NewService(serviceClass, serviceName, null);
        }

        public static int NewService(string serviceClass, string serviceName, byte[] param)
        {
            Type type = Type.GetType(serviceClass);
            object obj = Activator.CreateInstance(type);

            ServiceContext service = obj as ServiceContext;
            ServiceSlots.GetInstance().Add(service);

            Message initMsg = new Message();
            initMsg.Source = 0;
            initMsg.Destination = service.GetId();
            initMsg.Method = "Init";
            initMsg.RPCSession = 0;
            initMsg.Data = param;
            initMsg.Type = MessageType.ServiceRequest;

            service.Push(initMsg);

            if (serviceName != "")
            {
                ServiceSlots.GetInstance().Name(service.GetId(), serviceName);
            }

            LoggerHelper.Info(service.GetId(), string.Format("{0} launched", serviceName));

            return service.GetId();
        }
    }
}

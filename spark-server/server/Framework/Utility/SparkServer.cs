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
        public static int NewService(string serviceClass, string serviceName = "")
        {
            Type type = Type.GetType(serviceClass);
            object obj = Activator.CreateInstance(type);

            ServiceBase service = obj as ServiceBase;
            service.Init();

            ServiceSlots.GetInstance().Add(service);
            if (serviceName != "")
            {
                ServiceSlots.GetInstance().Name(service.GetId(), serviceName);
            }

            return service.GetId();
        }
    }
}

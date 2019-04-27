using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace SparkServer.Framework.Service.Logger
{
    class LoggerService : ServiceBase
    {
        private NLog.Logger m_logger; // = LogManager.GetCurrentClassLogger();

        public override void Init()
        {
            // 配置参见 https://github.com/nlog/nlog/wiki/Configuration-API

            base.Init();

            var config = new LoggingConfiguration();

            var logRoot = "../";

            var filePrefix = "battle_1_";

            var fileTarget = new FileTarget("target")
            {
                FileName = logRoot + "logs/${shortdate}/" + filePrefix + "${date:universalTime=false:format=yyyy_MM_dd_HH}.log",
                Layout = "${longdate} ${message}"
            };
            config.AddTarget(fileTarget);

            fileTarget.Layout = @"${longdate} ${message}";

            config.AddRuleForAllLevels(fileTarget);

            LogManager.Configuration = config;

            m_logger = LogManager.GetCurrentClassLogger();
        }

        public override void Callback(Message msg)
        {
            base.Callback(msg);

            string outStr = string.Format("[{0:X8}] {1}", msg.Source, Encoding.ASCII.GetString(msg.Data));
            m_logger.Info(outStr);

            // Console.WriteLine("{0} source serviceId:{1} info:{2}", DateTime.Now, msg.Source, Encoding.ASCII.GetString(msg.Data));
        }
    }
}
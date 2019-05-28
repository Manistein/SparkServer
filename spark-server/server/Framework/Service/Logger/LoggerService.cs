using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSprotoType;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace SparkServer.Framework.Service.Logger
{
    class LoggerService : ServiceContext
    {
        private NLog.Logger m_logger; // = LogManager.GetCurrentClassLogger();

        protected override void Init(byte[] param)
        {
            base.Init();

            Logger_Init loggerInit = new Logger_Init(param);
            Startup(loggerInit.logger_path);

            RegisterServiceMethods("OnLog", OnLog);
        }

        private void Startup(string loggerPath)
        {
            // 配置参见 https://github.com/nlog/nlog/wiki/Configuration-API

            var config = new LoggingConfiguration();

            var logRoot = loggerPath;

            var filePrefix = "log_";

            var fileTarget = new FileTarget("target")
            {
                FileName = logRoot + "logs/${shortdate}/" + filePrefix + "${date:universalTime=false:format=yyyy_MM_dd_HH}.log",
                Layout = "${longdate} ${message}",
                KeepFileOpen = true,
                AutoFlush = true,
            };
            config.AddTarget(fileTarget);

            config.AddRuleForAllLevels(fileTarget);

            LogManager.Configuration = config;

            m_logger = LogManager.GetCurrentClassLogger();
        }

        private void OnLog(int source, int session, string method, byte[] param)
        {
            string outStr = string.Format("[{0:X8}] {1}", source, Encoding.ASCII.GetString(param));
            m_logger.Info(outStr);

            // Console.WriteLine("{0}", outStr);
        }
    }
}
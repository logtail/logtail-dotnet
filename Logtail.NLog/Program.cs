using NLog;
using NLog.Config;
using NLog.Targets;
using System;

namespace Logtail.NLog
{
    class Program
    {
        public static void Main()
        {
            var config = LogManager.Configuration = new LoggingConfiguration();

            config.AddTarget(new ColoredConsoleTarget("console"));
            config.AddTarget(new LogtailTarget("logtail") { SourceToken = "Ho4WpN8evxKTBJUSFSWsQN2y" });

            config.AddRuleForAllLevels("logtail");

            LogManager.Configuration = config;

            var log = LogManager.GetCurrentClassLogger();
            log.Info("NLog");
            log.Warn("NLog");
        }
    }
}

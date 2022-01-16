using System;
using System.Configuration;
using System.IO;
using NLog.Config;
using NLog.Targets;
// ReSharper disable RedundantArgumentDefaultValue

namespace AppUninstallerNetFramework.Helper
{
    public static class AppHelper
    {
        private static string LogBasePath => Directory.GetCurrentDirectory();
        private const string DefaultConsoleTargetName = "console";
        
        public static NLog.Logger ConfigureAndGetLogger()
        {
            NLog.LogManager.Configuration = ConfigureLoggingOptions();
            return NLog.LogManager.GetCurrentClassLogger();
        }

        public static string GetAppDisplayNameForUninstall()
        {
            return ConfigurationManager.AppSettings["AppDisplayName"];
        }
        
        private const string DefaultFileTargetName = "file";
        private static LoggingConfiguration ConfigureLoggingOptions()
        {
            var logConfiguration = new LoggingConfiguration();
            var fileLayout = "${longdate:universalTime=true:format=o}: ${uppercase:${level}} | ${hostname} | ${logger:shortName=True} | ${message} ${exception:format=ToString,Data:exceptionDataSeparator=newline}";
            var consoleLayout = "${longdate:universalTime=true:format=o}: ${uppercase:${level}} | ${logger:shortName=True} | ${message} ${exception:format=ToString,Data:exceptionDataSeparator=newline}";

            var consoleTarget = new ColoredConsoleTarget(DefaultConsoleTargetName)
            {
                Layout = consoleLayout
            };

            var baseLogPath = LogBasePath;
            var now = DateTime.UtcNow.ToString("yyyy.MM.dd_HH.mm.ss");

            var fileTarget = new FileTarget(DefaultFileTargetName)
            {
                Layout = fileLayout,
                FileNameKind = FilePathKind.Absolute,
                FileName = Path.Combine(baseLogPath, "Logs", "${hostname}-" + now, "${uppercase:${level}}.log")
            };

            logConfiguration.LoggingRules.Clear();
            logConfiguration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget, "*");
            logConfiguration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget, "*");
            logConfiguration.AddRuleForAllLevels(fileTarget, "Microsoft*", true);

            return logConfiguration;
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToConstant.Local
// ReSharper disable StringLiteralTypo
// ReSharper disable RedundantArgumentDefaultValue
#pragma warning disable CS0618
#pragma warning disable CS8618

namespace AppUninstaller.Helper;

public static class AppHelper
{
    public static ServiceProvider ServiceProvider { get; set; }
    private static string LogBasePath => Directory.GetCurrentDirectory();
    private const string DefaultConsoleTargetName = "console";
    private const string DefaultFileTargetName = "file";
    
    public static void ConfigureDI()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var factory = provider.GetService<ILoggerFactory>();
        factory.AddNLog();
        factory.ConfigureNLog(ConfigureLoggingOptions());

        ServiceProvider = provider;
    }

    public static string GetAppDisplayNameForUninstall()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: false);

        IConfiguration config = builder.Build();

        var appDisplayName = config.GetSection("AppDisplayName").Value;

        return appDisplayName;
    }

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
using AppUninstaller.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
// ReSharper disable UnusedParameter.Local

namespace AppUninstaller;

public class Program
{
    static void Main(string[] args)
    {
        AppHelper.ConfigureDI();
        var logger = AppHelper.ServiceProvider.GetService<ILogger<Program>>();

        var appDisplayName = AppHelper.GetAppDisplayNameForUninstall();
        if (string.IsNullOrWhiteSpace(appDisplayName))
        {
            logger.LogError("AppDisplayName not found or empty in config.json");
            return;
        }

        logger.LogInformation("Uninstall Flow Started...");
        logger.LogInformation($"Search Uninstall App Display Name: {appDisplayName}");

        if (ApplicationHelper.IsApplicationInstalled(appDisplayName))
        {
            logger.LogInformation("Installed App found!");
            var applicationInfos = ApplicationHelper.GetApplicationInfo(appDisplayName);

            logger.LogInformation("Founded App infos.");
            foreach (var applicationInfo in applicationInfos)
            {
                logger.LogInformation(
                    $"App Info: {JsonConvert.SerializeObject(applicationInfo)}");
            }

            foreach (var applicationInfo in applicationInfos)
            {
                try
                {
                    logger.LogInformation(
                        $"Start Uninstall App for Display Name: {appDisplayName}, Key: {applicationInfo.Key}, UninstallString: {applicationInfo.UninstallString}");
                    ApplicationHelper.UninstallApplication(applicationInfo.UninstallString);
                    logger.LogInformation(
                        $"Uninstall App Completed for Display Name: {appDisplayName}, Key: {applicationInfo.Key}, UninstallString: {applicationInfo.UninstallString}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        $"Uninstall App Error for Display Name: {appDisplayName}, Key: {applicationInfo.Key}, UninstallString: {applicationInfo.UninstallString}");
                }
            }
        }
        else
        {
            logger.LogInformation("Install App  not found!");
        }

        logger.LogInformation($"Uninstall Flow Finished...");
    }
}
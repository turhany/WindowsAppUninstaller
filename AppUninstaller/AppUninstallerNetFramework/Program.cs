using System;
using AppUninstallerNetFramework.Helper;
using Newtonsoft.Json;
// ReSharper disable UnusedParameter.Local

namespace AppUninstallerNetFramework
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var logger = AppHelper.ConfigureAndGetLogger();

            var appDisplayName = AppHelper.GetAppDisplayNameForUninstall();
            if (string.IsNullOrWhiteSpace(appDisplayName))
            {
                logger.Error("AppDisplayName not found or empty in App.config");
                return;
            }

            logger.Info($"Uninstall Flow Started...");
            logger.Info($"Search Uninstall App Display Name: {appDisplayName}");

            if (ApplicationHelper.IsApplicationInstalled(appDisplayName))
            {
                logger.Info("Installed App found!");
                var applicationInfos = ApplicationHelper.GetApplicationInfo(appDisplayName);

                logger.Info("Founded App infos.");
                foreach (var applicationInfo in applicationInfos)
                {
                    logger.Info(
                        $"App Info: {JsonConvert.SerializeObject(applicationInfo)}");
                }

                foreach (var applicationInfo in applicationInfos)
                {
                    try
                    {
                        logger.Info(
                            $"Start Uninstall App for Display Name: {appDisplayName}, Key: {applicationInfo.Key}, UninstallString: {applicationInfo.UninstallString}");
                        ApplicationHelper.UninstallApplication(applicationInfo.UninstallString);
                        logger.Info(
                            $"Uninstall App Completed for Display Name: {appDisplayName}, Key: {applicationInfo.Key}, UninstallString: {applicationInfo.UninstallString}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Uninstall App Error for Display Name: {appDisplayName}, Key: {applicationInfo.Key}, UninstallString: {applicationInfo.UninstallString}");
                    }
                }
            }
            else
            {
                logger.Info("Install App  not found!");
            }
            logger.Info($"Uninstall Flow Finished...");
        }
    }
}
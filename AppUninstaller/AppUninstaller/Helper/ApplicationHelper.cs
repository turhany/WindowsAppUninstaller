using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable CollectionNeverQueried.Global
#pragma warning disable CS8618
#pragma warning disable CS8604
#pragma warning disable CS8602
#pragma warning disable CS8601
#pragma warning disable CS8600

namespace AppUninstaller.Helper;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public static class ApplicationHelper

{
    private const string DisplayNameKey = "DisplayName";
    private const string UninstallStringKey = "UninstallString";

    private const string CurrentUserAppsRegKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    private const string LocalMachineAppsRegKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    private const string LocalMachine64BitAppsRegKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

    public static bool IsApplicationInstalled(string applicationDisplayName)
    {
        string displayName;

        // search in: CurrentUser
        var key = Registry.CurrentUser.OpenSubKey(CurrentUserAppsRegKey);
        if (key != null)
        {
            foreach (var keyName in key.GetSubKeyNames())
            {
                var subKey = key.OpenSubKey(keyName);
                if (subKey == null)
                {
                    continue;
                }

                displayName = subKey.GetValue(DisplayNameKey) as string;
                if (applicationDisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }


        // search in: LocalMachine_32
        key = Registry.LocalMachine.OpenSubKey(LocalMachineAppsRegKey);
        if (key != null)
        {
            foreach (var keyName in key.GetSubKeyNames())
            {
                var subKey = key.OpenSubKey(keyName);
                if (subKey == null)
                {
                    continue;
                }

                displayName = subKey.GetValue(DisplayNameKey) as string;
                if (applicationDisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }


        // search in: LocalMachine_64
        key = Registry.LocalMachine.OpenSubKey(LocalMachine64BitAppsRegKey);
        if (key != null)
        {
            foreach (var keyName in key.GetSubKeyNames())
            {
                var subKey = key.OpenSubKey(keyName);
                if (subKey == null)
                {
                    continue;
                }

                displayName = subKey.GetValue(DisplayNameKey) as string;
                if (applicationDisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        // NOT FOUND
        return false;
    }

    public static List<ApplicationInfo> GetApplicationInfo(string applicationDisplayName)
    {
        var response = new List<ApplicationInfo>();

        // search in: CurrentUser
        var key = Registry.CurrentUser.OpenSubKey(CurrentUserAppsRegKey);
        if (key != null)
        {
            foreach (String keyName in key.GetSubKeyNames())
            {
                if (response.Any(p => p.Key.Equals(keyName)))
                {
                    continue;
                }

                RegistryKey subKey = key.OpenSubKey(keyName);

                if (subKey == null)
                {
                    continue;
                }

                var applicationInfo = new ApplicationInfo
                {
                    Key = keyName,
                    DisplayName = subKey.GetValue(DisplayNameKey) as string,
                    UninstallString = subKey.GetValue(UninstallStringKey) as string
                };

                if (!applicationDisplayName.Equals(applicationInfo.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (subKey.ValueCount == 0)
                {
                    response.Add(applicationInfo);
                    continue;
                }

                foreach (var subKeyValue in subKey.GetValueNames())
                {
                    applicationInfo.KeyValues.Add(subKeyValue, subKey.GetValue(subKeyValue).ToString());
                }

                response.Add(applicationInfo);
            }
        }


        // search in: LocalMachine_32
        key = Registry.LocalMachine.OpenSubKey(LocalMachineAppsRegKey);
        if (key != null)
        {
            foreach (String keyName in key.GetSubKeyNames())
            {
                if (response.Any(p => p.Key.Equals(keyName)))
                {
                    continue;
                }

                RegistryKey subKey = key.OpenSubKey(keyName);

                if (subKey == null)
                {
                    continue;
                }

                var applicationInfo = new ApplicationInfo
                {
                    Key = keyName,
                    DisplayName = subKey.GetValue(DisplayNameKey) as string,
                    UninstallString = subKey.GetValue(UninstallStringKey) as string
                };

                if (!applicationDisplayName.Equals(applicationInfo.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (subKey.ValueCount == 0)
                {
                    response.Add(applicationInfo);
                    continue;
                }

                foreach (var subKeyValue in subKey.GetValueNames())
                {
                    applicationInfo.KeyValues.Add(subKeyValue, subKey.GetValue(subKeyValue).ToString());
                }

                response.Add(applicationInfo);
            }
        }


        // search in: LocalMachine_64
        key = Registry.LocalMachine.OpenSubKey(LocalMachine64BitAppsRegKey);
        if (key != null)
        {
            foreach (String keyName in key.GetSubKeyNames())
            {
                if (response.Any(p => p.Key.Equals(keyName)))
                {
                    continue;
                }

                var subKey = key.OpenSubKey(keyName);

                if (subKey == null)
                {
                    continue;
                }

                var applicationInfo = new ApplicationInfo
                {
                    Key = keyName,
                    DisplayName = subKey.GetValue(DisplayNameKey) as string,
                    UninstallString = subKey.GetValue(UninstallStringKey) as string
                };

                if (!applicationDisplayName.Equals(applicationInfo.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (subKey.ValueCount == 0)
                {
                    response.Add(applicationInfo);
                    continue;
                }

                foreach (var subKeyValue in subKey.GetValueNames())
                {
                    applicationInfo.KeyValues.Add(subKeyValue, subKey.GetValue(subKeyValue).ToString());
                }

                response.Add(applicationInfo);
            }
        }

        return response;
    }

    public static void UninstallApplication(string uninstallString)
    {
        if (string.IsNullOrEmpty(uninstallString))
        {
            throw new ArgumentNullException(nameof(uninstallString));
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();

        uninstallString = uninstallString.Replace(@"""", string.Empty);
        int indexOfExe = uninstallString.IndexOf(".exe");
        
        //Check for executable existence 
        if (indexOfExe > 0)
        {
            //Get exe path 
            string uninstallerPath = uninstallString.Substring(0, indexOfExe + 4);
            startInfo.FileName = uninstallerPath;

            //Check for arguments
            if (uninstallerPath.Length != uninstallString.Length)
            {
                string args = uninstallString.Substring(uninstallerPath.Length);
                if (!string.IsNullOrEmpty(args))
                {
                    /*If not set to false You will get InvalidOperationException :
                     *The Process object must have the UseShellExecute property set to false in order to use environment variables.*/
                    startInfo.UseShellExecute = false;

                    startInfo.Arguments = args.Replace("/I", "/x ") + " /qn";
                }
            }
        }
        //Not tested 
        else
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c " + uninstallString;
        }

        //Start the process 
        Process.Start(startInfo).WaitForExit();
    }

    public class ApplicationInfo
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string UninstallString { get; set; }
        public Dictionary<string, string> KeyValues { get; set; } = new Dictionary<string, string>();
    }
}
using System;
using Microsoft.Win32;
using System.IO;

namespace WcfPad.Analysis.Helpers
{
    /// <summary>
    /// For retrieving paths of various files & folders
    /// </summary>
    public interface IPathHelper
    {
        string GetWcfPadAppDataDirectory();
        string GetSvcutilPath();
    }
    
    public class PathHelper : IPathHelper
    {
        public string GetWcfPadAppDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WcfPad");
        }

        public string GetSvcutilPath()
        {
            var svcutilPath = Path.Combine(GetSvcutilDirectoryFromRegistry(), "svcutil.exe");
            if (!File.Exists(svcutilPath))
            {
                throw new FileNotFoundException($"Could not locate svcutil.exe at {svcutilPath}");
            }

            return svcutilPath;
        }

        private string GetSvcutilDirectoryFromRegistry()
        {
            var isNewFramework = typeof(object).Assembly.GetName().Version > new Version(3, 5);
            var registryPath = isNewFramework 
                ? "SOFTWARE\\Microsoft\\Microsoft SDKs\\NETFXSDK"
                : "SOFTWARE\\Microsoft\\Microsoft SDKs\\Windows";

            var sdkVersion = isNewFramework ? "4.6" : "v8.0A";
            var toolsVersion = isNewFramework ? "WinSDK-NetFx40Tools-x86" : "WinSDK-NetFx35Tools-x86";

            var registryPathWithVersion = $"{registryPath}\\{sdkVersion}\\{toolsVersion}";

            var installationFolder = TryGetValueFromRegistry(registryPathWithVersion, "InstallationFolder");
            if (installationFolder != null)
            {
                return installationFolder;
            }

            var currentInstallFolder = TryGetValueFromRegistry(registryPath, "CurrentInstallFolder");
            if (currentInstallFolder != null)
            {
                return $"{currentInstallFolder}\\Bin";
            }

            throw new Exception("Could not locate Microsoft SDK directory (required since this directory contains svcutil.exe)");
        }

        private string TryGetValueFromRegistry(string registryPath, string name)
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (var subKey = baseKey.OpenSubKey(registryPath))
                    {
                        return subKey.GetValue(name).ToString();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

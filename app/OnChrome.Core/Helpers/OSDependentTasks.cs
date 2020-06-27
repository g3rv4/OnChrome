using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OnChrome.Core.Models;
using OnChrome.Core.Models.Enums;

namespace OnChrome.Core.Helpers
{
    public abstract class OsDependentTasks
    {
        private static OsDependentTasks? _instance;
        private static OsDependentTasks Instance => _instance ??= GetInstance();

        private static OsDependentTasks GetInstance()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsOsTasks();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacOsTasks();
            }

            throw new Exception("Non supported OS");
        }

        public static void OpenChrome(string url, string? profile) =>
            Instance.OpenChromeImpl(url, profile);

        public static (bool, string?) Uninstall() =>
            Instance.UninstallImpl();

        public static RegistrationState GetNativeMessagingState()
        {
            if (!File.Exists(Instance.ManifestPath))
            {
                return RegistrationState.Unregistered;
            }

            var manifest = AppManifest.FromJson(File.ReadAllText(Instance.ManifestPath));
            if (manifest?.Path != PathToExecutable)
            {
                return RegistrationState.RegisteredDifferentHandler;
            }

            return RegistrationState.Registered;
        }

        public static void SetupNativeMessaging()
        {
            var manifestDirectory = Path.GetDirectoryName(Instance.ManifestPath);
            if (!Directory.Exists(manifestDirectory))
            {
                Directory.CreateDirectory(manifestDirectory);
            }

            File.WriteAllText(Instance.ManifestPath, AppManifest.Instance.ToJson());

            Instance.FinishSettingUpNativeMessaging();
        }

        public static void UnregisterNativeMessaging()
        {
            if (File.Exists(Instance.ManifestPath))
            {
                File.Delete(Instance.ManifestPath);
            }

            Instance.FinishUnregisteringNativeMessaging();
        }
        
        public static string? PathToExecutable =>
            Instance.GetExecutablePathFromAssemblyLocation(Assembly.GetEntryAssembly()?.Location);

        protected abstract string ManifestPath { get; }

        protected abstract void OpenChromeImpl(string url, string? profile);

        protected abstract (bool, string?) UninstallImpl();

        protected abstract string? GetExecutablePathFromAssemblyLocation(string? assemblyLocation);

        protected virtual void FinishSettingUpNativeMessaging() { }

        protected virtual void FinishUnregisteringNativeMessaging() { }
    }
}

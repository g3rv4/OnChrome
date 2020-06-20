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

        public static Task OpenChromeAsync(string url, string? profile) =>
            Instance.OpenChromeAsyncImpl(url, profile);

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
        }

        public static void UnregisterNativeMessaging()
        {
            if (File.Exists(Instance.ManifestPath))
            {
                File.Delete(Instance.ManifestPath);
            }
        }
        
        public static string? PathToExecutable =>
            Instance.GetExecutablePathFromAssemblyLocation(Assembly.GetEntryAssembly()?.Location);

        protected abstract string ManifestPath { get; }

        protected abstract Task OpenChromeAsyncImpl(string url, string? profile);

        protected abstract string? GetExecutablePathFromAssemblyLocation(string? assemblyLocation);
    }
}

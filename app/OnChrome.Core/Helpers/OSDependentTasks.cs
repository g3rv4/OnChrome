using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OnChrome.Core.Helpers
{
    public abstract class OsDependentTasks
    {
        private static OsDependentTasks? _instance;
        private static OsDependentTasks Instance => _instance ??= GetInstance();

        public static Task OpenChromeAsync(string url, string? profile) =>
            Instance.OpenChromeAsyncImpl(url, profile);

        public static Task OpenFirefoxOnWebappAsync() =>
            Instance.OpenFirefoxOnWebappAsyncImpl();

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

        protected abstract Task OpenChromeAsyncImpl(string url, string? profile);

        protected abstract Task OpenFirefoxOnWebappAsyncImpl();
    }
}

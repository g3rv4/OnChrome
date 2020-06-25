using CliWrap;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OnChrome.Core.Helpers
{
    internal class WindowsOsTasks : OsDependentTasks
    {
        protected override string ManifestPath
        {
            get
            {
                var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
                if (directory == null)
                    throw new Exception("Could not get the directory of the entry assembly");

                return Path.Combine(directory, "me.onchro.netcore.json");
            }
        }

        protected override async Task OpenChromeAsyncImpl(string url, string? profile)
        {
            var registryValue = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe").GetValue(null);
            if (registryValue is string pathToChrome && pathToChrome.HasValue())
            {
                await Cli.Wrap(pathToChrome)
                    .WithArguments(url)
                    .ExecuteAsync();
            }
            else
            {
                throw new Exception("Could not get the path to chrome");
            }
        }

        protected override Task<(bool, string?)> UninstallAsyncImpl() =>
            Task.FromResult<(bool, string?)>((false, "Please uninstall it from the control panel"));

        protected override string? GetExecutablePathFromAssemblyLocation(string? assemblyLocation) =>
            assemblyLocation?.Replace(".dll", ".exe");

        protected override void FinishSettingUpNativeMessaging()
        {
            var key = Registry.CurrentUser.CreateSubKey(@"Software\Mozilla\NativeMessagingHosts\me.onchro.netcore");
            key.SetValue(null, ManifestPath);
        }

        protected override void FinishUnregisteringNativeMessaging()
        {
            Registry.CurrentUser.DeleteSubKey(@"Software\Mozilla\NativeMessagingHosts\me.onchro.netcore");
        }
    }
}

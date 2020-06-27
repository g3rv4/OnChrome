using System;
using System.Diagnostics;
using System.IO;

namespace OnChrome.Core.Helpers
{
    internal class MacOsTasks : OsDependentTasks
    {
        private string? _manifestPath;
        protected override string ManifestPath => _manifestPath ??= GetManifestPath();

        private string GetManifestPath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library/Application Support/Mozilla/NativeMessagingHosts/me.onchro.netcore.json");
        }

        protected override void OpenChromeImpl(string url, string? profile)
        {
            var arguments = @"-a ""Google Chrome"" ";
            if (profile.HasValue())
            {
                arguments += $@"-n --args --profile-directory=""{profile}"" ";
            }

            arguments += url;

            Process.Start("open", arguments);
        }

        protected override (bool, string?) UninstallImpl()
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (directory == null)
                throw new Exception("Could not get the directory of the entry assembly");

            var uninstallerPath = Path.Combine(directory, "OnChrome.Uninstall.pkg");
            
            if (!File.Exists(uninstallerPath))
                return (false, "Could not find uninstall package at " + directory);
            
            Process.Start("open", uninstallerPath);
            return (true, null);
        }

        protected override string? GetExecutablePathFromAssemblyLocation(string? assemblyLocation) =>
            assemblyLocation?.Replace(".dll", "");
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using CliWrap;

namespace OnChrome.Core.Helpers
{
    internal class MacOsTasks : OsDependentTasks
    {
        private string? _manifestPath;
        protected override string ManifestPath => _manifestPath ??= GetManifestPath();

        private string GetManifestPath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library/Application Support/Mozilla/NativeMessagingHosts/me.onchro.json");
        }

        protected override async Task OpenChromeAsyncImpl(string url, string? profile)
        {
            var arguments = @"-a ""Google Chrome"" ";
            if (profile.HasValue())
            {
                arguments += $@"-n --args --profile-directory=""{profile}"" ";
            }

            arguments += url;

            await Cli.Wrap("open")
                .WithArguments(arguments)
                .ExecuteAsync();
        }

        protected override string? GetExecutablePathFromAssemblyLocation(string? assemblyLocation) =>
            assemblyLocation?.Replace(".dll", "");
    }
}

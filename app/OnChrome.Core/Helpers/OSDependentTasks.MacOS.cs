using System.Threading.Tasks;
using CliWrap;

namespace OnChrome.Core.Helpers
{
    internal class MacOsTasks : OsDependentTasks
    {
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
    }
}

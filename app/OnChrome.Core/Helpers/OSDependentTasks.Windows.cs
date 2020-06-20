using System;
using System.Threading.Tasks;

namespace OnChrome.Core.Helpers
{
    internal class WindowsOsTasks : OsDependentTasks
    {
        protected override string ManifestPath { get; }

        protected override Task OpenChromeAsyncImpl(string url, string? profile)
        {
            throw new NotImplementedException();
        }

        protected override string? GetExecutablePathFromAssemblyLocation(string? assemblyLocation)
        {
            throw new NotImplementedException();
        }
    }
}

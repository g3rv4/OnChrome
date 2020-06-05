using System;
using System.Threading.Tasks;

namespace OnChrome.Core.Helpers
{
    internal class WindowsOsTasks : OsDependentTasks
    {
        protected override Task OpenChromeAsyncImpl(string url, string? profile)
        {
            throw new NotImplementedException();
        }
    }
}

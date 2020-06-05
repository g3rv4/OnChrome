using System;
using System.Threading.Tasks;
using OnChrome.Core.Helpers;
using OnChrome.Core.Models;

namespace OnChrome.Core.Actions
{
    public static class OpenChromeAction
    {
        public static async Task<BaseNMResponse> ProcessAsync(OpenChromeRequest request)
        {
            try
            {
                await OsDependentTasks.OpenChromeAsync(request.Url, request.Profile);
                return new OpenChromeResponse();
            }
            catch (Exception e)
            {
                return new FailedResponse($"{e.GetType()}: {e.Message}");
            }
        }
    }
}

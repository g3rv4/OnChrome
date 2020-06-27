using System;
using OnChrome.Core.Helpers;
using OnChrome.Core.Models;

namespace OnChrome.Core.Actions
{
    public static class OpenChromeAction
    {
        public static BaseNMResponse Process(OpenChromeRequest request)
        {
            try
            {
                OsDependentTasks.OpenChrome(request.Url, request.Profile);
                return new OpenChromeResponse();
            }
            catch (Exception e)
            {
                return new FailedResponse($"{e.GetType()}: {e.Message}");
            }
        }
    }
}

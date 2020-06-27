using System;
using System.Reflection;
using OnChrome.Core.Helpers;
using OnChrome.Core.Models;

namespace OnChrome.Core.Actions
{
    public static class CompatibilityAction
    {
        public static BaseNMResponse Process(CompatibilityRequest request)
        {
            try
            {
                var status = ExtensionVersionHelper.GetCompatibiltyStatus(request.ExtensionVersion, out var appVersion);
                return new CompatibilityResponse(appVersion, status);
            }
            catch (Exception e)
            {
                return new FailedResponse(e.Message);
            }
        }
    }
}

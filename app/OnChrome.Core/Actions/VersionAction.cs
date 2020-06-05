using System;
using System.Reflection;
using OnChrome.Core.Models;

namespace OnChrome.Core.Actions
{
    public static class VersionAction
    {
        public static BaseNMResponse Process(VersionRequest request)
        {
            var appVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            if (appVersion == null)
            {
                return new FailedResponse("Could not determine the app version");
            }

            if (!Version.TryParse(request.ExtensionVersion, out var extensionVersion))
            {
                return new FailedResponse("Could not parse the received extension version");
            }

            return new VersionResponse(appVersion.ToString(), isCompatible: appVersion.Major == extensionVersion.Major);
        }
    }
}

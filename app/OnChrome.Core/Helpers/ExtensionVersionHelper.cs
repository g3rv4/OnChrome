using System;
using System.Reflection;
using System.Text.RegularExpressions;
using OnChrome.Core.Models.Enums;

namespace OnChrome.Core.Helpers
{
    public static class ExtensionVersionHelper
    {
        private static Regex _versionWithoutRevision = new Regex(@"^([0-9]+\.[0-9]+\.[0-9]+)", RegexOptions.Compiled);
        public static CompatibilityStatus GetCompatibiltyStatus(string? extensionVersion, out string appVersion)
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version == null)
            {
                throw new Exception("Could not determine the app version");
            }
            
            appVersion = _versionWithoutRevision.Match(version.ToString()).Groups[1].Value;
            if (extensionVersion.IsNullOrEmpty())
            {
                return CompatibilityStatus.MissingExtension;
            }

            if (!Version.TryParse(extensionVersion, out var extVersion))
            {
                throw new Exception("Could not parse the received extension version");
            }

            if (version.Major > extVersion.Major)
            {
                return CompatibilityStatus.ExtensionNeedsUpdate;
            }
            else if (version.Major < extVersion.Major)
            {
                return CompatibilityStatus.AppNeedsUpdate;
            }

            return CompatibilityStatus.Ok;
        }
    }
}

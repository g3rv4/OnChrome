using System;

namespace OnChrome.Core.Models.Enums
{
    public enum CompatibilityStatus
    {
        Unknown,
        MissingExtension,
        AppNeedsUpdate,
        ExtensionNeedsUpdate,
        Ok
    }

    public static class CompatibilityStatusExtensions
    {
        public static bool IsAppOk(this CompatibilityStatus status) =>
            status != CompatibilityStatus.AppNeedsUpdate &&
            status != CompatibilityStatus.Unknown;

        public static bool IsExtensionOk(this CompatibilityStatus status) =>
            status == CompatibilityStatus.Ok ||
            status == CompatibilityStatus.AppNeedsUpdate;
        
        public static string GetAppDetails(this CompatibilityStatus status)
        {
            switch (status)
            {
                case CompatibilityStatus.MissingExtension:
                case CompatibilityStatus.Ok:
                case CompatibilityStatus.ExtensionNeedsUpdate:
                    return "You are running the application successfully.";
                case CompatibilityStatus.AppNeedsUpdate:
                    return "You need to update the application.";
                default:
                    return "Could not determine app status.";
            }
        }

        public static string GetExtensionDetails(this CompatibilityStatus status)
        {
            switch (status)
            {
                case CompatibilityStatus.AppNeedsUpdate:
                case CompatibilityStatus.Ok:
                    return "You are running the extension successfully.";
                case CompatibilityStatus.MissingExtension:
                    return "You need to install the extension.";
                case CompatibilityStatus.ExtensionNeedsUpdate:
                    return "You need to update the extension.";
                default:
                    return "Could not determine extension status.";
            }
        }
    }
}

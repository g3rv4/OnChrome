using OnChrome.Core.Models.Enums;

namespace OnChrome.Web.Models
{
    public class IndexViewModel
    {
        public RegistrationState NativeMessagingState { get; set; }
        public string AppVersion { get; set; }
        public string? ExtensionVersion { get; set; }
        public CompatibilityStatus CompatibilityStatus { get; set; }
    }
}

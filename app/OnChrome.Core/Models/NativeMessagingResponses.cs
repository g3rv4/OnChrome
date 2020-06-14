using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OnChrome.Core.Models.Enums;

namespace OnChrome.Core.Models
{
    public abstract class BaseNMResponse
    {
        public bool Success { get; }

        protected BaseNMResponse(bool success)
        {
            Success = success;
        }
    }

    public class FailedResponse : BaseNMResponse
    {
        public string ErrorMessage { get; }

        public FailedResponse(string errorMessage) : base(success: false)
        {
            ErrorMessage = errorMessage;
        }
    }

    public class CompatibilityResponse : BaseNMResponse
    {
        public string? AppVersion { get; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public CompatibilityStatus CompatibilityStatus { get; }

        public CompatibilityResponse(string? appVersion, CompatibilityStatus compatibilityStatus) : base(success: true)
        {
            AppVersion = appVersion;
            CompatibilityStatus = compatibilityStatus;
        }
    }

    public class OpenChromeResponse : BaseNMResponse
    {
        public OpenChromeResponse() : base(success: true)
        {
        }
    }
}

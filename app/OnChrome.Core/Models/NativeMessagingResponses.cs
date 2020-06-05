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

    public class VersionResponse : BaseNMResponse
    {
        public string? AppVersion { get; }
        public bool IsCompatible { get; }

        public VersionResponse(string? appVersion, bool isCompatible) : base(success: true)
        {
            AppVersion = appVersion;
            IsCompatible = isCompatible;
        }
    }

    public class OpenChromeResponse : BaseNMResponse
    {
        public OpenChromeResponse() : base(success: true)
        {
        }
    }
}

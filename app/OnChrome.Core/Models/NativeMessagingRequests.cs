using Newtonsoft.Json;
using OnChrome.Core.Helpers;

namespace OnChrome.Core.Models
{
    public abstract class BaseNMRequest
    {
        public string Command { get; set; }

        public static BaseNMRequest? Deserialize(string json) =>
            (BaseNMRequest?)JsonConvert.DeserializeObject(json, typeof(BaseNMRequest), IncomingMessageConverter.Instance);
    }

    public class VersionRequest : BaseNMRequest
    {
        public string ExtensionVersion { get; set; }
    }

    public class OpenChromeRequest : BaseNMRequest
    {
        public string Url { get; set; }
        public string? Profile { get; set; } 
    }
}

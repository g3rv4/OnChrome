using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OnChrome.Core.Helpers;

namespace OnChrome.Core.Models
{
    public class AppManifest
    {
        private static AppManifest? _instance;
        public static AppManifest Instance => _instance ??= new AppManifest
        (
            name: "me.onchro.netcore",
            description: "Extension to open certain urls on chrome. Visit onchrome.gervas.io for details",
            path: OsDependentTasks.PathToExecutable ?? "",
            type: "stdio",
            allowedExtensions: new [] { "onchrome@gervas.io" }
        );

        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public string ToJson() =>
            JsonConvert.SerializeObject(this, _jsonSettings);

        public static AppManifest? FromJson(string json) =>
            JsonConvert.DeserializeObject<AppManifest>(json, _jsonSettings);

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Path { get; private set; }
        public string Type { get; private set; }
        public string[] AllowedExtensions { get; private set; }

        [JsonConstructor]
        private AppManifest(string name, string description, string path, string type, string[] allowedExtensions)
        {
            Name = name;
            Description = description;
            Path = path;
            Type = type;
            AllowedExtensions = allowedExtensions;
        }
    }
}

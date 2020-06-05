using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OnChrome.Core.Models;

namespace OnChrome.Core.Helpers
{
    public class IncomingMessageConverter : JsonConverter
    {
        private static IncomingMessageConverter? _instance;
        public static IncomingMessageConverter Instance => _instance ??= new IncomingMessageConverter();
        
        private static Dictionary<string, Func<BaseNMRequest>> _factories =
            new Dictionary<string, Func<BaseNMRequest>>
            {
                ["version"] = () => new VersionRequest(),
                ["open"] = () => new OpenChromeRequest(),
            };
        
        public override bool CanConvert(Type objectType) => objectType == typeof(BaseNMRequest);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
            serializer.Serialize(writer, value);
        
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (jObject.TryGetValue("command", out var typeToken) &&
                typeToken.Type == JTokenType.String &&
                _factories.TryGetValue(typeToken.Value<string>(), out var instanceFactory))
            {
                existingValue = instanceFactory();
                using var innerReader = jObject.CreateReader();
                serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
                serializer.Populate(innerReader, existingValue);
                return existingValue;
            }

            return existingValue;
        }
    }
}

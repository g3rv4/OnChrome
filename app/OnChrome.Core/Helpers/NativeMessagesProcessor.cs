using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OnChrome.Core.Actions;
using OnChrome.Core.Models;

namespace OnChrome.Core.Helpers
{
    public static class NativeMessagesProcessor
    {
        public static async Task ProcessAsync()
        {
            var request = GetNativeMessagingContent();
            
            var response = request switch
            {
                VersionRequest m => VersionAction.Process(m),
                OpenChromeRequest m => await OpenChromeAction.ProcessAsync(m),
                _ => new FailedResponse("Could not process the incoming message"),
            };
            
            SendNativeMessagingResponse(response);
        }

        private static BaseNMRequest? GetNativeMessagingContent()
        {
            using var stdin = Console.OpenStandardInput();
            var lengthBytes = stdin.ReadExactly(4);
            var length = BitConverter.ToInt32(lengthBytes);

            var messageJson = System.Text.Encoding.UTF8.GetString(stdin.ReadExactly(length));
            return BaseNMRequest.Deserialize(messageJson); 
        }

        private static void SendNativeMessagingResponse(BaseNMResponse response)
        {
            using var stdout = Console.OpenStandardOutput();
            
            var responseJson = JsonConvert.SerializeObject(response, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseJson);
            var lengthBytes = BitConverter.GetBytes(responseBytes.Length);
            
            var isBigEndian = !BitConverter.IsLittleEndian;
            if (isBigEndian)
            {
                Array.Reverse(lengthBytes);
            }

            stdout.Write(lengthBytes);
            stdout.Write(responseBytes);
            stdout.Flush();
        }
    }
}

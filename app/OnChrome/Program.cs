using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OnChrome.Core.Helpers;

namespace OnChrome
{
    class Program
    {
        private static string[] _validCommands = new[] {"register", "unregister"};
        static async Task Main(string[] args)
        {
            var command = args.Length > 0 && _validCommands.Contains(args[1])
                ? args[1]
                : null;
            
            if (command == null && Console.IsInputRedirected)
            {
                // when this happens, it's because Firefox is sending us a native message.
#if DEBUG
                // useful to be able to attach a debugger to the process and see what's going on
                await Task.Delay(10000);
#endif
                await NativeMessagesProcessor.ProcessAsync();
            }
            else
            {
                switch (command)
                {
                    case "register":
                        OsDependentTasks.SetupNativeMessaging();
                        break;
                    case "unregister":
                        OsDependentTasks.UnregisterNativeMessaging();
                        break;
                    default:
                        Console.WriteLine("OnChrome v" + Assembly.GetEntryAssembly()?.GetName().Version);
                        Console.WriteLine("Supported commands: " + String.Join(',', _validCommands));
                        break;
                }
            }
        }
    }
}

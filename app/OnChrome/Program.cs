using System;
using System.Threading.Tasks;
using OnChrome.Core.Helpers;

namespace OnChrome
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (Console.IsInputRedirected)
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
                var command = "register";
                if (args.Length >= 1)
                {
                    command = args[0];
                }

                switch (command)
                {
                    case "register":
                        OsDependentTasks.SetupNativeMessaging();
                        break;
                    case "unregister":
                        OsDependentTasks.UnregisterNativeMessaging();
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }
    }
}

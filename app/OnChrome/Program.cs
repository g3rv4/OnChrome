using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using OnChrome.Core.Helpers;

namespace OnChrome
{
    class Program
    {
        private static string[] _validCommands = new[] {"register", "unregister", "uninstall"};
        static void Main(string[] args)
        {
            var command = args.Length > 0 && _validCommands.Contains(args[0])
                ? args[0]
                : null;

            if (command == null && Console.IsInputRedirected)
            {
                // when this happens, it's because Firefox is sending us a native message.
#if DEBUG
                // useful to be able to attach a debugger to the process and see what's going on
                Thread.Sleep(10000);
#endif
                NativeMessagesProcessor.Process();
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
                    case "uninstall":
                        var (success, message) = OsDependentTasks.Uninstall();
                        if (!success)
                        {
                            Console.WriteLine("Failure: " + message);
                        }
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

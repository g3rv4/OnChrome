using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnChrome.Core.Helpers;
using OnChrome.Core.Models.Enums;

namespace OnChrome.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
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
                var nativeMessagingState = OsDependentTasks.GetNativeMessagingState();
                if (nativeMessagingState != RegistrationState.Registered)
                {
                    OsDependentTasks.SetupNativeMessaging();
                }
                
                var host = CreateHostBuilder(args).Build();
                var life = host.Services.GetRequiredService<IHostApplicationLifetime>();
                life.ApplicationStarted.Register(() => {
                    OsDependentTasks.OpenFirefoxOnWebappAsync().GetAwaiter().GetResult();
                });
                await host.RunAsync();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseUrls("http://127.0.0.1:12346");
                });
    }
}

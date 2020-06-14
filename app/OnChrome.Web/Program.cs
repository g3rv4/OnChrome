using System;
using System.Net.Http;
using System.Threading;
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
        public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        
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

                // is there another version running? if so, kill it! this will make upgrading easier
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(200);
                    try
                    {
                        var response = await client.GetAsync("http://localhost:12346/shutdown");
                        if (response.IsSuccessStatusCode)
                        {
                            // give it time to shut down
                            await Task.Delay(1000);
                        }
                    }
                    catch {}
                }
                
                await host.RunAsync(CancellationTokenSource.Token);
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

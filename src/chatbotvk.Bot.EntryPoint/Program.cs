using chatbotvk.Bot.Core;
using chatbotvk.Bot.Core.Contracts;
using chatbotvk.Core.Services.External;
using chatbotvk.Services.Bank;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http;
using VkNet;
using VkNet.Abstractions;

namespace chatbotvk.Bot.EntryPoint
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<Microsoft.Extensions.Logging.ILogger, NullLogger>();

                    services.AddTransient<HttpClient>();

                    services.AddSingleton<IVkApi, VkApi>(
                        implementationFactory: (provider) =>
                        {
                            return new VkApi(services)
                            {
                                RequestsPerSecond = 20
                            };
                        });

                    services.AddSingleton<IVkBotManager, VkBotManager>();
                    services.AddTransient<IExchangeRateService, ExchangeRateService>();

                    services.AddSingleton<Bot>();
                })
                .Build();

            Bot bot = ActivatorUtilities.CreateInstance<Bot>(host.Services);
            bot.Start();
        }
    }
}

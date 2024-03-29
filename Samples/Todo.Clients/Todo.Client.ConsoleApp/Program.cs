﻿using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Components.Users;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Todo.Client.ConsoleApp
{

    internal class Program
    {
        public const string SITE_TODO_SERVER_MAIN = "Todo.Server.Main";
        public const string SITE_TODO_SERVER_MAIN_BASE_URL = "https://localhost:7157/api/";

        static async Task Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();

            services.AddOptions();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddDebug();
                loggingBuilder.AddConsole();
            });

            Configure(services);

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IConsoleInitializeService>().Initialize();

            await serviceProvider.GetRequiredService<TaskExecutor>().RunDownAsync();

            Console.ReadLine();
        }

        private static void Configure(ServiceCollection services)
        {
            services.AddFullStackClient(
                clientOptions =>
                {
                },
                fileManagerOptions =>
                {
                },
                apiClientOptions =>
                {
                    apiClientOptions.HttpClientTimeout = TimeSpan.FromMinutes(15);

                    apiClientOptions.TokenSiteSetting = new SiteSetting
                    {
                        SiteName = "TokenSite",
                        BaseUrl = new Uri(SITE_TODO_SERVER_MAIN_BASE_URL)
                    };
                });
            
            AddConsoleService(services);
        }

        private static void AddConsoleService(ServiceCollection services)
        {
            services.AddSingleton<ITokenPreferences, ConsolePreferenceProvider>();

            services.AddSingleton<IConsoleInitializeService>(new ConsoleInitializeService());

            services.AddSingleton<TaskExecutor>();
        }
    }
}
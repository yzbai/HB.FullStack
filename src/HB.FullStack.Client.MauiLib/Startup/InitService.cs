using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;
using HB.FullStack.Client.Services.Sync;
using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Hosting;

namespace HB.FullStack.Client.MauiLib.Startup
{
    public class InitOptions : IOptions<InitOptions>
    {
        public InitOptions Value => this;

    }

    public class InitService : IMauiInitializeService
    {
        private readonly InitOptions _options;

        public InitService(IOptions<InitOptions> options)
        {
            _options = options.Value;
        }

        public void Initialize(IServiceProvider services)
        {
            InitLog(services);

            InitService.InitDatabase(services.GetRequiredService<IDatabase>());

            InitSomeServices(services);
        }

        private static void InitLog(IServiceProvider services)
        {
            Globals.Logger = services.GetRequiredService<ILogger<InitService>>();
        }

        private static void InitSomeServices(IServiceProvider services)
        {
            services.GetRequiredService<ISyncManager>().Initialize();
            
            //这个最后调用
            services.GetRequiredService<IClientEvents>().Initialize();   
            //TODO: 检查是否会卡住UI

        }

        private static void InitDatabase(IDatabase database)
        {
            JoinableTasks.JoinableTaskFactory.Run(async () =>
            {
                try
                {
                    await database.InitializeAsync();
                }
                catch(Exception ex)
                {
                    //TODO: 应对
                    Globals.Logger.LogCritical(ex, "Database Initialzie Failed !!!!!");
                }
            });
        }



    }
}
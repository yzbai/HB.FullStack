using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;
using HB.FullStack.Client.Services;
using HB.FullStack.Client.Services.Offline;
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

        public IEnumerable<DbInitContext> DbInitContexts { get; set; } = new List<DbInitContext>();
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

            InitGlobalExceptions();

            InitDatabase(services.GetRequiredService<IDatabase>());

            InitSomeServices(services);
        }

        private static void InitGlobalExceptions()
        {
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                //TODO: 上报

                Globals.Logger.LogError(e.Exception, $"发现没有处理的UnobservedTaskException。Sender: {sender?.GetType().FullName}");

                Currents.ShowToast("抱歉，发生了错误");

                e.SetObserved();
            };

            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex =>
            {
                //TODO:上报

                Globals.Logger.LogError(ex, "使用了SafeFireAndForget的默认异常处理");


                Currents.ShowToast("抱歉，发生了错误");
            });
        }

        private static void InitLog(IServiceProvider services)
        {
            Globals.Logger = services.GetRequiredService<ILogger<InitService>>();
        }

        private static void InitSomeServices(IServiceProvider services)
        {
            services.GetRequiredService<INetwork>().Initialize();   

            
            services.GetRequiredService<ISyncManager>().InitializeAsync();
            //TODO: 检查是否会卡住UI

        }

        private void InitDatabase(IDatabase database)
        {
            JoinableTasks.JoinableTaskFactory.Run(async () =>
            {
                try
                {
                    await database.InitializeAsync(_options.DbInitContexts);
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
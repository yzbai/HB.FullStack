using System;

using CommunityToolkit.Maui;

using HB.FullStack.Client.MauiLib;
using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Startup;
using HB.FullStack.Client.Services.Files;
using HB.FullStack.Client.Services.KVManager;
using HB.FullStack.Client.ApiClient;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;

using SkiaSharp.Views.Maui.Controls.Hosting;
using HB.FullStack.Client;
using AsyncAwaitBestPractices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Hosting
{
    public static class FullStackMauiServiceRegister
    {
        public static MauiAppBuilder UseFullStackClient(
            this MauiAppBuilder builder,
            Action<DbOptions> databaseConfig,
            Action<ApiClientOptions> apiClientConfig,
            Action<FileManagerOptions> fileManagerOptionConfig,
            Action<InitOptions> configureInitOptions,
            string tCaptchaAppId)
        {
            SetGlobalException();

            IServiceCollection services = builder.Services;

            //Options
            services.AddOptions();

            //Core
            services.AddIdGen(idGenOptions =>
            {
                idGenOptions.MachineId = 1;
            });
            services.AddDatabase(databaseConfig, databaseEngineBuilder => databaseEngineBuilder.AddSQLite());


            //HB.FullStack.Client
            services.AddSingleton<IPreferenceProvider, MauiPreferenceProvider>();
            services.AddSingleton<IClientEvents, MauiClientEvents>();
            services.AddApiClient(apiClientConfig);
            services.AddFileManager(fileManagerOptionConfig);
            services.AddKVManager();
            services.AddSyncManager();
            services.AddSmsClientService();


            //HB.FullStack.Client.MauiLib

            services.AddLocalFileManager();
            services.AddTCaptcha(tCaptchaAppId);

            //Initializers
            services.Configure(configureInitOptions);
            services.AddTransient<IMauiInitializeService, InitService>();

            //Handlers
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
            });

            //Skiasharp
            builder.UseSkiaSharp();

            //UIs
            services.AddSingleton<PopupSizeConstants>();
            services.AddTransient<CropperPage>();
            services.AddTransient<CropperViewModel>();

            //Essentials
            services.AddSingleton<IDeviceDisplay>(DeviceDisplay.Current);
            services.AddSingleton<IDeviceInfo>(DeviceInfo.Current);

            return builder;
        }

        private static void SetGlobalException()
        {
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                //TODO: 上报

                Globals.Logger?.LogError(e.Exception, $"发现没有处理的UnobservedTaskException。Sender: {sender?.GetType().FullName}");

                Currents.ShowToast("抱歉，发生了错误");

                e.SetObserved();
            };

            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(ex =>
            {
                //TODO:上报

                Globals.Logger?.LogError(ex, "使用了SafeFireAndForget的默认异常处理");


                Currents.ShowToast("抱歉，发生了错误");
            });
        }
    }
}

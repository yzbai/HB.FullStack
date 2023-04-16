using System;

using CommunityToolkit.Maui;

using HB.FullStack.Client.MauiLib;
using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Services.TCaptcha;
using HB.FullStack.Client.MauiLib.Startup;
using HB.FullStack.Client.Services.Files;
using HB.FullStack.Client.Services.KVManager;
using HB.FullStack.Client.ApiClient;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;

using SkiaSharp.Views.Maui.Controls.Hosting;
using HB.FullStack.Client;

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
            IServiceCollection services = builder.Services;

            //Options
            services.AddOptions();

            //Basic
            services.AddIdGen(idGenOptions =>
            {
                idGenOptions.MachineId = 1;
            });
            services.AddDatabase(databaseConfig, databaseEngineBuilder => databaseEngineBuilder.AddSQLite());
            services.AddApiClient(apiClientConfig);
            services.AddSingleton<IPreferenceProvider, MauiPreferenceProvider>();
            services.AddSmsClientService();


            //HB.FullStack.Client
            services.AddKVManager();
            services.AddSyncManager();
            services.AddFileManager(fileManagerOptionConfig);

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
    }
}

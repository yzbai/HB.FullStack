using System;

using CommunityToolkit.Maui;

using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Services.TCaptcha;
using HB.FullStack.Client.MauiLib.Startup;
using HB.FullStack.Client.Services.Files;
using HB.FullStack.Client.Services.KeyValue;
using HB.FullStack.Client.Services.Offline;
using HB.FullStack.Common.ApiClient;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;

using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Microsoft.Maui.Hosting
{
    public static class FullStackMauiServiceRegister
    {
        public static MauiAppBuilder UseFullStackClient(
            this MauiAppBuilder builder,
            Action<InitOptions> configureInitOptions,
            Action<FileManagerOptions> fileManagerOptionConfig,
            Action<IdGenSettings> idGenConfig,
           
            Action<DbOptions> databaseConfig,
            Action<ApiClientOptions> apiClientConfig,
            //IEnumerable<Migration>? migrations,
            string tCaptchaAppId)
        {
            IServiceCollection services = builder.Services;

            //Options
            services.AddOptions();

            //Basic
            services.AddIdGen(idGenConfig);
            services.AddDatabase(databaseConfig, databaseEngineBuilder => databaseEngineBuilder.AddSQLite());
            services.AddApiClient(apiClientConfig);

            //HB.FullStack.Client
            services.AddTransient<CropperPage>();
            services.AddTransient<CropperViewModel>();

            services.AddKVManager();
            services.AddOfflineManager();

            //HB.FullStack.Client.Maui
            services.AddPreferences();
            services.AddLocalFileManager();
            services.AddFileManager(fileManagerOptionConfig);
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

            //controlers
            services.AddSingleton<PopupSizeConstants>();

            //Essentials
            services.AddSingleton<IDeviceDisplay>(DeviceDisplay.Current);
            services.AddSingleton<IDeviceInfo>(DeviceInfo.Current);

            return builder;
        }
    }
}

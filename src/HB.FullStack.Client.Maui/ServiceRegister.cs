using System;
using System.Collections.Generic;

using CommunityToolkit.Maui;

using HB.FullStack.Client;
using HB.FullStack.Client.Maui;
using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Client.Maui.Controls;
using HB.FullStack.Client.Maui.Controls.Cropper;
using HB.FullStack.Client.Maui.Controls.Popups;
using HB.FullStack.Client.Maui.File;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Devices;

using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Microsoft.Maui.Hosting
{
    public static class FullStackMauiServiceRegister
    {
        public static MauiAppBuilder UseFullStackClient(
            this MauiAppBuilder builder,
            Action<FileManagerOptions> fileManagerOptionConfig,
            Action<IdGenSettings> idGenConfig,
           
            Action<DbOptions> databaseConfig,
            Action<ApiClientOptions> apiClientConfig,
            IEnumerable<Migration>? migrations,
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
            services.AddFileManager(fileManagerOptionConfig);
            services.AddSingleton<StatusManager>();
            services.AddSingleton(typeof(IStatusManager), sp => sp.GetRequiredService<StatusManager>());
            services.AddTCaptcha(tCaptchaAppId);

            //Initializers
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService>(_ => new BaseInitializeService(migrations ?? new List<Migration>())));
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeScopedService, BaseInitalizeScopedService>());
            //builder.Services.AddTransient<IMauiInitializeService, BaseInitializeService>();
            //builder.Services.AddTransient<IMauiInitializeScopedService, BaseInitalizeScopedService>();

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

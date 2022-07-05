﻿using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Client.Maui.Controls;
using HB.FullStack.Client.Maui.Controls.Cropper;
using HB.FullStack.Client.Maui.Controls.Popups;
using HB.FullStack.Client.Maui.File;
using HB.FullStack.Common.ApiClient;
using HB.FullStack.Database;
using HB.Infrastructure.SQLite;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;

using SkiaSharp.Views.Maui.Controls.Hosting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Maui.Hosting
{
    public static class FullStackMauiServiceRegister
    {
        public static MauiAppBuilder UseFullStack(
            this MauiAppBuilder builder,
            Action<FileManagerOptions> fileManagerOptionConfig,
            Action<IdGenSettings> idGenConfig,
            Action<SQLiteOptions> sqliteConfig,
            Action<ApiClientOptions> apiClientConfig,
            IEnumerable<Migration>? migrations,
            string tCaptchaAppId)
        {
            IServiceCollection services = builder.Services;

            //Options
            services.AddOptions();

            //Basic
            services.AddIdGen(idGenConfig);
            services.AddSQLite(sqliteConfig);
            services.AddApiClient(apiClientConfig);

            //HB.FullStack.Client
            services.AddKVManager();

            //HB.FullStack.Client.Maui
            services.AddPreferences();
            services.AddNavigationManager();
            services.AddFileManager(fileManagerOptionConfig);
            services.AddNetworkManager();
            services.AddTCaptcha(tCaptchaAppId);

            //Initializers
            //builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService>(_ => new BaseInitializeService(migrations)));
            //builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeScopedService, BaseInitalizeScopedService>());
            builder.Services.AddTransient<IMauiInitializeService, BaseInitializeService>();
            builder.Services.AddTransient<IMauiInitializeScopedService, BaseInitalizeScopedService>();

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
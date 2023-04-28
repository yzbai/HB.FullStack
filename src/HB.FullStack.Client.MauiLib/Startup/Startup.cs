/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using CommunityToolkit.Maui;

using HB.FullStack.Client;
using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Files;
using HB.FullStack.Client.MauiLib;
using HB.FullStack.Client.MauiLib.Components;
using HB.FullStack.Client.MauiLib.Controls;
using HB.FullStack.Client.MauiLib.Startup;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Microsoft.Maui.Hosting
{
    public static class FullStackMauiServiceRegister
    {
        public static MauiAppBuilder UseFullStackMaui(
            this MauiAppBuilder builder,
            Action<DbOptions> configDbOptions,
            Action<ApiClientOptions> configApiClientOptions,
            Action<FileManagerOptions> configFileManagerOptions,
            Action<ClientOptions> configClientOptions,
            Action<MauiOptions> configMauiOptions,
            string tCaptchaAppId)
        {
            ConfigureGlobalException();

            //Logging
            if (Currents.IsDebug)
            {
                builder.Logging.AddDebug().AddFilter("HB", LogLevel.Trace);
                //builder.Services.AddLogging(loggingBuilder => { loggingBuilder.AddDebug().AddFilter("HB", LogLevel.Trace); });
            }
            else
            {
                builder.Logging.AddDebug().AddFilter("HB", LogLevel.Trace);
                //builder.Services.AddLogging(loggingBuilder => { loggingBuilder.AddDebug().AddFilter("HB", LogLevel.Trace); });
            }

            IServiceCollection services = builder.Services;

            //Core
            services.AddOptions();
            services.AddIdGen(idGenOptions =>
            {
                idGenOptions.MachineId = 1;
            });
            services.AddDatabase(configDbOptions, databaseEngineBuilder => databaseEngineBuilder.AddSQLite());

            //Client
            services.AddFullStackClient(configClientOptions, configFileManagerOptions, configApiClientOptions);

            //MauiLib - Client Abstractions
            services.AddSingleton<ITokenPreferences, MauiTokenPreferences>();
            services.AddSingleton<IClientEvents, MauiClientEvents>();

            //MauiLib - Components
            services.AddLocalFileManager();
            services.AddTCaptcha(tCaptchaAppId);

            //MauiLib - Initialize
            services.Configure(configMauiOptions);
            services.AddTransient<IMauiInitializeService, MauiInitService>();

            //MauiLib - Controls
            services.AddSingleton<PopupSizeConstants>();
            services.AddTransient<CropperPage>();
            services.AddTransient<CropperViewModel>();
            services.AddTransient<IntroducePage>();
            services.AddTransient<IntroduceViewModel>();
            services.AddTransient<LoginPage>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<UserProfileUpdatePage>();
            services.AddTransient<UserProfileUpdateViewModel>();
            services.AddTransient<SmsVerifyPage>();
            services.AddTransient<SmsVerifyViewModel>();

            //MauiLib - Essentials
            services.AddSingleton<IDeviceDisplay>(DeviceDisplay.Current);
            services.AddSingleton<IDeviceInfo>(DeviceInfo.Current);

            //MauiLib - Handlers
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
            });

            //Skiasharp
            builder.UseSkiaSharp();

            return builder;
        }

        private static void ConfigureGlobalException()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                string msg = "AppDomain UnHandled Exceptions : " + e.ExceptionObject.ToString();
                
                Globals.Logger?.LogError(msg);

                Currents.ShowToast(msg);

                //程序还是会结束
            };

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
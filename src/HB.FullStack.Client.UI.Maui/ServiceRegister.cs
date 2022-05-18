using HB.FullStack.Client.UI.Maui.Controls;
using HB.FullStack.Client.UI.Maui.File;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

using System;

namespace HB.FullStack.Client.UI.Maui
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddFullStackMaui(this IServiceCollection services, Action<FileManagerOptions> fileManagerOptionConfig, string tCaptchaAppId)
        {
            //HB.FullStack.Client
            services.AddKVManager();

            //HB.FullStack.Client.UI.Maui
            services.AddPreferences();
            services.AddFileManager(fileManagerOptionConfig);
            services.AddNetworkManager();
            services.AddTCaptcha(tCaptchaAppId);

            return services;
        }

        public static MauiAppBuilder AddFullStackHandler(this MauiAppBuilder builder)
        {
            builder.ConfigureMauiHandlers(handlers => {
                handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
            });
            return builder;
        }
    }
}

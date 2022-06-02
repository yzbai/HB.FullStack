using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Client.Maui.Controls;
using HB.FullStack.Client.Maui.Controls.Cropper;
using HB.FullStack.Client.Maui.Controls.Popups;
using HB.FullStack.Client.Maui.File;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;

using SkiaSharp.Views.Maui.Controls.Hosting;

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Maui.Hosting
{
    public static class FullStackMauiServiceRegister
    {
        public static MauiAppBuilder UseFullStack(this MauiAppBuilder builder, Action<FileManagerOptions> fileManagerOptionConfig, string tCaptchaAppId)
        {
            IServiceCollection services = builder.Services;

            //Skiasharp
            builder.UseSkiaSharp();

            //Options
            services.AddOptions();

            //HB.FullStack.Client
            services.AddKVManager();

            //HB.FullStack.Client.Maui
            services.AddPreferences();
            services.AddNavigationManager();
            services.AddFileManager(fileManagerOptionConfig);
            services.AddNetworkManager();
            services.AddTCaptcha(tCaptchaAppId);

            //Initializers
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, BaseInitializeService>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeScopedService, BaseInitalizeScopedService>());

            //Handlers
            builder.ConfigureMauiHandlers(handlers => {
                handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
            });

            //controlers
            services.AddSingleton<PopupSizeConstants>();

            services.AddTransient<CropperViewModel>();

            return builder;
        }

        public static IConfiguration GetConfiguration(string appsettingsFile, [ValidatedNotNull] Assembly executingAssembly)
        {
            ThrowIf.Empty(appsettingsFile, nameof(appsettingsFile));

            string fileName = $"{executingAssembly.FullName!.Split(',')[0]}.{appsettingsFile}";

            using Stream? resFileStream = executingAssembly.GetManifestResourceStream(fileName);

            IConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonStream(resFileStream);

            return builder.Build();
        }

        public static string Environment =>
#if DEBUG
    "Debug";
#endif
#if RELEASE
            "Release";
#endif
    }
}

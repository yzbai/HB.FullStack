using HB.FullStack.Client.Maui.Controls;
using HB.FullStack.Client.Maui.File;

using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Hosting;

using SkiaSharp.Views.Maui.Controls.Hosting;

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FullStackMauiServiceRegister
    {
        public static MauiAppBuilder UseFullStack(this MauiAppBuilder builder, Action<FileManagerOptions> fileManagerOptionConfig, string tCaptchaAppId)
        {
            #region Services
            
            IServiceCollection services = builder.Services;

            //add skiasharp
            builder.UseSkiaSharp();


            services.AddOptions();



            //HB.FullStack.Client
            services.AddKVManager();

            //HB.FullStack.Client.Maui
            services.AddPreferences();
            services.AddNavigationManager();
            services.AddFileManager(fileManagerOptionConfig);
            services.AddNetworkManager();
            services.AddTCaptcha(tCaptchaAppId);

            #endregion

            builder.ConfigureMauiHandlers(handlers => {
                handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
            });
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

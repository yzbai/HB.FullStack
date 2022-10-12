using System;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Client.File;
using HB.FullStack.Client.Maui.File;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileManagerServiceRegister
    {
        public static IServiceCollection AddFileManager(this IServiceCollection services, IConfiguration configuration)
        {
            return AddFileManager(services, options => configuration.Bind(options));
        }

        public static IServiceCollection AddFileManager(this IServiceCollection services, Action<FileManagerOptions> action)
        {
            FileManagerOptions options = new FileManagerOptions();
            action(options);

            services.Configure(action);

            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<StsTokenRepo>();

            ClientModelDefFactory.Register<StsToken>(new ClientModelDef
            {
                ExpiryTime = TimeSpan.MaxValue,
                AllowOfflineRead = false,
                AllowOfflineAdd = false,
                AllowOfflineDelete = false,
                AllowOfflineUpdate = false
            });

            return services;
        }
    }
}

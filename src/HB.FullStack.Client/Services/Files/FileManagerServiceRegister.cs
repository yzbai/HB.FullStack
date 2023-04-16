using System;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Client.Services.Files;
using HB.FullStack.Client.Services.Sts;
using HB.FullStack.Common.Files;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileManagerServiceRegister
    {
        public static IServiceCollection AddFileManager(this IServiceCollection services, Action<FileManagerOptions> action)
        {
            //FileManagerOptions options = new FileManagerOptions();
            //action(options);

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

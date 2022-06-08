using HB.FullStack.Client.ClientEntity;
using HB.FullStack.Client.File;
using HB.FullStack.Client.Maui.File;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            services.ConfigureOptions(options);

            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<AliyunStsTokenRepo>();

            ClientEntityDefFactory.Register<AliyunStsToken>(new ClientEntityDef
            {
                ExpiryTime = options.AliyunStsTokenExpiryTime,
                AllowOfflineRead = false,
                AllowOfflineWrite = false,
                NeedLogined = true
            });

            return services;
        }
    }
}

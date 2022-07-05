﻿using HB.FullStack.Client.ClientEntity;
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

            services.Configure(action);

            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<StsTokenRepo>();

            ClientEntityDefFactory.Register<StsToken>(new ClientEntityDef
            {
                ExpiryTime = TimeSpan.MaxValue,
                AllowOfflineRead = false,
                AllowOfflineWrite = false,
                NeedLogined = true
            });

            return services;
        }
    }
}
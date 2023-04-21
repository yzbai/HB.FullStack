/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Base;
using HB.FullStack.Client.Components.Files;
using HB.FullStack.Client.Components.KVManager;
using HB.FullStack.Client.Components.Sts;
using HB.FullStack.Client.Components.User;
using HB.FullStack.Common.Files;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientServiceRegister
    {
        public static IServiceCollection AddFullStackClient(this IServiceCollection services,
            Action<ClientOptions> configClientOptions,
            Action<FileManagerOptions> configFileManagerOptions,
            Action<ApiClientOptions> configApiClientOptions)
        {
            services.Configure(configClientOptions);

            //Base
            services.AddSingleton<IClientModelSettingFactory, ClientModelSettingFactory>();

            //ApiClient
            services.AddApiClient(configApiClientOptions);

            //File
            services.Configure(configFileManagerOptions);
            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<StsTokenRepo>();

            //KVManager
            services.AddKVManager();

            //SmsService
            services.AddSmsService();

            //Sync
            services.AddSyncManager();

            //User
            services.AddSingleton<UserProfileRepo>();
            services.AddSingleton<IUserService, UserService>();

            return services;
        }
    }
}
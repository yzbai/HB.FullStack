/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Client.Base;
using HB.FullStack.Client.Components.File;
using HB.FullStack.Client.Components.Files;
using HB.FullStack.Client.Components.Sts;
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

            return services;
        }
    }
}
/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Client.MauiLib.Components;
using HB.FullStack.Common.Files;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LocalFileManagerServiceRegister
    {
        public static IServiceCollection AddLocalFileManager(this IServiceCollection services)
        {
            services.AddSingleton<ILocalFileManager, LocalFileManager>();

            return services;
        }
    }
}
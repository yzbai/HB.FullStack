using HB.FullStack.Client.MauiLib.Services.Files;
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

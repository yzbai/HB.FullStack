using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Client.Services.KVManager
{
    public static class KVManagerServiceRegister
    {
        public static IServiceCollection AddKVManager(this IServiceCollection services)
        {
            services.AddSingleton<KVRepo>();
            services.AddSingleton<IKVManager, DbKVManager>();   
            services.AddSingleton<IDbSimpleLocker, DbSimpleLocker>();

            return services;
        }
    }
}

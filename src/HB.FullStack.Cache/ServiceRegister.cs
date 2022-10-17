using HB.FullStack.Cache;
using HB.FullStack.Common.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            services.AddSingleton<ICacheModelDefFactory, CacheModelDefFactory>();

            //IModelDefProvider
            services.AddSingleton(typeof(IModelDefProvider), sp => sp.GetRequiredService(typeof(ICacheModelDefFactory)));

            return services;
        }
    }
}

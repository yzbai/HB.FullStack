using HB.FullStack.Common.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddFullStackCommon(this IServiceCollection services)
        {
            services.AddSingleton<IModelDefFactory, ModelDefFactory>();

            services.AddSingleton<IModelDefProvider, PlainModelDefProvider>();

            return services;
        }
    }
}

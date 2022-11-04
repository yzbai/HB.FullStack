using HB.FullStack.Common.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddModelDefFactory(this IServiceCollection services)
        {
            services.AddOptions();

            services.AddSingleton<IModelDefFactory, ModelDefFactory>();

            services.AddSingleton<IModelDefProvider, PlainModelDefProvider>();

            return services;
        }
    }
}

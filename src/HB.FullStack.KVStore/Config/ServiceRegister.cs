using System;

using HB.FullStack.Common.Models;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Config;
using HB.FullStack.KVStore.KVStoreModels;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KVStoreServiceRegister
    {
        public static IServiceCollection AddKVStore(this IServiceCollection services, IConfiguration configuration, Action<IKVStoreEngineBuilder> configKVStoreEngineBuilder)
        {
            //services.AddOptions();
            services.Configure<KVStoreOptions>(configuration);

            configKVStoreEngineBuilder(new KVStoreEngineBuilder(services));

            services.AddKVStoreCore();

            return services;
        }

        public static IServiceCollection AddKVStore(this IServiceCollection services, Action<KVStoreOptions> configKVStoreOptions, Action<IKVStoreEngineBuilder> configKVStoreEngineBuilder)
        {
            //services.AddOptions();
            services.Configure(configKVStoreOptions);

            configKVStoreEngineBuilder(new KVStoreEngineBuilder(services));

            services.AddKVStoreCore();

            return services;
        }

        private static IServiceCollection AddKVStoreCore(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddOptions();

            serviceCollection.AddSingleton<IKVStoreModelDefFactory, KVStoreModelDefFactory>();
            serviceCollection.AddSingleton<IKVStore, DefaultKVStore>();

            //IModeDefProvider
            serviceCollection.AddSingleton(typeof(IModelDefProvider), sp => sp.GetRequiredService(typeof(IKVStoreModelDefFactory)));

            return serviceCollection;
        }
    }
}

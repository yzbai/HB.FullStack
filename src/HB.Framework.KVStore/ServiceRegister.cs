using HB.Framework.KVStore;
using HB.Framework.KVStore.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using HB.Framework.KVStore.Engine;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddKVStore(this IServiceCollection serviceCollection, Action<KVStoreOptions> kvstoreOptionsSetup)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<KVStoreOptions>(kvstoreOptionsSetup);

            AddService(serviceCollection);

            return serviceCollection;
        }

        public static IServiceCollection AddKVStore(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<KVStoreOptions>(configuration);

            AddService(serviceCollection);

            return serviceCollection;
        }

        private static void AddService(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IKVStoreEntityDefFactory, DefaultKVStoreModelDefFactory>();
            serviceCollection.AddSingleton<IKVStore, DefaultKVStore>();
        }
    }
}

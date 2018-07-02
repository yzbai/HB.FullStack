using HB.Framework.KVStore;
using HB.Framework.KVStore.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using HB.Framework.KVStore.Engine;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KVStoreServiceCollectionExtensions
    {
        public static IServiceCollection AddKVStore(this IServiceCollection serviceCollection, Action<KVStoreOptions> kvstoreOptionsSetup)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<KVStoreOptions>(kvstoreOptionsSetup);

            serviceCollection.AddSingleton<IKVStoreEntityDefFactory, DefaultKVStoreModelDefFactory>();
            serviceCollection.AddSingleton<IKVStore, DefaultKVStore>();

            return serviceCollection;
        }

        public static IServiceCollection AddKVStore(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            serviceCollection.Configure<KVStoreOptions>(configuration);

            serviceCollection.AddSingleton<IKVStoreEntityDefFactory, DefaultKVStoreModelDefFactory>();
            serviceCollection.AddSingleton<IKVStore, DefaultKVStore>();

            return serviceCollection;
        }
    }
}

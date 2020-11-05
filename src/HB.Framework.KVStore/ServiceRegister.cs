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
        public static IServiceCollection AddKVStore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions();

            serviceCollection.AddSingleton<IKVStoreEntityDefFactory, DefaultKVStoreModelDefFactory>();
            serviceCollection.AddSingleton<IKVStore, DefaultKVStore>();

            return serviceCollection;
        }
    }
}

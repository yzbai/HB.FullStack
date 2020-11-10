using HB.Framework.KVStore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using HB.Framework.KVStore.Engine;
using HB.Framework.KVStore.Entities;

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

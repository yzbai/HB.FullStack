using HB.FullStack.KVStore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using HB.FullStack.KVStore.Engine;
using HB.FullStack.KVStore.KVStoreModels;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddKVStore(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddOptions();

            serviceCollection.AddSingleton<IKVStore, DefaultKVStore>();

            return serviceCollection;
        }
    }
}

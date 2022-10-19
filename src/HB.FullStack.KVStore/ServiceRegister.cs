using HB.FullStack.Common.Models;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.KVStoreModels;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddKVStore(this IServiceCollection serviceCollection)
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

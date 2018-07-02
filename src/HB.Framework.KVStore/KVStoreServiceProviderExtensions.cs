using System;
using Microsoft.Extensions.DependencyInjection;

namespace HB.Framework.KVStore
{
    public static class KVStoreServiceProviderExtensions
    {
        public static IKVStore GetKVStore(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IKVStore>();
        }
    }
}

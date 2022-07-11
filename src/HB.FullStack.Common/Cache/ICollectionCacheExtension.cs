using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Cache
{
    public static class ICollectionCacheExtension
    {
        public static async Task<T?> GetFromCollectionAsync<T>(this ICache cache, string collectionKey, string itemKey, CancellationToken token = default) where T : class
        {
            byte[]? bytes = await cache.GetFromCollectionAsync(collectionKey, itemKey, token).ConfigureAwait(false);
            return SerializeUtil.Deserialize<T?>(bytes);
        }

        public static Task<bool> SetToCollectionAsync<T>(this ICache cache, string collectionKey, IEnumerable<string> itemKeys, IEnumerable<T> itemValues, IEnumerable<long> timestamps, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            IEnumerable<byte[]> bytess = SerializeUtil.Serialize<T>(itemValues).ToList();

            return cache.SetToCollectionAsync(collectionKey, itemKeys, bytess, timestamps, options, token);
        }

        public static Task<bool> SetToCollectionAsync<T>(this ICache cache, string collectionKey, string itemKey, T itemValue, long timestamp, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            byte[] bytes = SerializeUtil.Serialize(itemValue);

            return cache.SetToCollectionAsync(collectionKey, new string[] { itemKey }, new List<byte[]> { bytes }, new long[] { timestamp }, options, cancellationToken);
        }

        public static Task RemoveFromCollectionAsync(this ICache cache, string collectionKey, string itemKey, CancellationToken cancellationToken = default)
        {
            return cache.RemoveFromCollectionAsync(collectionKey, new string[] { itemKey }, cancellationToken);
        }
    }
}

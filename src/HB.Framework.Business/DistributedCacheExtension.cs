using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.Framework.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.Framework.Business
{
    public static class DistributedCacheExtension
    {
        public static async Task<bool> TryGetItemAsync<T>(this IDistributedCache cache, CacheItem<T> cacheItem, CancellationToken cancellationToken = default) where T : class
        {
            byte[]? bytes = await cache.GetAsync(cacheItem.Key, cancellationToken).ConfigureAwait(false);

            if (bytes == null)
            {
                cacheItem.Value = null;
                return false;
            }

            cacheItem.Value = await SerializeUtil.UnPackAsync<T>(bytes).ConfigureAwait(false);
            return true;
        }

        public static Task SetItemAsync<T>(this IDistributedCache cache, CacheItem<T> cacheItem, CancellationToken cancellationToken = default) where T : class
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheItem.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = cacheItem.SlidingExpiration
            };

            return cache.SetAsync(cacheItem.Key, cacheItem.Value, entryOptions, cancellationToken);
        }
    }
}

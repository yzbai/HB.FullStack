using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Common.Cache
{
    public static class CachedItemExtensions
    {
        public static Task<TResult?> GetAsync<TResult>(this ICache cache, CachedItem<TResult> cachedItem, CancellationToken cancellationToken = default)
        {
            ThrowOnEmptyCacheKey(cachedItem);

            return cache.GetAsync<TResult>(cachedItem.CacheKey, cancellationToken);
        }

        public static Task<bool> SetAsync<TResult>(this ICache cache, CachedItem<TResult> cachedItem, CancellationToken cancellationToken = default)
        {
            ThrowOnEmptyCacheKey(cachedItem);
            ThrowOnNullCacheValue(cachedItem);
            ThrowOnEmptyTimestamp(cachedItem);

            return cache.SetAsync(
                cachedItem.CacheKey,
                cachedItem.CacheValue!,
                cachedItem.Timestamp,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cachedItem.AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = cachedItem.SlidingExpiration
                },
                cancellationToken);
        }

        public static Task<bool> RemoveAsync(this ICache cache, ICachedItem cachedItem)
        {
            ThrowOnEmptyCacheKey(cachedItem);

            return cache.RemoveAsync(cachedItem.CacheKey);
        }

        public static Task<bool> RemoveAsync(this ICache cache, IEnumerable<ICachedItem> cachedItems)
        {
            return cache.RemoveAsync(cachedItems.Select(item => item.CacheKey).ToArray());
        }

        private static void ThrowOnEmptyCacheKey(ICachedItem cachedItem)
        {
            if (string.IsNullOrEmpty(cachedItem?.CacheKey))
            {
                throw CacheExceptions.CacheKeyNotSet(resourceType: cachedItem?.CachedType);
            }
        }

        private static void ThrowOnNullCacheValue<TResult>(CachedItem<TResult> cachedItem)
        {
            if (cachedItem.CacheValue == null)
            {
                throw CacheExceptions.CacheValueNotSet(resourceType: cachedItem.CachedType, cacheKey: cachedItem.CacheKey);
            }
        }

        private static void ThrowOnEmptyTimestamp(ICachedItem cachedItem)
        {
            if (cachedItem.Timestamp <= 0)
            {
                throw CacheExceptions.CachedItemTimestampNotSet(resourceType: cachedItem.CachedType, cacheKey: cachedItem.CacheKey, null);
            }
        }
    }
}
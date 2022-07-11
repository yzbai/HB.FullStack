using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Repository
{
    public static class CachedItemExtensions
    {
        public static Task<TResult?> GetAsync<TResult>(this ICache cache, CachedItem<TResult> cachedItem, CancellationToken cancellationToken = default) where TResult : class
        {
            ThrowOnEmptyCacheKey(cachedItem);

            return cache.GetAsync<TResult>(cachedItem.CacheKey, cancellationToken);
        }

        public static Task SetAsync<TResult>(this ICache cache, CachedItem<TResult> cachedItem, CancellationToken cancellationToken = default) where TResult : class
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

        public static Task<bool> RemoveAsync(this ICache cache, CachedItem cachedItem)
        {
            ThrowOnEmptyCacheKey(cachedItem);
            ThrowOnEmptyTimestamp(cachedItem);

            return cache.RemoveAsync(cachedItem.CacheKey);
        }

        public static Task<bool> RemoveAsync(this ICache cache, IEnumerable<CachedItem> cachedItems)
        {
            return cache.RemoveAsync(cachedItems.Select(item => item.CacheKey).ToArray());
        }

        private static void ThrowOnEmptyCacheKey(CachedItem cachedItem)
        {
            if (string.IsNullOrEmpty(cachedItem?.CacheKey))
            {
                throw RepositoryExceptions.CacheKeyNotSet(resourceType: cachedItem?.CachedType);
            }
        }

        private static void ThrowOnNullCacheValue<TResult>(CachedItem<TResult> cachedItem) where TResult : class
        {
            if (cachedItem.CacheValue == null)
            {
                throw RepositoryExceptions.CacheValueNotSet(resourceType: cachedItem.CachedType, cacheKey: cachedItem.CacheKey);
            }
        }

        private static void ThrowOnEmptyTimestamp(CachedItem cachedItem)
        {
            if (cachedItem.Timestamp <= 0)
            {
                throw RepositoryExceptions.CachedItemTimestampNotSet(resourceType: cachedItem.CachedType, cacheKey: cachedItem.CacheKey, null);
            }
        }
    }
}
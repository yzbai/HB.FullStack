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
            ThrowOnEmptyUtcTicks(cachedItem);

            return cache.SetAsync(
                cachedItem.CacheKey,
                cachedItem.CacheValue!,
                cachedItem.UtcTicks,
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
            ThrowOnEmptyUtcTicks(cachedItem);

            return cache.RemoveAsync(cachedItem.CacheKey, cachedItem.UtcTicks);
        }

        public static Task<bool> RemoveAsync(this ICache cache, IEnumerable<CachedItem> cachedItems, UtcNowTicks utcNowTicks)
        {
            return cache.RemoveAsync(cachedItems.Select(item => item.CacheKey).ToArray(), utcNowTicks);
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

        private static void ThrowOnEmptyUtcTicks(CachedItem cachedItem)
        {
            if (cachedItem.UtcTicks.IsEmpty())
            {
                throw RepositoryExceptions.UtcTicksNotSet(resourceType: cachedItem.CachedType, cacheKey: cachedItem.CacheKey, null);
            }
        }
    }
}


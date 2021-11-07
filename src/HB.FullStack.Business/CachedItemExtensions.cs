using System;
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

        /// <exception cref="RepositoryException"></exception>
        /// <exception cref="CacheException"></exception>
        public static Task SetAsync<TResult>(this ICache cache, CachedItem<TResult> cachedItem, CancellationToken cancellationToken = default) where TResult : class
        {
            ThrowOnEmptyCacheKey(cachedItem);
            ThrowOnNullCacheValue(cachedItem);
            ThrowOnEmptyUtcTicks(cachedItem);

            return cache.SetAsync(
                cachedItem.CacheKey,
                cachedItem.CacheValue!,
                cachedItem.UtcTikcs,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cachedItem.AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = cachedItem.SlidingExpiration
                },
                cancellationToken);
        }

        /// <exception cref="CacheException"></exception>
        /// <exception cref="RepositoryException"></exception>
        public static Task<bool> RemoveAsync(this ICache cache, CachedItem cachedItem)
        {
            ThrowOnEmptyCacheKey(cachedItem);
            ThrowOnEmptyUtcTicks(cachedItem);

            return cache.RemoveAsync(cachedItem.CacheKey, cachedItem.UtcTikcs);
        }

        private static void ThrowOnEmptyCacheKey(CachedItem cachedItem)
        {
            if (string.IsNullOrEmpty(cachedItem?.CacheKey))
            {
                throw RepositoryExceptions.CacheKeyNotSet(resourceType: cachedItem?.ResourceType);
            }
        }

        private static void ThrowOnNullCacheValue<TResult>(CachedItem<TResult> cachedItem) where TResult : class
        {
            if (cachedItem.CacheValue == null)
            {
                throw RepositoryExceptions.CacheValueNotSet(resourceType: cachedItem.ResourceType, cacheKey: cachedItem.CacheKey);
            }
        }

        private static void ThrowOnEmptyUtcTicks(CachedItem cachedItem)
        {
            if (cachedItem.UtcTikcs.IsEmpty())
            {
                throw RepositoryExceptions.UtcTicksNotSet(resourceType: cachedItem.ResourceType, cacheKey: cachedItem.CacheKey, null);
            }
        }
    }
}


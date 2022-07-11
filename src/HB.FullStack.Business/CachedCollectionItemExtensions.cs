using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Repository
{
    public static class CachedCollectionItemExtensions
    {
        public static Task<TResult?> GetAsync<TResult>(this ICache cache, CachedCollectionItem<TResult> cachedCollectionItem, CancellationToken cancellationToken = default) where TResult : class
        {
            ThrowOnEmptyCacheKey(cachedCollectionItem);

            return cache.GetFromCollectionAsync<TResult>(cachedCollectionItem.CollectionKey, cachedCollectionItem.ItemKey, cancellationToken);
        }

        public static Task SetAsync<TResult>(this ICache cache, CachedCollectionItem<TResult> cachedCollectionItem, CancellationToken cancellationToken = default) where TResult : class
        {
            ThrowOnEmptyCacheKey(cachedCollectionItem);
            ThrowOnNullCacheValue(cachedCollectionItem);
            ThrowOnEmptyUtcTicks(cachedCollectionItem);

            return cache.SetToCollectionAsync(
                cachedCollectionItem.CollectionKey,
                cachedCollectionItem.ItemKey,
                cachedCollectionItem.ItemValue,
                cachedCollectionItem.Timestamp,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cachedCollectionItem.AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = cachedCollectionItem.SlidingExpiration
                },
                cancellationToken);
        }

        public static Task<bool> RemoveAsync(this ICache cache, CachedCollectionItem cachedCollectionItem, CancellationToken cancellationToken = default)
        {
            ThrowOnEmptyCacheKey(cachedCollectionItem);
            ThrowOnEmptyUtcTicks(cachedCollectionItem);

            return cache.RemoveFromCollectionAsync(cachedCollectionItem.CollectionKey, cachedCollectionItem.ItemKey, cachedCollectionItem.UtcTicks, cancellationToken);
        }

        public static Task<bool> RemoveAsync(this ICache cache, IEnumerable<CachedCollectionItem> cachedCollectionItems, CancellationToken cancellationToken = default)
        {
            if (!cachedCollectionItems.Any())
            {
                return Task.FromResult(true);
            }

            foreach (CachedCollectionItem cachedCollectionItem in cachedCollectionItems)
            {
                ThrowOnEmptyCacheKey(cachedCollectionItem);
                ThrowOnEmptyUtcTicks(cachedCollectionItem);
            }

            string collectionKey = cachedCollectionItems.First().CollectionKey;
            UtcNowTicks utcTicks = cachedCollectionItems.First().UtcTicks;

            if (!cachedCollectionItems.All(item => item.CollectionKey == collectionKey && item.UtcTicks == utcTicks))
            {
                throw RepositoryExceptions.CacheCollectionKeyNotSame(cachedCollectionItems);
            }

            return cache.RemoveFromCollectionAsync(collectionKey, cachedCollectionItems.Select(item => item.ItemKey).ToList(), utcTicks, cancellationToken);
        }

        public static Task<bool> RemoveCollectionAsync(this ICache cache, CachedCollectionItem cachedCollectionItem)
        {
            return cache.RemoveCollectionAsync(cachedCollectionItem.CollectionKey);
        }

        private static void ThrowOnEmptyCacheKey(CachedCollectionItem cachedCollectionItem)
        {
            if (string.IsNullOrEmpty(cachedCollectionItem?.CollectionKey))
            {
                throw RepositoryExceptions.CacheKeyNotSet(resourceType: cachedCollectionItem?.CollectionKey);
            }

            if (string.IsNullOrEmpty(cachedCollectionItem?.ItemKey))
            {
                throw RepositoryExceptions.CacheKeyNotSet(resourceType: cachedCollectionItem?.ItemKey);
            }
        }

        private static void ThrowOnNullCacheValue<TResult>(CachedCollectionItem<TResult> cachedCollectionItem) where TResult : class
        {
            if (cachedCollectionItem.ItemValue == null)
            {
                throw RepositoryExceptions.CacheValueNotSet(resourceType: cachedCollectionItem.CollectionKey, cacheKey: cachedCollectionItem.ItemKey);
            }
        }

        private static void ThrowOnEmptyUtcTicks(CachedCollectionItem cachedCollectionItem)
        {
            if (cachedCollectionItem.UtcTicks.IsEmpty())
            {
                throw RepositoryExceptions.CachedItemTimestampNotSet(resourceType: cachedCollectionItem.CollectionKey, cacheKey: cachedCollectionItem.ItemKey, null);
            }
        }
    }
}
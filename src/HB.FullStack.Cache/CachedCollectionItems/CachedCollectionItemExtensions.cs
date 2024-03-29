﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Cache
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

        public static async Task RemoveAsync(this ICache cache, ICachedCollectionItem cachedCollectionItem, CancellationToken cancellationToken = default)
        {
            ThrowOnEmptyCacheKey(cachedCollectionItem);
            ThrowOnEmptyUtcTicks(cachedCollectionItem);

            await cache.RemoveFromCollectionAsync(cachedCollectionItem.CollectionKey, cachedCollectionItem.ItemKey, cancellationToken).ConfigureAwait(false);
        }

        public static async Task RemoveAsync(this ICache cache, IEnumerable<ICachedCollectionItem> cachedCollectionItems, CancellationToken cancellationToken = default)
        {
            if (!cachedCollectionItems.Any())
            {
                return;
            }

            foreach (ICachedCollectionItem cachedCollectionItem in cachedCollectionItems)
            {
                ThrowOnEmptyCacheKey(cachedCollectionItem);
                ThrowOnEmptyUtcTicks(cachedCollectionItem);
            }

            string collectionKey = cachedCollectionItems.First().CollectionKey;

            if (!cachedCollectionItems.All(item => item.CollectionKey == collectionKey))
            {
                throw CacheExceptions.CacheCollectionKeyNotSame(cachedCollectionItems);
            }

            await cache.RemoveFromCollectionAsync(collectionKey, cachedCollectionItems.Select(item => item.ItemKey).ToList(), cancellationToken).ConfigureAwait(false);
        }

        public static Task<bool> RemoveCollectionAsync(this ICache cache, ICachedCollectionItem cachedCollectionItem)
        {
            return cache.RemoveCollectionAsync(cachedCollectionItem.CollectionKey);
        }

        private static void ThrowOnEmptyCacheKey(ICachedCollectionItem cachedCollectionItem)
        {
            if (string.IsNullOrEmpty(cachedCollectionItem?.CollectionKey))
            {
                throw CacheExceptions.CacheKeyNotSet(resourceType: cachedCollectionItem?.CollectionKey);
            }

            if (string.IsNullOrEmpty(cachedCollectionItem?.ItemKey))
            {
                throw CacheExceptions.CacheKeyNotSet(resourceType: cachedCollectionItem?.ItemKey);
            }
        }

        private static void ThrowOnNullCacheValue<TResult>(CachedCollectionItem<TResult> cachedCollectionItem) where TResult : class
        {
            if (cachedCollectionItem.ItemValue == null)
            {
                throw CacheExceptions.CacheValueNotSet(resourceType: cachedCollectionItem.CollectionKey, cacheKey: cachedCollectionItem.ItemKey);
            }
        }

        private static void ThrowOnEmptyUtcTicks(ICachedCollectionItem cachedCollectionItem)
        {
            if (cachedCollectionItem.Timestamp <= 0)
            {
                throw CacheExceptions.CachedItemTimestampNotSet(resourceType: cachedCollectionItem.CollectionKey, cacheKey: cachedCollectionItem.ItemKey, null);
            }
        }
    }
}
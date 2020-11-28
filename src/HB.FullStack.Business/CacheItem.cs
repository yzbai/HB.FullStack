using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Business
{
    public abstract class CacheItem<TCacheItem, TResult>
        where TResult : class where TCacheItem : CacheItem<TCacheItem, TResult>, new()
    {
        //internal CacheItem() { }

        public abstract string Prefix { get; }

        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        public string CacheKey { get; private set; } = null!;

        public TResult? CacheValue { get; private set; }

        public Task<TResult?> GetFromAsync(ICache cache, CancellationToken cancellationToken = default)
        {
            return cache.GetAsync<TResult>(CacheKey, cancellationToken);
        }

        public Task SetToAsync(ICache cache, CancellationToken cancellationToken = default)
        {
            if (CacheValue == null)
            {
                throw new ArgumentNullException(nameof(CacheValue));
            }

            return cache.SetAsync<TResult>(
                CacheKey,
                CacheValue,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = SlidingExpiration
                },
                cancellationToken);
        }

        public CacheItem<TCacheItem, TResult> Value(TResult result)
        {
            CacheValue = result;
            return this;
        }

        public static TCacheItem Key(params string[] keys)
        {
            TCacheItem cacheItem = new TCacheItem();

            cacheItem.CacheKey = cacheItem.Prefix + keys.ToJoinedString("_");

            return cacheItem;
        }

    }
}


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Business
{
    public abstract class CacheItem<TResult>
        where TResult : class
    {
        private CacheItem() { ResourceType = this.GetType().Name; }
        protected CacheItem(params string[] keys) : this()
        {
            CacheKey = ResourceType + keys.ToJoinedString("_");
        }
        public string ResourceType { get; private set; }

        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        public string CacheKey { get; private set; } = null!;

        public TResult? CacheValue { get; private set; }

        public long TimestampInUnixMilliseconds { get; private set; } = -1;

        public Task<TResult?> GetFromAsync(ICache cache, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentNullException(nameof(CacheKey));
            }

            return cache.GetAsync<TResult>(CacheKey, cancellationToken);
        }

        public Task SetToAsync(ICache cache, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentNullException(nameof(CacheKey));
            }

            if (CacheValue == null)
            {
                throw new ArgumentNullException(nameof(CacheValue));
            }

            if (TimestampInUnixMilliseconds == -1)
            {
                throw new ArgumentException(nameof(TimestampInUnixMilliseconds));
            }

            return cache.SetAsync<TResult>(
                CacheKey,
                CacheValue,
                TimestampInUnixMilliseconds,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = SlidingExpiration
                },
                cancellationToken);
        }

        public async Task<bool> RemoveFromAsync(ICache cache)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new ArgumentNullException(nameof(CacheKey));
            }

            if (TimestampInUnixMilliseconds == -1)
            {
                throw new ArgumentException(nameof(TimestampInUnixMilliseconds));
            }

            return await cache.RemoveAsync(CacheKey, TimestampInUnixMilliseconds).ConfigureAwait(false);
        }

        public CacheItem<TResult> Value(TResult result)
        {
            CacheValue = result;
            return this;
        }

        public CacheItem<TResult> Timestamp(long nowInUnixMilliseconds)
        {
            TimestampInUnixMilliseconds = nowInUnixMilliseconds;

            return this;
        }
    }
}


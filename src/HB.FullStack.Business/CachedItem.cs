using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Repository
{
    public abstract class CachedItem<TResult>
        where TResult : class
    {
        private CachedItem() { ResourceType = this.GetType().Name; }
        protected CachedItem(params string[] keys) : this()
        {
            CacheKey = ResourceType + keys.ToJoinedString("_");
        }
        public string ResourceType { get; private set; }

        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        public string CacheKey { get; private set; } = null!;

        public TResult? CacheValue { get; private set; }

        public UtcNowTicks UtcTikcs { get; private set; } = UtcNowTicks.Empty;

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

            if (UtcTikcs.IsEmpty())
            {
                throw new ArgumentException(nameof(UtcTikcs));
            }

            return cache.SetAsync<TResult>(
                CacheKey,
                CacheValue,
                UtcTikcs,
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

            if (UtcTikcs.IsEmpty())
            {
                throw new ArgumentException(nameof(UtcTikcs));
            }

            return await cache.RemoveAsync(CacheKey, UtcTikcs).ConfigureAwait(false);
        }

        public CachedItem<TResult> Value(TResult result)
        {
            CacheValue = result;
            return this;
        }

        public CachedItem<TResult> Timestamp(UtcNowTicks utcTicks)
        {
            UtcTikcs = utcTicks;

            return this;
        }
    }
}


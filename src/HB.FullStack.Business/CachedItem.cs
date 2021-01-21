using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Repository
{
    public abstract class CachedItem<TResult> where TResult : class
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

        /// <exception cref="CacheException"></exception>
        /// <exception cref="RepositoryException"></exception>
        public Task<TResult?> GetFromAsync(ICache cache, CancellationToken cancellationToken = default)
        {
            ThrowOnNullOrEmptyCacheKey();

            return cache.GetAsync<TResult>(CacheKey, cancellationToken);
        }

        /// <exception cref="RepositoryException"></exception>
        /// <exception cref="CacheException"></exception>
        public Task SetToAsync(ICache cache, CancellationToken cancellationToken = default)
        {
            ThrowOnNullOrEmptyCacheKey();
            ThrowOnNullCacheValue();
            ThrowOnEmptyUtcTicks();

            return cache.SetAsync<TResult>(
                CacheKey,
                CacheValue!,
                UtcTikcs,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow,
                    SlidingExpiration = SlidingExpiration
                },
                cancellationToken);
        }

        /// <exception cref="CacheException"></exception>
        /// <exception cref="RepositoryException"></exception>
        public async Task<bool> RemoveFromAsync(ICache cache)
        {
            ThrowOnNullOrEmptyCacheKey();
            ThrowOnEmptyUtcTicks();

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

        private void ThrowOnEmptyUtcTicks()
        {
            if (UtcTikcs.IsEmpty())
            {
                throw new RepositoryException(RepositoryErrorCode.UtcTicksNotSet, $"ResourceType:{ResourceType}, CacheKey:{CacheKey}, CacheValue:{CacheValue}");
            }
        }

        private void ThrowOnNullCacheValue()
        {
            if (CacheValue == null)
            {
                throw new RepositoryException(RepositoryErrorCode.CacheValueNotSet, $"ResourceType:{ResourceType}, CacheKey:{CacheKey}");
            }
        }

        private void ThrowOnNullOrEmptyCacheKey()
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                throw new RepositoryException(RepositoryErrorCode.CacheKeyNotSet, $"ResourceType:{ResourceType}");
            }
        }
    }
}


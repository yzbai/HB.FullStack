using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Repository
{
    public abstract class CachedItem
    {
        public string ResourceType => GetType().FullName;

        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        public string CacheKey { get; protected set; } = null!;

        public UtcNowTicks UtcTikcs { get; protected set; } = UtcNowTicks.Empty;

        
    }

    public abstract class CachedItem<TResult> : CachedItem where TResult : class
    {
        public TResult? CacheValue { get; private set; }

        protected CachedItem(params string[] keys)
        {
            CacheKey = ResourceType + keys.ToJoinedString("_");
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


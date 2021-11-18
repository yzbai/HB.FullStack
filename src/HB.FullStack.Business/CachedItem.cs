using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Repository
{
    public abstract class CachedItem
    {
        public string CachedType => GetType().Name;

        /// <summary>
        /// 对于那些无法主动Invalidate的项目，必须设置绝对过期值
        /// 如果设置为null，那么就是需要确保主动invalidation正确
        /// </summary>
        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        public string CacheKey { get; protected set; } = null!;

        /// <summary>
        /// 刚从数据库取出的时间，越贴近数据库取出时间，越好
        /// </summary>
        public UtcNowTicks UtcTicks { get; protected set; } = UtcNowTicks.Empty;
    }

    /// <summary>
    /// 每个CachedItem条目都是独立存在的，有独立的过期日期。
    /// 要确保可以准确的Invalidation
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class CachedItem<TResult> : CachedItem where TResult : class
    {
        public TResult? CacheValue { get; private set; }

        protected CachedItem(object? key)
        {
            CacheKey = $"{CachedType}_{key ?? "null"}";
        }

        public CachedItem<TResult> Value(TResult result)
        {
            CacheValue = result;
            return this;
        }

        public CachedItem<TResult> Timestamp(UtcNowTicks utcTicks)
        {
            UtcTicks = utcTicks;

            return this;
        }
    }
}
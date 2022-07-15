using System;

namespace HB.FullStack.Common.Cache
{

    /// <summary>
    /// 每个CachedItem条目都是独立存在的，有独立的过期日期。
    /// 要确保可以准确的Invalidation
    /// </summary>
    public abstract class CachedItem<TResult> : ICachedItem
    {
        public string CachedType => GetType().Name;

        public string CacheKey { get; protected set; } = null!;

        public TResult? CacheValue { get; private set; }

        public long Timestamp { get; protected set; } = -1;


        /// <summary>
        /// 对于那些无法主动Invalidate的项目，必须设置绝对过期值
        /// 如果设置为null，那么就是需要确保主动invalidation正确
        /// </summary>
        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        /// <summary>
        /// 强迫程序员填写，作为提醒
        /// </summary>
        public abstract string WhenToInvalidate { get; }

        protected CachedItem(object? key)
        {
            CacheKey = $"{CachedType}_{key ?? "null"}";
        }

        public CachedItem<TResult> SetValue(TResult result)
        {
            CacheValue = result;
            return this;
        }

        public CachedItem<TResult> SetTimestamp(long timestamp)
        {
            Timestamp = timestamp;

            return this;
        }
    }
}
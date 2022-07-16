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

        #region Settings

        //TODO: 其实没必要将这些设置每一个实例都声明。一个类有一份就行了，要是static 可以 override就好了。
        //或者移到一个统一设置的地方，可能会变得复杂起来，目前先这样。

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

        #endregion

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
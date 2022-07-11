using System;

namespace HB.FullStack.Repository
{
    /// <summary>
    /// 一个collection中的条目
    /// </summary>
    public abstract class CachedCollectionItem
    {
        public string CollectionKey => GetType().Name;

        public string ItemKey { get; protected set; } = null!;

        public long Timestamp { get; protected set; } = -1;

        /// <summary>
        /// 对于那些无法主动Invalidate的项目，必须设置绝对过期值
        /// 如果设置为null，那么就是需要确保主动invalidation正确
        /// </summary>
        public abstract TimeSpan AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        protected CachedCollectionItem(string itemKey)
        {
            ItemKey = itemKey;
        }

        public static string GetCollectionKey<T>() where T : CachedCollectionItem
        {
            return typeof(T).Name;
        }
    }

    public abstract class CachedCollectionItem<TResult> : CachedCollectionItem where TResult : class
    {
        public TResult? ItemValue { get; private set; }

        protected CachedCollectionItem(string itemKey) : base(itemKey)
        {
        }

        public CachedCollectionItem<TResult> SetValue(TResult result)
        {
            ItemValue = result;
            return this;
        }

        public CachedCollectionItem<TResult> SetTimestamp(long timestamp)
        {
            Timestamp = timestamp;

            return this;
        }
    }
}
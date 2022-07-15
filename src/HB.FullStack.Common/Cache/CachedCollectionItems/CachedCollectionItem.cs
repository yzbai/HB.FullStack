using System;

namespace HB.FullStack.Common.Cache
{
    /// <summary>
    /// 一个collection中的条目. 可以将整个Collection Invalidate掉。相当于比CachedItem多了一个范围。
    /// </summary>
    public abstract class CachedCollectionItem<TResult> : ICachedCollectionItem
    {
        public string CollectionKey => ICachedCollectionItem.GetCollectionKey(GetType());

        public string ItemKey { get; protected set; } = null!;

        public TResult? ItemValue { get; private set; }

        public long Timestamp { get; protected set; } = -1;

        /// <summary>
        /// 对于那些无法主动Invalidate的项目，必须设置绝对过期值
        /// 如果设置为null，那么就是需要确保主动invalidation正确
        /// </summary>
        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }

        public abstract TimeSpan? SlidingExpiration { get; }

        /// 强迫程序员填写，作为提醒
        /// </summary>
        public abstract string WhenToInvalidate { get; }

        protected CachedCollectionItem(string itemKey)
        {
            ItemKey = itemKey;
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
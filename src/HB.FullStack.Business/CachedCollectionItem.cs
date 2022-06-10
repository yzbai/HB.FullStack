using System;

namespace HB.FullStack.Repository
{
    public abstract class CachedCollectionItem
    {
        public string CollectionKey => GetType().Name;

        public string ItemKey { get; protected set; } = null!;

        public UtcNowTicks UtcTicks { get; protected set; } = UtcNowTicks.Empty;

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

        public CachedCollectionItem<TResult> Value(TResult result)
        {
            ItemValue = result;
            return this;
        }

        public CachedCollectionItem<TResult> Timestamp(UtcNowTicks utcTicks)
        {
            UtcTicks = utcTicks;

            return this;
        }
    }
}
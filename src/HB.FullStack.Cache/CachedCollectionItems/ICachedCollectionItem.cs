using System;

namespace HB.FullStack.Cache
{
    public interface ICachedCollectionItem
    {

        public static string GetCollectionKey(Type cachedCollectionItemType)
        {
            return cachedCollectionItemType.Name;
        }

        TimeSpan? AbsoluteExpirationRelativeToNow { get; }
        string CollectionKey { get; }
        string ItemKey { get; }
        TimeSpan? SlidingExpiration { get; }
        long Timestamp { get; }
    }
}
using System;

namespace HB.FullStack.Common.Cache
{
    public interface ICachedCollectionItem
    {

        public static string GetCollectionKey(Type type)
        {
            return type.Name;
        }

        TimeSpan? AbsoluteExpirationRelativeToNow { get; }
        string CollectionKey { get; }
        string ItemKey { get; }
        TimeSpan? SlidingExpiration { get; }
        long Timestamp { get; }
    }
}
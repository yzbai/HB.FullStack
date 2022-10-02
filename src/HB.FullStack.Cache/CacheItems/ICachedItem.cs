using System;

namespace HB.FullStack.Cache
{
    public interface ICachedItem
    {
        TimeSpan? AbsoluteExpirationRelativeToNow { get; }
        string CachedType { get; }
        string CacheKey { get; }
        TimeSpan? SlidingExpiration { get; }
        long Timestamp { get; }
    }
}
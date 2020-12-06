using HB.FullStack.Common.Entities;

namespace HB.FullStack.Cache.Test
{
    [CacheEntity]
    public class Book : Entity
    {
        [CacheDimensionKey]
        public string Name { get; set; } = null!;

        [CacheDimensionKey]
        public long BookID { get; set; }

        public string? Publisher { get; set; }

        public double Price { get; set; }
    }
}

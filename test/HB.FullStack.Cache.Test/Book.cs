using HB.FullStack.Common.Entities;

namespace HB.FullStack.Cache.Test
{
    [CacheEntity(IsBatchEnabled = true)]
    public class Book : Entity
    {
        [CacheDifferentDimensionKey]
        public string Name { get; set; } = null!;

        [CacheDifferentDimensionKey]
        public long BookID { get; set; }

        public string? Publisher { get; set; }

        public double Price { get; set; }
    }
}

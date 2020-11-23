using HB.Framework.Common.Entities;

namespace HB.Framework.Cache.Test
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

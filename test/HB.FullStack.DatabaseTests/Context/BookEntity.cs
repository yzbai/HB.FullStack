using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.DatabaseTests.Data
{
    [CacheEntity]
    public class BookEntity : DatabaseEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Book : DatabaseEntity
    {
        [CacheDimensionKey]
        public string Name { get; set; } = null!;

        [CacheDimensionKey]
        public long BookID { get; set; }

        public string? Publisher { get; set; }

        public double Price { get; set; }
    }
}

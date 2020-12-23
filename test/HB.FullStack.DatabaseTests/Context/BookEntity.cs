using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.DatabaseTests.Data
{
    [CacheEntity]
    public class BookEntity : IdGenGuidEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Book : IdGenGuidEntity
    {
        [CacheDimensionKey]
        public string Name { get; set; } = null!;

        [CacheDimensionKey]
        public long BookID { get; set; }

        public string? Publisher { get; set; }

        public double Price { get; set; }
    }

    [CacheEntity]
    public class BookEntity_Client : AutoIncrementIdGuidEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Book_Client : AutoIncrementIdGuidEntity
    {
        [CacheDimensionKey]
        public string Name { get; set; } = null!;

        [CacheDimensionKey]
        public long BookID { get; set; }

        public string? Publisher { get; set; }

        public double Price { get; set; }
    }
}

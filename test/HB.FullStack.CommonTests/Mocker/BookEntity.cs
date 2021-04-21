using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.Entities;

namespace HB.FullStack.DatabaseTests.Data
{
    [CacheEntity]
    public class BookEntity : IdGenEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Book : IdGenEntity
    {
        [EntityProperty]
        [CacheDimensionKey]
        public string Name { get; set; } = null!;

        [EntityProperty]
        [CacheDimensionKey]
        public long BookID { get; set; }

        [EntityProperty]
        public string? Publisher { get; set; }

        [EntityProperty]
        public double Price { get; set; }
    }

    [CacheEntity]
    public class BookEntity_Client : IdGenEntity
    {

        [EntityProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Book_Client : IdGenEntity
    {
        [CacheDimensionKey]
        [EntityProperty]
        public string Name { get; set; } = null!;

        [EntityProperty]
        [CacheDimensionKey]
        public long BookID { get; set; }

        [EntityProperty]
        public string? Publisher { get; set; }

        [EntityProperty]
        public double Price { get; set; }
    }
}

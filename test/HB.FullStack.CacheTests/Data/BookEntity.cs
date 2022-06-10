using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.Entities;

using MessagePack;

namespace HB.FullStack.CacheTests
{
    [CacheEntity]
    public class BookEntity : FlackIdEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Guid_BookEntity : GuidEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Book : FlackIdEntity
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
    public class BookEntity_Client : FlackIdEntity
    {
        [EntityProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    public class Book_Client : FlackIdEntity
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
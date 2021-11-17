using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.Entities;

using MessagePack;

namespace HB.FullStack.CacheTests
{
    [CacheEntity]
    [MessagePackObject]
    public class BookEntity : FlackIdEntity
    {
        [EntityProperty]
        [Key(7)]
        public string Name { get; set; } = default!;

        [EntityProperty]
        [Key(8)]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    [MessagePackObject]
    public class Guid_BookEntity : GuidEntity
    {
        [EntityProperty]
        [Key(7)]
        public string Name { get; set; } = default!;

        [EntityProperty]
        [Key(8)]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    [MessagePackObject]
    public class Book : FlackIdEntity
    {
        [EntityProperty]
        [CacheDimensionKey]
        [Key(7)]
        public string Name { get; set; } = null!;

        [EntityProperty]
        [CacheDimensionKey]
        [Key(8)]
        public long BookID { get; set; }

        [EntityProperty]
        [Key(9)]
        public string? Publisher { get; set; }

        [EntityProperty]
        [Key(10)]
        public double Price { get; set; }
    }

    [CacheEntity]
    [MessagePackObject]
    public class BookEntity_Client : FlackIdEntity
    {
        [EntityProperty(NeedIndex = true)]
        [Key(7)]
        public string Name { get; set; } = default!;

        [EntityProperty]
        [Key(8)]
        public double Price { get; set; } = default!;
    }

    [CacheEntity]
    [MessagePackObject]
    public class Book_Client : FlackIdEntity
    {
        [CacheDimensionKey]
        [EntityProperty]
        [Key(7)]
        public string Name { get; set; } = null!;

        [EntityProperty]
        [CacheDimensionKey]
        [Key(8)]
        public long BookID { get; set; }

        [EntityProperty]
        [Key(9)]
        public string? Publisher { get; set; }

        [EntityProperty]
        [Key(10)]
        public double Price { get; set; }
    }
}
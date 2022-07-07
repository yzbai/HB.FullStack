using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

using MessagePack;

namespace HB.FullStack.CacheTests
{
    [CacheModel]
    public class BookModel : FlackIdModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Guid_BookModel : GuidModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book : FlackIdModel
    {
        [ModelProperty]
        [CacheDimensionKey]
        public string Name { get; set; } = null!;

        [ModelProperty]
        [CacheDimensionKey]
        public long BookID { get; set; }

        [ModelProperty]
        public string? Publisher { get; set; }

        [ModelProperty]
        public double Price { get; set; }
    }

    [CacheModel]
    public class BookModel_Client : FlackIdModel
    {
        [ModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book_Client : FlackIdModel
    {
        [CacheDimensionKey]
        [ModelProperty]
        public string Name { get; set; } = null!;

        [ModelProperty]
        [CacheDimensionKey]
        public long BookID { get; set; }

        [ModelProperty]
        public string? Publisher { get; set; }

        [ModelProperty]
        public double Price { get; set; }
    }
}
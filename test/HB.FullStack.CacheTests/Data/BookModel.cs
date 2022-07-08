using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

using MessagePack;

namespace HB.FullStack.CacheTests
{
    [CacheModel]
    public class BookModel : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Guid_BookModel : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        [CacheDimensionKey]
        public string Name { get; set; } = null!;

        [DatabaseModelProperty]
        [CacheDimensionKey]
        public long BookID { get; set; }

        [DatabaseModelProperty]
        public string? Publisher { get; set; }

        [DatabaseModelProperty]
        public double Price { get; set; }
    }

    [CacheModel]
    public class BookModel_Client : FlackIdDatabaseModel
    {
        [DatabaseModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book_Client : FlackIdDatabaseModel
    {
        [CacheDimensionKey]
        [DatabaseModelProperty]
        public string Name { get; set; } = null!;

        [DatabaseModelProperty]
        [CacheDimensionKey]
        public long BookID { get; set; }

        [DatabaseModelProperty]
        public string? Publisher { get; set; }

        [DatabaseModelProperty]
        public double Price { get; set; }
    }
}
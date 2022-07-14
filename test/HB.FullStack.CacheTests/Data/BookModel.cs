using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

using MessagePack;

namespace HB.FullStack.CacheTests
{
    [CacheModel]
    public class BookModel : TimestampFlackIdDBModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Guid_BookModel : TimestampGuidDBModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book : TimestampFlackIdDBModel
    {
        [DatabaseModelProperty]
        [CacheModelAltKey]
        public string Name { get; set; } = null!;

        [DatabaseModelProperty]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DatabaseModelProperty]
        public string? Publisher { get; set; }

        [DatabaseModelProperty]
        public double Price { get; set; }
    }

    [CacheModel]
    public class BookModel_Client : TimestampFlackIdDBModel
    {
        [DatabaseModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book_Client : TimestampFlackIdDBModel
    {
        [CacheModelAltKey]
        [DatabaseModelProperty]
        public string Name { get; set; } = null!;

        [DatabaseModelProperty]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DatabaseModelProperty]
        public string? Publisher { get; set; }

        [DatabaseModelProperty]
        public double Price { get; set; }
    }
}
using System;

using HB.FullStack.Database.DBModels;

namespace HB.FullStack.CacheTests
{
    [CacheModel]
    public class BookModel : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Guid_BookModel : TimestampGuidDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        [CacheModelAltKey]
        public string Name { get; set; } = null!;

        [DBModelProperty]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DBModelProperty]
        public string? Publisher { get; set; }

        [DBModelProperty]
        public double Price { get; set; }
    }

    [CacheModel]
    public class BookModel_Client : TimestampFlackIdDBModel
    {
        [DBModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book_Client : TimestampFlackIdDBModel
    {
        [CacheModelAltKey]
        [DBModelProperty]
        public string Name { get; set; } = null!;

        [DBModelProperty]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DBModelProperty]
        public string? Publisher { get; set; }

        [DBModelProperty]
        public double Price { get; set; }
    }
}
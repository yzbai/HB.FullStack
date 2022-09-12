using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.CacheTests
{
    [CacheModel]
    public class BookModel : TimestampFlackIdDbModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Guid_BookModel : TimestampGuidDbModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book : TimestampFlackIdDbModel
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
    public class BookModel_Client : TimestampFlackIdDbModel
    {
        [DBModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheModel]
    public class Book_Client : TimestampFlackIdDbModel
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
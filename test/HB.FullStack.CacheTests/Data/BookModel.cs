using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

using MessagePack;

namespace HB.FullStack.CacheTests
{
    [CacheThisModel]
    public class BookModel : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheThisModel]
    public class Guid_BookModel : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheThisModel]
    public class Book : FlackIdDatabaseModel
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

    [CacheThisModel]
    public class BookModel_Client : FlackIdDatabaseModel
    {
        [DatabaseModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    [CacheThisModel]
    public class Book_Client : FlackIdDatabaseModel
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
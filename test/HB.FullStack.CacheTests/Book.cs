using System;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.CacheTests
{
    [CacheModel]
    public class Book : DbModel<long>, ITimestamp
    {
        [DbField]
        [CacheModelAltKey]
        public string Name { get; set; } = null!;

        [DbField]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}
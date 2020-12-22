using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using OrmBenchmark.Core;

namespace HB.FullStack.Benchmark.Database
{
    [Database(TableName = "Posts")]
    public class Post : DatabaseEntity, IPost
    {
        [EntityProperty(MaxLength = 2000)]
        public string Text { get; set; } = null!;

        [EntityProperty]
        public long CreationDate { get; set; }

        [EntityProperty]
        public long LastChangeDate { get; set; }

        [EntityProperty]
        public int? Counter1 { get; set; }
        [EntityProperty]
        public int? Counter2 { get; set; }
        [EntityProperty]
        public int? Counter3 { get; set; }
        [EntityProperty]
        public int? Counter4 { get; set; }
        [EntityProperty]
        public int? Counter5 { get; set; }
        [EntityProperty]
        public int? Counter6 { get; set; }
        [EntityProperty]
        public int? Counter7 { get; set; }
        [EntityProperty]
        public int? Counter8 { get; set; }
        [EntityProperty]
        public int? Counter9 { get; set; }

    }
}

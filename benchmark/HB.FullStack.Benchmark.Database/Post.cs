using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Entities;

using OrmBenchmark.Core;

namespace HB.FullStack.Benchmark.Database
{
    [DatabaseEntity(TableName = "Posts")]
    public class Post : Entity, IPost
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

global using static HB.FullStack.BaseTest.BaseTestClass;

using System;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbModel(DbSchema_Mysql)]
    public class AutoIdBTTimestamp : DbModel2<long>, ITimestamp
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);

        public int Age { get; set; } = 77;

        public long Timestamp { get; set; }

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }

        public override bool Deleted { get; set; }

        public override string LastUser { get; set; } = null!;
    }


    [DbModel(DbSchema_Mysql)]
    public class AutoIdBT : DbModel2<long>
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 66;

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string LastUser { get; set; } = null!;
    }

}

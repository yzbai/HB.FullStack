using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.Sqlites
{

    [DbTable(DbSchema_Sqlite)]
    public class AutoIdBTTimestamp: TimestampAutoIncrementIdDbModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 77;

    }

    [DbTable(DbSchema_Sqlite)]
    public class AutoIdBTTimeless: TimelessAutoIncrementIdDbModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 66;

    }
}

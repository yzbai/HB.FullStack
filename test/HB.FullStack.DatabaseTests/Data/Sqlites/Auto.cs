using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data.Sqlites
{

    [DbModel(DbSchema_Sqlite)]
    public class AutoIdBTTimestamp: TimestampAutoIncrementIdDbModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 77;

    }

    [DbModel(DbSchema_Sqlite)]
    public class AutoIdBTTimeless: TimelessAutoIncrementIdDbModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 66;

    }
}

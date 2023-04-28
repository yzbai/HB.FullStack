global using static HB.FullStack.BaseTest.BaseTestClass;
using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbTable(DbSchema_Mysql)]
    public class AutoIdBTTimestamp : TimestampAutoIncrementIdDbModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 77;

    }

    [DbTable(DbSchema_Mysql)]
    public class AutoIdBTTimeless : TimelessAutoIncrementIdDbModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 66;

    }
}

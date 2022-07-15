using System;

using HB.FullStack.Database.DBModels;

namespace HB.FullStack.DatabaseTests.Data
{
    public class AutoIdBTTimestamp : TimestampAutoIncrementIdDBModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 77;

    }

    public class AutoIdBTTimeless : TimelessAutoIncrementIdDBModel
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);
        public int Age { get; set; } = 66;

    }
}

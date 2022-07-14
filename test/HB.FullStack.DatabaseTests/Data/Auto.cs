using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DatabaseModels;

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

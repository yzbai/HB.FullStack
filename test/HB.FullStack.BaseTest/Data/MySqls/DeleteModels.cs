using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbTable(DbSchema_Mysql)]
    public class DeleteTimestampModel : TimestampGuidDbModel
    {
        public string? Name { get; set; }
    }

    [DbTable(DbSchema_Mysql)]
    public class DeleteTimelessModel : TimelessGuidDbModel
    {
        public string? Name { get; set; }
    }
}

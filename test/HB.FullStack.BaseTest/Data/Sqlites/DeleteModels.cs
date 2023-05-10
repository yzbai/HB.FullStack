using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.Sqlites
{
    [DbModel(DbSchema_Sqlite)]
    public class DeleteTimestampModel : TimestampGuidDbModel
    {
        public string? Name { get; set; }
    }

    [DbModel(DbSchema_Sqlite)]
    public class DeleteTimelessModel : TimelessGuidDbModel
    {
        public string? Name { get; set; }
    }
}

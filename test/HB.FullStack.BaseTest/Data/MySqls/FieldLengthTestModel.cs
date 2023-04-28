using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbTable(DbSchema_Mysql)]
    public class FieldLengthTestModel : TimestampAutoIncrementIdDbModel
    {
        [DbField(MaxLength = 10)]
        public string? Content { get; set; }
    }
}

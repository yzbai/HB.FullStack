using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateSelectSql(params DbModelDef[] modelDefs)
        {
            StringBuilder builder = new StringBuilder("SELECT ");

            foreach (DbModelDef modelDef in modelDefs)
            {
                string DbTableReservedName = modelDef.DbTableReservedName;

                foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
                {
                    builder.Append($"{DbTableReservedName}.{propertyDef.DbReservedName},");
                }
            }

            builder.RemoveLast();

            return builder.ToString();
        }
    }
}

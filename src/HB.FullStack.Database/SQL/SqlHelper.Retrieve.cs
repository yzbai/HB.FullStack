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
            string cacheKey = GetCachedSqlKey(modelDefs, null, null);

            if (SqlCache.TryGetValue(cacheKey, out string? sql))
            {
                return sql;
            }

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

            sql = builder.ToString();

            SqlCache[cacheKey] = sql;

            return sql;
        }
    }
}

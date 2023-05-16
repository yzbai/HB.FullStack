using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateAddSql(DbModelDef modelDef, string placeHolder = "0", bool returnId = true)
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { placeHolder, returnId });

            if(SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            StringBuilder args = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    continue;
                }

                args.Append($"{propertyDef.DbReservedName},");

                values.Append($"{propertyDef.DbParameterizedName}_{placeHolder},");
            }

            args.RemoveLast();
            values.RemoveLast();

            string returnIdStatement = returnId && modelDef.IdType == DbModelIdType.AutoIncrementLongId 
                ? $"select {LastInsertIdStatement(modelDef.EngineType)};" 
                : string.Empty;

            sql = $"insert into {modelDef.DbTableReservedName}({args}) values({values});{returnIdStatement}";

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchAddSql(DbModelDef modelDef, int modelCount)
        {
            var batchSqlReturnType = modelDef.IdType == DbModelIdType.AutoIncrementLongId 
                ? BatchSqlReturnType.ReturnLastInsertIds 
                : BatchSqlReturnType.None;

                return CreateBatchSql(
                    batchSqlReturnType,
                    modelDef,
                    modelCount,
                    () => CreateAddSql(modelDef, "{0}", false));
        }
    }
}

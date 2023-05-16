using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateInsertSql(DbModelDef modelDef, string placeHolder = "0", bool returnId = true)
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

        public static string CreateBatchInsertSql(DbModelDef modelDef, int modelCount)
        {
            var batchSqlReturnType = modelDef.IdType == DbModelIdType.AutoIncrementLongId 
                ? BatchSqlReturnType.ReturnLastInsertIds 
                : BatchSqlReturnType.None;

                return CreateBatchSqlUsingTemplate(
                    batchSqlReturnType,
                    modelDef,
                    modelCount,
                    () => CreateInsertSql(modelDef, "{0}", false));

            //DbEngineType engineType = modelDef.EngineType;

            //string cacheKey = modelDef.FullName + nameof(CreateBatchInsertSql);

            //if (!BatchSqlTemplateCache.TryGetValue(cacheKey, out string? insertTemplate))
            //{
            //    insertTemplate = CreateInsertSql(modelDef, "{0}", false);
            //    BatchSqlTemplateCache[cacheKey] = insertTemplate;
            //}

            //StringBuilder sqlBuilder = new StringBuilder();

            //sqlBuilder.Append(Transaction_Begin(engineType));

            //if (modelDef.IdType == DbModelIdType.AutoIncrementLongId)
            //{
            //    string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            //    sqlBuilder.Append(TempTable_Drop(tempTableName, engineType));
            //    sqlBuilder.Append(TempTable_Create_Id(tempTableName, engineType));

            //    for (int i = 0; i < modelCount; ++i)
            //    {
            //        sqlBuilder.AppendFormat(insertTemplate, i);
            //        sqlBuilder.Append($"{TempTable_Insert_Id(tempTableName, LastInsertIdStatement(engineType), engineType)}");
            //    }

            //    sqlBuilder.Append(TempTable_Select_Id(tempTableName, engineType));

            //    sqlBuilder.Append(TempTable_Drop(tempTableName, engineType));
            //}
            //else
            //{
            //    for (int i = 0; i < modelCount; ++i)
            //    {
            //        sqlBuilder.AppendFormat(insertTemplate, i);
            //    }
            //}

            //sqlBuilder.Append(Transaction_Commit(engineType));

            //return sqlBuilder.ToString();
        }
    }
}

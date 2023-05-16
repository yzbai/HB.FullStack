/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateUpdateIgnoreConflictCheckSql(DbModelDef modelDef, string placeHolder = "0")
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { placeHolder });

            if(SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            //add Primary Check Where
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{placeHolder}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                """;

            sql = $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, placeHolder)} WHERE {where};";

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateUpdateUsingTimestampSql(DbModelDef modelDef, string placeHolder = "0")
        {
            if (!modelDef.IsTimestamp)
            {
                throw DbExceptions.ConflictCheckError($"Update Using Timestamp but not a timestamp model. {modelDef.FullName}");
            }

            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { placeHolder });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{placeHolder}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                AND
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{OLD_PROPERTY_VALUE_SUFFIX}_{placeHolder}
                """;

            sql = $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, placeHolder)} WHERE {where};";

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchUpdateUsingTimestampSql(DbModelDef modelDef, int modelCount)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                modelCount,
                () => CreateUpdateUsingTimestampSql(modelDef, "{0}"));
        }

        public static string CreateBatchUpdateIgnoreConflictCheckSql(DbModelDef modelDef, int modelCount)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                modelCount,
                () => CreateUpdateIgnoreConflictCheckSql(modelDef, "{0}"));
        }

        private static StringBuilder GetUpdateAssignments(DbModelDef modelDef, string placeHolder)
        {
            StringBuilder assignments = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (propertyDef.IsPrimaryKey)
                {
                    continue;
                }

                assignments.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{placeHolder},");
            }

            assignments.RemoveLast();
            return assignments;
        }
    }
}
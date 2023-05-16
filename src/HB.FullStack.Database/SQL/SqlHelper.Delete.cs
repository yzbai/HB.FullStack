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
        public static string CreateDeleteIgnoreConflictCheckSql(DbModelDef modelDef, bool trulyDeleted, string placeHolder = "0")
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { trulyDeleted, placeHolder });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{placeHolder}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                """;

            if (trulyDeleted)
            {
                sql = $"delete from {modelDef.DbTableReservedName} where {where};";
            }
            else
            {
                StringBuilder assignments = new StringBuilder($"""
                    {modelDef.DeletedPropertyDef.DbReservedName}=1,
                    {modelDef.LastUserPropertyDef.DbReservedName}={modelDef.LastUserPropertyDef.DbParameterizedName}_{placeHolder}
                    """);

                if (modelDef.IsTimestamp)
                {
                    assignments.Append($", {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{placeHolder} ");
                }

                sql = $"update {modelDef.DbTableReservedName} set {assignments} where {where};";
            }

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchDeleteIgnoreConflictCheckSql(DbModelDef modelDef, bool trulyDeleted, int modelCount)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                modelCount,
                () => CreateDeleteIgnoreConflictCheckSql(modelDef, trulyDeleted, "{0}"));
        }

        public static string CreateDeleteUsingTimestampSql(DbModelDef modelDef, bool trulyDeleted, string placeHolder = "0")
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { trulyDeleted, placeHolder });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{placeHolder}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                AND
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{placeHolder}
                """;

            if (trulyDeleted)
            {
                sql = $"delete from {modelDef.DbTableReservedName} where {where};";
            }
            else
            {
                sql = $"""
                    update {modelDef.DbTableReservedName} set
                    {modelDef.DeletedPropertyDef.DbReservedName}=1,
                    {modelDef.LastUserPropertyDef.DbReservedName}={DbParameterName_LastUser}_{placeHolder},
                    {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUE_SUFFIX}_{placeHolder}
                    where {where};
                    """;
            }

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchDeleteUsingTimestampSql(DbModelDef modelDef, bool trulyDeleted, int modelCount)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                modelCount,
                () => CreateDeleteUsingTimestampSql(modelDef, trulyDeleted, "{0}"));
        }

        public static string CreateDeleteUsingOldNewCompareSql(DbModelDef modelDef, bool trulyDeleted, string placeHolder = "0")
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { trulyDeleted, placeHolder });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            StringBuilder where = new StringBuilder();

            foreach (var propertyDef in modelDef.PropertyDefs)
            {
                where.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{placeHolder} AND");
            }

            where.RemoveLast(3);

            if (trulyDeleted)
            {
                sql = $"delete from {modelDef.DbTableReservedName} where {where};";
            }
            else
            {

                StringBuilder assignments = new StringBuilder($"""
                {modelDef.DeletedPropertyDef.DbReservedName}=1,
                {modelDef.LastUserPropertyDef.DbReservedName}={modelDef.LastUserPropertyDef.DbParameterizedName}_{NEW_PROPERTY_VALUE_SUFFIX}_{placeHolder}
                """);

                if (modelDef.IsTimestamp)
                {
                    assignments.Append($", {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUE_SUFFIX}_{placeHolder} ");
                }

                sql = $"update {modelDef.DbTableReservedName} set {assignments} where {where};";
            }

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchDeleteUsingOldNewCompareSql(DbModelDef modelDef, bool trulyDeleted, int modelCount)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                modelCount,
                () => CreateDeleteUsingOldNewCompareSql(modelDef, trulyDeleted, "{0}"));
        }
    }
}
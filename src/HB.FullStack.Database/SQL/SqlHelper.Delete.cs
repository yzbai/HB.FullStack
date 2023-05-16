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
        ///// <summary>
        ///// 针对Client
        ///// </summary>
        //public static string CreateUpdateDeletedSql(DbModelDef modelDef, int number = 0)
        //{
        //    return $"update {modelDef.DbTableReservedName} set  {deletedProperty.DbReservedName}={deletedProperty.DbParameterizedName}_{number},{lastNameProperty.DbReservedName}={lastNameProperty.DbParameterizedName}_{number}";
        //}

        //public static string CreateDeleteSql(DbModelDef modelDef, int number = 0)
        //{
        //    return $"delete from {modelDef.DbTableReservedName} ";
        //}

        //public static string CreateDeleteByPropertiesSql(DbModelDef modelDef, IEnumerable<string> propertyNames, int number = 0)
        //{
        //    StringBuilder where = new StringBuilder();

        //    foreach (string propertyName in propertyNames)
        //    {
        //        DbModelPropertyDef propertyDef = modelDef.GetDbPropertyDef(propertyName) ?? throw DbExceptions.PropertyNotFound(modelDef.FullName, propertyName);

        //        where.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number} ");
        //        where.Append("AND");
        //    }

        //    where.RemoveLast(3);// "AND".Length

        //    return $"delete from {modelDef.DbTableReservedName} where {where};";
        //}

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

        public static string CreateBatchDeleteIgnoreConflictCheckSql2(DbModelDef modelDef, bool trulyDeleted, int modelCount)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                modelCount,
                () => CreateDeleteIgnoreConflictCheckSql(modelDef, trulyDeleted, "{0}"));
        }

        public static string CreateDeleteUsingTimestampSql(DbModelDef modelDef, bool trulyDeleted, int number = 0)
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { trulyDeleted, number });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                AND
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{number}
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
                    {modelDef.LastUserPropertyDef.DbReservedName}={DbParameterName_LastUser}_{number},
                    {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUE_SUFFIX}_{number}
                    where {where};
                    """;
            }

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateDeleteUsingOldNewCompareSql(DbModelDef modelDef, bool trulyDeleted, int number = 0)
        {
            //StringBuilder where = new StringBuilder($"""
            //    {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{OLD_PROPERTY_VALUE_SUFFIX}_{number}
            //    AND
            //    {modelDef.DeletedPropertyDef.DbReservedName}=0
            //    """);

            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { trulyDeleted, number });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            StringBuilder where = new StringBuilder();

            foreach (var propertyDef in modelDef.PropertyDefs)
            {
                where.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number} AND");
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
                {modelDef.LastUserPropertyDef.DbReservedName}={modelDef.LastUserPropertyDef.DbParameterizedName}_{NEW_PROPERTY_VALUE_SUFFIX}_{number}
                """);

                if (modelDef.IsTimestamp)
                {
                    assignments.Append($", {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUE_SUFFIX}_{number} ");
                }

                sql = $"update {modelDef.DbTableReservedName} set {assignments} where {where};";
            }

            SqlCache[cacheKey] = sql;

            return sql;
        }
    }
}
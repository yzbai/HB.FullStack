/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateUpdatePropertiesIgnoreConflictCheckSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, propertyNames, new List<object?> { number });

            if(SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            //assignments
            StringBuilder assignments = GetUpdatePropertiesAssignments(modelDef.EngineType, propertyNames,null, number);

            //where
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                """;

            sql = $"UPDATE {modelDef.DbTableReservedName} SET {assignments} WHERE {where};";

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchUpdatePropertiesIgnoreConflictCheckSql(DbModelDef modelDef, IList<IList<string>> propertyNamesList)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                propertyNamesList.Cast<object?>().ToList(),
                (number, propertyNamesObj) =>
                {
                    IList<string>? propertyNames = propertyNamesObj as IList<string>;

                    propertyNames.ThrowIfNullOrEmpty(nameof(propertyNames));

                    return CreateUpdatePropertiesIgnoreConflictCheckSql(modelDef, propertyNames, number);
                });
        }

        public static string CreateUpdatePropertiesUsingTimestampSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, propertyNames, new List<object?> { number });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            //assignments
            StringBuilder assignments = GetUpdatePropertiesAssignments(modelDef.EngineType, propertyNames, null, number);

            //where
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                AND
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{SqlHelper.OLD_PARAMETER_SUFFIX}{number}
                """;

            sql= $"UPDATE {modelDef.DbTableReservedName} SET {assignments} WHERE {where};";

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchUpdatePropertiesUsingTimestampSql(DbModelDef modelDef, IList<IList<string>> propertyNamesList)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                propertyNamesList.Cast<object?>().ToList(),
                (number, propertyNamesObj) =>
                {
                    //sql
                    //Remark: 由于packs中的pack可能是各种各样的，所以这里不能用模板，像Update那样
                    //TODO: 如果限制packs中所有PropertyNames都相同，可以提高性能

                    IList<string>? propertyNames = propertyNamesObj as IList<string>;

                    propertyNames.ThrowIfNullOrEmpty(nameof(propertyNames));

                    return CreateUpdatePropertiesUsingTimestampSql(modelDef, propertyNames, number);
                });
        }

        public static string CreateUpdatePropertiesUsingOldNewCompareSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, propertyNames, new List<object?> { number });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            DbEngineType engineType = modelDef.EngineType;

            StringBuilder assignments = GetUpdatePropertiesAssignments(
                engineType,
                new List<string>(propertyNames) { nameof(BaseDbModel.LastUser) },
                SqlHelper.NEW_PARAMETER_SUFFIX,
                number);

            StringBuilder where = new StringBuilder($"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={DbParameterName_PrimaryKey}_{number}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                """);

            foreach (string propertyName in propertyNames)
            {
                where.Append($" AND {GetReserved(propertyName, engineType)}={GetParameterized(propertyName)}_{number}");
            }

            sql = $"UPDATE {modelDef.DbTableReservedName} SET {assignments} WHERE {where};";

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchUpdatePropertiesUsingOldNewCompareSql(DbModelDef modelDef, IList<IList<string>> propertyNamesList)
        {
            return CreateBatchSql(
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows,
                modelDef,
                propertyNamesList.Cast<object?>().ToList(),
                (number, propertyNamesObj) =>
                {
                    IList<string>? propertyNames = propertyNamesObj as IList<string>;

                    propertyNames.ThrowIfNullOrEmpty(nameof(propertyNames));

                    return CreateUpdatePropertiesUsingOldNewCompareSql(modelDef, propertyNames, number);
                });
        }

        private static StringBuilder GetUpdatePropertiesAssignments(DbEngineType engineType, IList<string> propertyNames, string? parameterSuffix, int number)
        {
            StringBuilder assignments = new StringBuilder();

            foreach (string propertyName in propertyNames)
            {
                if (propertyName == nameof(DbModel2<long>.Id))
                {
                    continue;
                }

                assignments.Append($" {GetReserved(propertyName, engineType)}={GetParameterized(propertyName)}_{parameterSuffix}{number},");
            }

            assignments.RemoveLast();
            return assignments;
        }
    }
}
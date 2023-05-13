/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateUpdatePropertiesIgnoreConflictCheckSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            //assignments
            StringBuilder assignments = GetUpdatePropertiesAssignments(modelDef.EngineType, propertyNames, number.ToString());

            //where
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0 
                """;

            return $"UPDATE {modelDef.DbTableReservedName} SET {assignments} WHERE {where};";
        }

        public static string CreateUpdatePropertiesUsingTimestampSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            //assignments
            StringBuilder assignments = GetUpdatePropertiesAssignments(modelDef.EngineType, propertyNames, number.ToString());

            //where
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                AND
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{OLD_PROPERTY_VALUE_SUFFIX}_{number}
                """;

            return $"UPDATE {modelDef.DbTableReservedName} SET {assignments} WHERE {where};";
        }

        public static string CreateUpdatePropertiesUsingOldNewCompareSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            DbEngineType engineType = modelDef.EngineType;

            StringBuilder assignments = GetUpdatePropertiesAssignments(engineType, new List<string>(propertyNames) { nameof(BaseDbModel.LastUser) }, $"{NEW_PROPERTY_VALUE_SUFFIX}_{number}");

            StringBuilder where = new StringBuilder($"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{OLD_PROPERTY_VALUE_SUFFIX}_{number} 
                AND 
                {modelDef.DeletedPropertyDef.DbReservedName}=0 
                """);

            foreach (string propertyName in propertyNames)
            {
                where.Append($" AND {GetReserved(propertyName, engineType)}={GetParameterized(propertyName)}_{OLD_PROPERTY_VALUE_SUFFIX}_{number}");
            }

            return $"UPDATE {modelDef.DbTableReservedName} SET {assignments} WHERE {where};";
        }

        private static StringBuilder GetUpdatePropertiesAssignments(DbEngineType engineType, IList<string> propertyNames, string placeHolder)
        {
            StringBuilder assignments = new StringBuilder();

            foreach (string propertyName in propertyNames)
            {
                if (propertyName == nameof(DbModel2<long>.Id))
                {
                    continue;
                }

                assignments.Append($" {GetReserved(propertyName, engineType)}={GetParameterized(propertyName)}_{placeHolder},");
            }

            assignments.RemoveLast();
            return assignments;
        }
    }
}
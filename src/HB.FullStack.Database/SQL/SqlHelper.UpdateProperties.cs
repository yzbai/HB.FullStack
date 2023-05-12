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
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateUpdatePropertiesIgnoreConflictCheck(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            //assignments
            StringBuilder assignments = GetUpdatePropertiesAssignments(modelDef.EngineType, modelDef.PrimaryKeyPropertyDef.Name, propertyNames, number);

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

        public static string CreateUpdatePropertiesUsingTimestampSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            //assignments
            StringBuilder assignments = GetUpdatePropertiesAssignments(modelDef.EngineType, modelDef.PrimaryKeyPropertyDef.Name, propertyNames, number);

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

        private static StringBuilder GetUpdatePropertiesAssignments(DbEngineType engineType, string primaryKeyName, IList<string> propertyNames, int number)
        {
            StringBuilder assignments = new StringBuilder();

            foreach (string propertyName in propertyNames)
            {
                if (propertyName == primaryKeyName)
                {
                    continue;
                }

                assignments.Append($" {GetReserved(propertyName, engineType)}={GetParameterized(propertyName)}_{number},");
            }

            assignments.RemoveLast();
            return assignments;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateUpdateIgnoreConflictCheckSql(DbModelDef modelDef, int number = 0)
        {
            //where
            string where = $"{modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number} AND {modelDef.DeletedPropertyDef.DbReservedName}=0";

            return $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, number)} WHERE {where};";
        }

        private static StringBuilder GetUpdateAssignments(DbModelDef modelDef, int number)
        {
            StringBuilder assignments = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (propertyDef.IsPrimaryKey)
                {
                    continue;
                }

                assignments.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},");
            }

            assignments.RemoveLast();
            return assignments;
        }

        public static string CreateUpdateUsingTimestampSql(DbModelDef modelDef, int number = 0)
        {
            StringBuilder where = new StringBuilder();

            where.Append($"{modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number} AND {modelDef.DeletedPropertyDef.DbReservedName}=0");

            where.Append($" AND {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{OLD_PROPERTY_VALUE_SUFFIX}_{number} ");

            return $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, number)} WHERE {where};";
        }
    }
}

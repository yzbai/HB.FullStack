using System;
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
            //add Primary Check Where
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number} 
                AND 
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                """;

            return $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, number)} WHERE {where};";
        }

        public static string CreateUpdateUsingTimestampSql(DbModelDef modelDef, int number = 0)
        {
            if (!modelDef.IsTimestamp)
            {
                throw DbExceptions.ConflictCheckError($"Update Using Timestamp but not a timestamp model. {modelDef.FullName}");
            }

            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number} 
                AND 
                {modelDef.DeletedPropertyDef.DbReservedName}=0 
                AND 
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{OLD_PROPERTY_VALUE_SUFFIX}_{number} 
                """;

            return $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, number)} WHERE {where};";
        }

        public static string CreateUpdatePropertiesUsingCompareSql(DbModelDef modelDef, IEnumerable<string> propertyNames, int number = 0)
        {
            DbModelPropertyDef primaryKeyProperty = modelDef.PrimaryKeyPropertyDef;
            DbModelPropertyDef deletedProperty = modelDef.GetDbPropertyDef(nameof(BaseDbModel.Deleted))!;
            DbModelPropertyDef lastUserProperty = modelDef.GetDbPropertyDef(nameof(BaseDbModel.LastUser))!;

            StringBuilder args = new StringBuilder();
            args.Append(Invariant($"{lastUserProperty.DbReservedName}={DbParameterName_LastUser}_{NEW_PROPERTY_VALUES_SUFFIX}_{number}"));

            //如果是TimestampDBModel，强迫加上Timestamp字段
            if (modelDef.IsTimestamp)
            {
                DbModelPropertyDef timestampProperty = modelDef.GetDbPropertyDef(nameof(ITimestamp.Timestamp))!;

                args.Append(Invariant($", {timestampProperty.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUES_SUFFIX}_{number}"));
            }

            StringBuilder where = new StringBuilder();

            where.Append(Invariant($" {primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{NEW_PROPERTY_VALUES_SUFFIX}_{number} "));
            where.Append(Invariant($" AND {deletedProperty.DbReservedName}=0 "));

            foreach (string propertyName in propertyNames)
            {
                DbModelPropertyDef propertyDef = modelDef.GetDbPropertyDef(propertyName) ?? throw DbExceptions.PropertyNotFound(modelDef.FullName, propertyName);

                //这里就不加了
                if (propertyName != nameof(ITimestamp.Timestamp))
                {
                    args.Append(Invariant($",{propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{NEW_PROPERTY_VALUES_SUFFIX}_{number}"));
                }

                where.Append(Invariant($" AND  {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{OLD_PROPERTY_VALUE_SUFFIX}_{number}"));
            }

            //TODO: 还是要查验一下found_rows的并发？
            string sql = $"UPDATE {modelDef.DbTableReservedName} SET {args} WHERE {where};";

            if (modelDef.IsTimestamp)
            {
                //" SELECT {FoundUpdateMatchedRows_Statement(engineType)}, {timestampProperty.DbReservedName} FROM {modelDef.DbTableReservedName} WHERE {primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{newSuffix}_{number} AND {deletedProperty.DbReservedName}=0 ";
            }

            return sql;
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
    }
}

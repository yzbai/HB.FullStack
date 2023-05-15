using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

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

        public static string CreateDeleteIgnoreConflictCheckSql(DbModelDef modelDef, bool trulyDeleted, int number = 0)
        {
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number} 
                AND 
                {modelDef.DeletedPropertyDef.DbReservedName}=0 
                """;

            if (trulyDeleted)
            {
                return $"delete from {modelDef.DbTableReservedName} where {where};";
            }

            StringBuilder assignments = new StringBuilder($"""
                {modelDef.DeletedPropertyDef.DbReservedName}=1,
                {modelDef.LastUserPropertyDef.DbReservedName}={modelDef.LastUserPropertyDef.DbParameterizedName}_{number} 
                """);

            if (modelDef.IsTimestamp)
            {
                assignments.Append($", {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{number} ");
            }

            return $"update {modelDef.DbTableReservedName} set {assignments} where {where};";
        }

        public static string CreateDeleteUsingTimestampSql(DbModelDef modelDef, bool trulyDeleted, int number = 0)
        {
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{number}
                AND
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                AND
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{number} 
                """;

            if (trulyDeleted)
            {
                return $"delete from {modelDef.DbTableReservedName} where {where};";
            }

            return $"""
                update {modelDef.DbTableReservedName} set 
                {modelDef.DeletedPropertyDef.DbReservedName}=1,
                {modelDef.LastUserPropertyDef.DbReservedName}={DbParameterName_LastUser}_{number},
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUE_SUFFIX}_{number} 
                where {where};
                """;
        }

        public static string CreateDeleteUsingOldNewCompareSql(DbModelDef modelDef, bool trulyDeleted, int number = 0)
        {
            //StringBuilder where = new StringBuilder($"""
            //    {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{OLD_PROPERTY_VALUE_SUFFIX}_{number} 
            //    AND 
            //    {modelDef.DeletedPropertyDef.DbReservedName}=0 
            //    """);

            StringBuilder where = new StringBuilder();

            foreach (var propertyDef in modelDef.PropertyDefs)
            {
                where.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number} AND");
            }

            where.RemoveLast(3);

            if (trulyDeleted)
            {
                return $"delete from {modelDef.DbTableReservedName} where {where};";
            }

            StringBuilder assignments = new StringBuilder($"""
                {modelDef.DeletedPropertyDef.DbReservedName}=1,
                {modelDef.LastUserPropertyDef.DbReservedName}={modelDef.LastUserPropertyDef.DbParameterizedName}_{NEW_PROPERTY_VALUE_SUFFIX}_{number} 
                """);

            if(modelDef.IsTimestamp)
            {
                assignments.Append($", {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUE_SUFFIX}_{number} ");
            }

            return $"update {modelDef.DbTableReservedName} set {assignments} where {where};";
        }
    }
}

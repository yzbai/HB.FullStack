using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateUpdatePropertiesUsingTimestampSql(DbModelDef modelDef, IList<string> propertyNames, int number = 0)
        {
            //assignments
            StringBuilder assignments = new StringBuilder();

            string primaryKeyName = modelDef.PrimaryKeyPropertyDef.Name;

            foreach (string propertyName in propertyNames)
            {
                if (propertyName == primaryKeyName)
                {
                    continue;
                }

                DbModelPropertyDef propertyDef = modelDef.GetDbPropertyDef(propertyName) ?? throw DbExceptions.PropertyNotFound(modelDef.FullName, propertyName);

                assignments.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},");
            }

            assignments.RemoveLast();

            
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

        //因为UpdatePacks有可能各有各的PropertyNames，所以不能同一生成模板
        //public static string CreateBatchUpdatePropertiesUsingTimestampSql(DbModelDef modelDef, int modelCount )
        //{
            
        //    for(int i = 0; i < modelCount; i++)
        //    {
                
        //    }

            
        //}
    }
}

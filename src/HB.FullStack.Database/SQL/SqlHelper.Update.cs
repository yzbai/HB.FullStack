using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateUpdateIgnoreConflictCheckSql(DbModelDef modelDef, string placeHolder = "0")
        {
            //add Primary Check Where
            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{placeHolder} 
                AND 
                {modelDef.DeletedPropertyDef.DbReservedName}=0
                """;

            return $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, placeHolder)} WHERE {where};";
        }

        public static string CreateUpdateUsingTimestampSql(DbModelDef modelDef, string placeHolder = "0")
        {
            if (!modelDef.IsTimestamp)
            {
                throw DbExceptions.ConflictCheckError($"Update Using Timestamp but not a timestamp model. {modelDef.FullName}");
            }

            string where = $"""
                {modelDef.PrimaryKeyPropertyDef.DbReservedName}={modelDef.PrimaryKeyPropertyDef.DbParameterizedName}_{placeHolder} 
                AND 
                {modelDef.DeletedPropertyDef.DbReservedName}=0 
                AND 
                {modelDef.TimestampPropertyDef!.DbReservedName}={DbParameterName_Timestamp}_{OLD_PROPERTY_VALUE_SUFFIX}_{placeHolder} 
                """;

            return $"UPDATE {modelDef.DbTableReservedName} SET {GetUpdateAssignments(modelDef, placeHolder)} WHERE {where};";
        }

        private static Dictionary<string, string> _updateTemplateCache = new Dictionary<string, string>();

        public static string CreateBatchUpdateUsingTimestampSql(DbModelDef modelDef, int modelCount)
        {
            DbEngineType engineType = modelDef.EngineType;

            string cacheKey = modelDef.FullName + "#Timestamp";
            if (!_updateTemplateCache.TryGetValue(cacheKey, out string? updateTemplate))
            {
                updateTemplate = CreateUpdateUsingTimestampSql(modelDef, "{0}");
                _updateTemplateCache[cacheKey] = updateTemplate;
            }

            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            StringBuilder innerBuilder = new StringBuilder();

            for (int i = 0; i < modelCount; ++i)
            {
                innerBuilder.AppendFormat(updateTemplate, i);
                innerBuilder.Append($" {SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
            }

            return $@"{SqlHelper.Transaction_Begin(engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Create_Id(tempTableName, engineType)}
                                    {innerBuilder}
                                    {SqlHelper.TempTable_Select_Id(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.Transaction_Commit(engineType)}";
        }

        public static string CreateBatchUpdateIgnoreConflictCheckSql(DbModelDef modelDef, int modelCount)
        {
            DbEngineType engineType = modelDef.EngineType;

            string cacheKey = modelDef.FullName + "#Ignore";
            if (!_updateTemplateCache.TryGetValue(cacheKey, out string? updateTemplate))
            {
                updateTemplate = CreateUpdateIgnoreConflictCheckSql(modelDef, "{0}");
                _updateTemplateCache[cacheKey] = updateTemplate;
            }

            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            StringBuilder innerBuilder = new StringBuilder();

            for (int i = 0; i < modelCount; ++i)
            {
                innerBuilder.AppendFormat(updateTemplate, i);
                innerBuilder.Append($" {SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
            }

            return $@"{SqlHelper.Transaction_Begin(engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Create_Id(tempTableName, engineType)}
                                    {innerBuilder}
                                    {SqlHelper.TempTable_Select_Id(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.Transaction_Commit(engineType)}";
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

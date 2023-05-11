﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public static string CreateInsertSql(DbModelDef modelDef, string placeHolder = "0", bool returnId = true)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    continue;
                }

                args.Append($"{propertyDef.DbReservedName},");

                values.Append($"{propertyDef.DbParameterizedName}_{placeHolder},");
            }

            args.RemoveLast();
            values.RemoveLast();

            string returnIdStatement = returnId && modelDef.IdType == DbModelIdType.AutoIncrementLongId ? $"select {GetLastInsertIdStatement(modelDef.EngineType)};" : string.Empty;

            return $"insert into {modelDef.DbTableReservedName}({args}) values({values});{returnIdStatement}";
        }

        private static Dictionary<DbModelDef, string> _insertTemplateCache = new Dictionary<DbModelDef, string>();

        public static string CreateBatchInsertSql(DbModelDef modelDef, int modelCount)
        {
            DbEngineType engineType = modelDef.EngineType;

            if (!_insertTemplateCache.TryGetValue(modelDef, out string? insertTemplate))
            {
                insertTemplate = CreateInsertSql(modelDef, "{0}", false);
                _insertTemplateCache[modelDef] = insertTemplate;
            }

            StringBuilder sqlBuilder = new StringBuilder();

            sqlBuilder.Append(Transaction_Begin(engineType));

            if (modelDef.IdType == DbModelIdType.AutoIncrementLongId)
            {
                string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

                sqlBuilder.Append(TempTable_Drop(tempTableName, engineType));
                sqlBuilder.Append(TempTable_Create_Id(tempTableName, engineType));

                for (int i = 0; i < modelCount; ++i)
                {
                    sqlBuilder.AppendFormat(insertTemplate, i);
                    sqlBuilder.Append($"{TempTable_Insert_Id(tempTableName, GetLastInsertIdStatement(engineType), engineType)}");
                }

                sqlBuilder.Append(TempTable_Select_Id(tempTableName, engineType));

                sqlBuilder.Append(TempTable_Drop(tempTableName, engineType));
            }
            else
            {
                for (int i = 0; i < modelCount; ++i)
                {
                    sqlBuilder.AppendFormat(insertTemplate, i);
                }
            }

            sqlBuilder.Append(Transaction_Commit(engineType));

            return sqlBuilder.ToString();
        }
    }
}

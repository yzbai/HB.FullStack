/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Text;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        /// <summary>
        /// 只用于客户端，没有做Timestamp检查
        /// </summary>
        public static string CreateAddOrUpdateSql(DbModelDef modelDef, bool returnModel, string placeHolder= "0")
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new object[] { returnModel, placeHolder });

            if (SqlCache.TryGetValue(cacheKey, out var sql))
            {
                return sql;
            }

            StringBuilder addArgs = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();
            StringBuilder addValues = new StringBuilder();
            StringBuilder updatePairs = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (returnModel)
                {
                    selectArgs.Append(propertyDef.DbReservedName);
                    selectArgs.Append(',');
                }

                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    continue;
                }

                addArgs.Append(propertyDef.DbReservedName);
                addArgs.Append(',');

                addValues.Append($"{propertyDef.DbParameterizedName}_{placeHolder},");

                if (propertyDef.IsPrimaryKey)
                {
                    continue;
                }

                updatePairs.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{placeHolder},");
            }

            selectArgs.RemoveLast();
            addValues.RemoveLast();
            addArgs.RemoveLast();
            updatePairs.RemoveLast();

            DbModelPropertyDef primaryKeyProperty = modelDef.PrimaryKeyPropertyDef;

            sql = $"insert into {modelDef.DbTableReservedName}({addArgs}) values({addValues}) {OnDuplicateKeyUpdateStatement(modelDef.EngineType, primaryKeyProperty)} {updatePairs};";

            if (returnModel)
            {
                if (modelDef.IdType == DbModelIdType.AutoIncrementLongId)
                {
                    sql += $"select {selectArgs} from {modelDef.DbTableReservedName} where {primaryKeyProperty.DbReservedName} = {LastInsertIdStatement(modelDef.EngineType)};";
                }
                else
                {
                    sql += $"select {selectArgs} from {modelDef.DbTableReservedName} where {primaryKeyProperty.DbReservedName} = {primaryKeyProperty.DbParameterizedName}_{placeHolder};";
                }
            }

            SqlCache[cacheKey] = sql;

            return sql;
        }

        public static string CreateBatchAddOrUpdateSql(DbModelDef modelDef, bool returnModel, int modelCount)
        {
            return CreateBatchSql(
                BatchSqlReturnType.None, 
                modelDef, 
                modelCount, 
                () => CreateAddOrUpdateSql(modelDef, returnModel, "{0}"));
        }
    }
}
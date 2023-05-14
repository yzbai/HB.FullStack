using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Implements;
using HB.FullStack.Database.SQL;

using Microsoft;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder : IDbCommandBuilder
    {
        private readonly IDbModelDefFactory _modelDefFactory;
        private readonly ISQLExpressionVisitor _expressionVisitor;
        private readonly ConcurrentDictionary<string, string> _commandTextCache = new ConcurrentDictionary<string, string>();

        public DbCommandBuilder(IDbModelDefFactory modelDefFactory, ISQLExpressionVisitor expressionVisitor)
        {
            _modelDefFactory = modelDefFactory;
            _expressionVisitor = expressionVisitor;
        }

        private string GetCachedSql(SqlType sqlType, DbModelDef[] modelDefs, IList<string>? propertyNames = null, bool addOrUpdateReturnModel = false)
        {
            string cacheKey = GetCommandTextCacheKey(sqlType, modelDefs, propertyNames, addOrUpdateReturnModel);

            if (!_commandTextCache.TryGetValue(cacheKey, out string? commandText))
            {
                commandText = sqlType switch
                {
                    SqlType.Select => SqlHelper.CreateSelectSql(modelDefs),
                    SqlType.Insert => SqlHelper.CreateInsertSql(modelDefs[0]),

                    SqlType.UpdateIgnoreConflictCheck => SqlHelper.CreateUpdateIgnoreConflictCheckSql(modelDefs[0]),
                    SqlType.UpdateUsingTimestamp => SqlHelper.CreateUpdateUsingTimestampSql(modelDefs[0]),

                    SqlType.Update => SqlHelper.CreateUpdateModelSql(modelDefs[0]),

                    SqlType.UpdatePropertiesIgnoreConflictCheck => SqlHelper.CreateUpdatePropertiesIgnoreConflictCheckSql(modelDefs[0], propertyNames!),
                    SqlType.UpdatePropertiesUsingTimestamp => SqlHelper.CreateUpdatePropertiesUsingTimestampSql(modelDefs[0], propertyNames!),
                    //SqlType.UpdatePropertiesUsingTimestampCompare => SqlHelper.CreateUpdatePropertiesUsingTimestampCompareSql(modelDefs[0], propertyNames!),
                    SqlType.UpdatePropertiesUsingOldNewCompare => SqlHelper.CreateUpdatePropertiesUsingOldNewCompareSql(modelDefs[0], propertyNames!),

                    //SqlType.DeleteModel => SqlHelper.CreateDeleteModelSql(modelDefs[0]),
                    SqlType.UpdateDeletedFields => SqlHelper.CreateUpdateDeletedSql(modelDefs[0]),
                    SqlType.AddOrUpdateModel => SqlHelper.CreateAddOrUpdateSql(modelDefs[0], addOrUpdateReturnModel),


                    SqlType.Delete => SqlHelper.CreateDeleteSql(modelDefs[0]),
                    SqlType.DeleteByProperties => SqlHelper.CreateDeleteByPropertiesSql(modelDefs[0], propertyNames!),

                    _ => throw new NotSupportedException(),
                };

                _commandTextCache.TryAdd(cacheKey, commandText);
            }

            return commandText;

            static string GetCommandTextCacheKey(SqlType textType, DbModelDef[] modelDefs, IEnumerable<string>? propertyNames, bool addOrUpdateReturnModel)
            {
                StringBuilder builder = new StringBuilder(modelDefs[0].DbSchemaName);

                foreach (DbModelDef modelDef in modelDefs)
                {
                    builder.Append(modelDef.TableName);
                    builder.Append('_');
                }

                if (propertyNames != null)
                {
                    foreach (string propertyName in propertyNames)
                    {
                        builder.Append(propertyName);
                        builder.Append('_');
                    }
                }

                builder.Append(textType.ToString());
                builder.Append(addOrUpdateReturnModel);

                return builder.ToString();
            }
        }

        #region 条件构造

        public FromExpression<T> From<T>() where T : BaseDbModel, new()
        {
            return new FromExpression<T>(_modelDefFactory, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>() where T : BaseDbModel, new()
        {
            DbModelDef modelDef = _modelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : BaseDbModel, new()
        {
            DbModelDef modelDef = _modelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, _expressionVisitor).Where(sqlFilter, filterParams);
        }

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : BaseDbModel, new()
        {
            DbModelDef modelDef = _modelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, _expressionVisitor).Where(predicate);
        }

        #endregion

        

        #region 更改 - AddOrUpdate

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update. 且Version不变,不增长
        /// </summary>
        public DbEngineCommand CreateAddOrUpdateCommand<T>(DbModelDef modelDef, T model, bool returnModel) where T : DbModel, new()
        {
            //只在客户端开放，因为不检查Version就update. 且Version不变,不增长
            return new DbEngineCommand(
                GetCachedSql(SqlType.AddOrUpdateModel, new DbModelDef[] { modelDef }, null, returnModel),
                model.ToDbParameters(modelDef, _modelDefFactory));
        }

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update，并且无法更新models
        /// </summary>
        public DbEngineCommand CreateBatchAddOrUpdateCommand<T>(DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            DbEngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            //string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T model in models)
            {
                string addOrUpdateCommandText = SqlHelper.CreateAddOrUpdateSql(modelDef, false, number);

                parameters.AddRange(model.ToDbParameters(modelDef, _modelDefFactory, number));

                innerBuilder.Append(addOrUpdateCommandText);

                number++;
            }

            StringBuilder commandTextBuilder = new StringBuilder();

            if (needTrans)
            {
                commandTextBuilder.Append(SqlHelper.Transaction_Begin(engineType));
            }

            //commandTextBuilder.Append($"{SqlHelper.TempTable_Drop(tempTableName, engineType)}");
            //commandTextBuilder.Append($"{SqlHelper.TempTable_Create_Id(tempTableName, engineType)}");
            commandTextBuilder.Append(innerBuilder);
            //commandTextBuilder.Append($"{SqlHelper.TempTable_Drop(tempTableName, engineType)}");

            if (needTrans)
            {
                commandTextBuilder.Append(SqlHelper.Transaction_Commit(engineType));
            }

            return new DbEngineCommand(commandTextBuilder.ToString(), parameters);
        }

        #endregion


        #region Management

        public DbEngineCommand CreateTableCreateCommand(DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength, int maxVarcharFieldLength, int maxMediumTextFieldLength)
        {
            string sql = SqlHelper.GetTableCreateSql(modelDef, addDropStatement, varcharDefaultLength, maxVarcharFieldLength, maxMediumTextFieldLength);

            return new DbEngineCommand(sql);
        }

        public DbEngineCommand CreateIsTableExistCommand(DbEngineType engineType, string tableName)
        {
            string sql = SqlHelper.GetIsTableExistSql(engineType);

            var parameters = new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>("@tableName", tableName )
            };

            return new DbEngineCommand(sql, parameters);
        }

        public DbEngineCommand CreateSystemInfoRetrieveCommand(DbEngineType engineType)
        {
            string sql = SqlHelper.GetSystemInfoRetrieveSql(engineType);

            return new DbEngineCommand(sql);
        }

        public DbEngineCommand CreateSystemVersionSetCommand(DbEngineType engineType, string dbSchemaName, int version)
        {
            string sql;
            List<KeyValuePair<string, object>> parameters;

            if (version == 1)
            {
                sql = SqlHelper.GetSystemInfoCreateSql(engineType);

                parameters = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>($"@{SystemInfoNames.DATABASE_SCHEMA}", dbSchemaName) };
            }
            else
            {
                sql = SqlHelper.GetSystemInfoUpdateVersionSql(engineType);

                parameters = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>($"@{SystemInfoNames.VERSION}", version) };
            }

            return new DbEngineCommand(sql, parameters);
        }

        #endregion Management
    }
}
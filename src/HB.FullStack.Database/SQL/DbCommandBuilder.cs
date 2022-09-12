using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.SQL;

using Microsoft;

#pragma warning disable CA1822 // Mark members as static
namespace HB.FullStack.Database
{
    internal class DbCommandBuilder : IDbCommandBuilder
    {
        private readonly IDbModelDefFactory _modelDefFactory;
        private readonly ISQLExpressionVisitor _expressionVisitor;
        private readonly ConcurrentDictionary<string, string> _commandTextCache = new ConcurrentDictionary<string, string>();

        public DbCommandBuilder(IDbModelDefFactory modelDefFactory, ISQLExpressionVisitor expressionVisitor)
        {
            _modelDefFactory = modelDefFactory;
            _expressionVisitor = expressionVisitor;
        }

        private string GetCachedSql(EngineType engineType, SqlType commandTextType, DbModelDef[] modelDefs, IEnumerable<string>? propertyNames = null, bool addOrUpdateReturnModel = false)
        {
            string cacheKey = GetCommandTextCacheKey(commandTextType, modelDefs, propertyNames);

            if (!_commandTextCache.TryGetValue(cacheKey, out string? commandText))
            {
                commandText = commandTextType switch
                {
                    SqlType.AddModel => SqlHelper.CreateAddModelSql(modelDefs[0], engineType, true),
                    SqlType.UpdateModel => SqlHelper.CreateUpdateModelSql(modelDefs[0]),
                    SqlType.UpdateFieldsUsingTimestampCompare => SqlHelper.CreateUpdateFieldsUsingTimestampCompareSql(modelDefs[0], propertyNames!),
                    SqlType.UpdateFieldsUsingOldNewCompare => SqlHelper.CreateUpdateFieldsUsingOldNewCompareSql(modelDefs[0], engineType, propertyNames!),
                    SqlType.DeleteModel => SqlHelper.CreateDeleteModelSql(modelDefs[0]),
                    SqlType.SelectModel => SqlHelper.CreateSelectModelSql(modelDefs),
                    SqlType.Delete => SqlHelper.CreateDeleteSql(modelDefs[0]),
                    SqlType.AddOrUpdateModel => SqlHelper.CreateAddOrUpdateSql(modelDefs[0], engineType, addOrUpdateReturnModel),
                    _ => throw new NotSupportedException(),
                };

                _commandTextCache.TryAdd(cacheKey, commandText);
            }

            return commandText;

            static string GetCommandTextCacheKey(SqlType textType, DbModelDef[] modelDefs, IEnumerable<string>? propertyNames)
            {
                StringBuilder builder = new StringBuilder(modelDefs[0].DatabaseName);

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

                return builder.ToString();
            }
        }

        #region 条件构造

        public FromExpression<T> From<T>(EngineType engineType) where T : DbModel, new()
        {
            return new FromExpression<T>(engineType, _modelDefFactory, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>(EngineType engineType) where T : DbModel, new()
        {
            return new WhereExpression<T>(engineType, _modelDefFactory, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>(EngineType engineType, string sqlFilter, params object[] filterParams) where T : DbModel, new()
        {
            return new WhereExpression<T>(engineType, _modelDefFactory, _expressionVisitor).Where(sqlFilter, filterParams);
        }

        public WhereExpression<T> Where<T>(EngineType engineType, Expression<Func<T, bool>> predicate) where T : DbModel, new()
        {
            return new WhereExpression<T>(engineType, _modelDefFactory, _expressionVisitor).Where(predicate);
        }

        #endregion

        #region 查询

        public EngineCommand CreateRetrieveCommand<T>(EngineType engineType, DbModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DbModel, new()
        {
            return AssembleRetrieveCommand(GetCachedSql(engineType, SqlType.SelectModel, new DbModelDef[] { modelDef }), fromCondition, whereCondition, engineType);
        }

        public EngineCommand CreateCountCommand<T>(EngineType engineType, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DbModel, new()
        {
            return AssembleRetrieveCommand("SELECT COUNT(1) ", fromCondition, whereCondition, engineType);
        }

        public EngineCommand CreateRetrieveCommand<T1, T2>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(engineType, SqlType.SelectModel, returnModelDefs),
                fromCondition,
                whereCondition,
                engineType);
        }

        public EngineCommand CreateRetrieveCommand<T1, T2, T3>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new()
            where T3 : DbModel, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(engineType, SqlType.SelectModel, returnModelDefs),
                fromCondition,
                whereCondition,
                engineType);
        }

        public EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType engineType, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DbModelDef[] returnModelDefs)
            where TSelect : DbModel, new()
            where TFrom : DbModel, new()
            where TWhere : DbModel, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(engineType, SqlType.SelectModel, returnModelDefs),
                fromCondition,
                whereCondition,
                engineType);
        }

        private EngineCommand AssembleRetrieveCommand<TFrom, TWhere>(string selectText, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, EngineType engineType)
            where TFrom : DbModel, new()
            where TWhere : DbModel, new()
        {
            string sql = selectText;
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            if (fromCondition == null)
            {
                fromCondition = new FromExpression<TFrom>(engineType, _modelDefFactory, _expressionVisitor);
            }

            sql += fromCondition.ToStatement();
            parameters.AddRange(fromCondition.GetParameters());

            if (whereCondition != null)
            {
                sql += whereCondition.ToStatement(engineType);

                parameters.AddRange(whereCondition.GetParameters());
            }

            return new EngineCommand(sql, parameters);
        }

        #endregion 查询

        #region 更改

        public EngineCommand CreateAddCommand<T>(EngineType engineType, DbModelDef modelDef, T model) where T : DbModel, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.AddModel, new DbModelDef[] { modelDef }),
                model.ToDbParameters(modelDef, engineType, _modelDefFactory));
        }

        public EngineCommand CreateUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, T model, long oldTimestamp) where T : DbModel, new()
        {
            var paramters = model.ToDbParameters(modelDef, engineType, _modelDefFactory);

            if (modelDef.IsTimestampDBModel)
            {
                DbModelPropertyDef timestampProperty = modelDef.GetPropertyDef(nameof(TimestampDbModel.Timestamp))!;
                paramters.Add(new KeyValuePair<string, object>($"{timestampProperty.DbParameterizedName}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0", oldTimestamp));
            }

            return new EngineCommand(
                GetCachedSql(engineType, SqlType.UpdateModel, new DbModelDef[] { modelDef }),
                paramters);
        }

        /// <summary>
        /// 针对ServerDatabaseModel
        /// </summary>
        public EngineCommand CreateUpdateFieldsUsingTimestampCompareCommand(EngineType engineType, DbModelDef modelDef, object id, long oldTimestamp, long newTimestamp, string lastUser,
            IList<string> propertyNames, IList<object?> propertyValues)
        {
            propertyNames.Add(nameof(TimestampLongIdDbModel.Id));
            propertyValues.Add(id);

            if (modelDef.IsTimestampDBModel)
            {
                propertyNames.Add(nameof(TimestampDbModel.Timestamp));
                propertyValues.Add(newTimestamp);

                propertyNames.Add(nameof(TimestampDbModel.LastUser));
                propertyValues.Add(lastUser);
            }

            IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(modelDef, engineType, _modelDefFactory, propertyNames, propertyValues);

            if (modelDef.IsTimestampDBModel)
            {
                DbModelPropertyDef timestampProperty = modelDef.GetPropertyDef(nameof(TimestampDbModel.Timestamp))!;
                parameters.Add(new KeyValuePair<string, object>($"{timestampProperty.DbParameterizedName}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0", oldTimestamp));
            }

            return new EngineCommand(
                GetCachedSql(engineType, SqlType.UpdateFieldsUsingTimestampCompare, new DbModelDef[] { modelDef }, propertyNames),
                parameters);
        }

        public EngineCommand CreateUpdateFieldsUsingOldNewCompareCommand(EngineType engineType, DbModelDef modelDef,
            object id, long newTimestamp, string lastUser, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues)
        {
            string sql = GetCachedSql(engineType, SqlType.UpdateFieldsUsingOldNewCompare, new DbModelDef[] { modelDef }, propertyNames);

            var oldParameters = DbModelConvert.PropertyValuesToParameters(modelDef, engineType, _modelDefFactory, propertyNames, oldPropertyValues, $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0");

            propertyNames.Add(nameof(TimestampLongIdDbModel.Id));
            newPropertyValues.Add(id);

            if (modelDef.IsTimestampDBModel)
            {
                propertyNames.Add(nameof(TimestampDbModel.Timestamp));
                newPropertyValues.Add(newTimestamp);

                propertyNames.Add(nameof(TimestampDbModel.LastUser));
                newPropertyValues.Add(lastUser);
            }

            var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, engineType, _modelDefFactory, propertyNames, newPropertyValues, $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_0");

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(oldParameters);
            parameters.AddRange(newParameters);

            return new EngineCommand(sql, parameters);
        }

        public EngineCommand CreateDeleteCommand<T>(EngineType engineType, DbModelDef modelDef, T model, long oldTimestamp) where T : DbModel, new()
            => CreateUpdateCommand(engineType, modelDef, model, oldTimestamp);

        /// <summary>
        /// 针对Client
        /// </summary>
        public EngineCommand CreateDeleteCommand<T>(EngineType engineType, DbModelDef modelDef, WhereExpression<T> whereExpression) where T : TimelessDbModel, new()
        {
            Requires.NotNull(whereExpression, nameof(whereExpression));

            string sql = GetCachedSql(engineType, SqlType.Delete, new DbModelDef[] { modelDef }) + whereExpression.ToStatement(engineType);

            return new EngineCommand(sql, whereExpression.GetParameters());
        }

        public EngineCommand CreateBatchAddCommand<T>(EngineType engineType, DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new()
        {
            //TODO: 在不需要返回Id的DatabaseModel中，使用如下句式：
            //insert into user_info （user_id,user_name,status,years）values （123,‘你好’,1,15）,(456,“你好”,2,16)；

            ThrowIf.Empty(models, nameof(models));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            bool isIdAutoIncrement = modelDef.IsIdAutoIncrement;

            foreach (T model in models)
            {
                string addCommandText = SqlHelper.CreateAddModelSql(modelDef, engineType, false, number);

                parameters.AddRange(model.ToDbParameters(modelDef, engineType, _modelDefFactory, number));

                innerBuilder.Append(addCommandText);

                if (isIdAutoIncrement)
                {
#if NET6_0_OR_GREATER
                    innerBuilder.Append(CultureInfo.InvariantCulture, $"{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.GetLastInsertIdStatement(engineType), engineType)}");
#elif NETSTANDARD2_1
                    innerBuilder.Append($"{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.GetLastInsertIdStatement(engineType), engineType)}");
#endif
                }
                number++;
            }

            StringBuilder commandTextBuilder = new StringBuilder();

            if (needTrans)
            {
                commandTextBuilder.Append(SqlHelper.Transaction_Begin(engineType));
            }

            commandTextBuilder.Append(SqlHelper.TempTable_Drop(tempTableName, engineType));
            commandTextBuilder.Append(SqlHelper.TempTable_Create_Id(tempTableName, engineType));
            commandTextBuilder.Append(innerBuilder);

            if (isIdAutoIncrement)
            {
                commandTextBuilder.Append(SqlHelper.TempTable_Select_Id(tempTableName, engineType));
            }

            commandTextBuilder.Append(SqlHelper.TempTable_Drop(tempTableName, engineType));

            if (needTrans)
            {
                commandTextBuilder.Append(SqlHelper.Transaction_Commit(engineType));
            }

            return new EngineCommand(commandTextBuilder.ToString(), parameters);
        }

        public EngineCommand CreateBatchUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : DbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            if (modelDef.IsTimestampDBModel)
            {
                ThrowIf.NotEqual(models.Count(), oldTimestamps.Count, nameof(models), nameof(oldTimestamps));
            }

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            DbModelPropertyDef? timestampProperty = modelDef.IsTimestampDBModel ? modelDef.GetPropertyDef(nameof(TimestampDbModel.Timestamp))! : null;

            foreach (T model in models)
            {
                string updateCommandText = SqlHelper.CreateUpdateModelSql(modelDef, number);

                parameters.AddRange(model.ToDbParameters(modelDef, engineType, _modelDefFactory, number));

                //这里要添加 一些参数值，参考update
                if (modelDef.IsTimestampDBModel)
                {
                    parameters.Add(new KeyValuePair<string, object>($"{timestampProperty!.DbParameterizedName}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}", oldTimestamps[number]));
                }

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundMatchedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundMatchedRows_Statement(engineType), engineType)}");
#endif

                number++;
            }

            string may_trans_begin = needTrans ? SqlHelper.Transaction_Begin(engineType) : "";
            string may_trans_commit = needTrans ? SqlHelper.Transaction_Commit(engineType) : "";

            string commandText = $@"{may_trans_begin}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Create_Id(tempTableName, engineType)}
                                    {innerBuilder}
                                    {SqlHelper.TempTable_Select_Id(tempTableName, engineType)}
                                    {SqlHelper.TempTable_Drop(tempTableName, engineType)}
                                    {may_trans_commit}";

            return new EngineCommand(commandText, parameters);
        }

        public EngineCommand CreateBatchDeleteCommand<T>(EngineType engineType, DbModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : DbModel, new()
            => CreateBatchUpdateCommand<T>(engineType, modelDef, models, oldTimestamps, needTrans);

        #endregion

        #region Management

        public EngineCommand CreateTableCreateCommand(EngineType engineType, DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength)
        {
            string sql = SqlHelper.GetTableCreateSql(modelDef, addDropStatement, varcharDefaultLength, engineType);

            return new EngineCommand(sql);
        }

        public EngineCommand CreateIsTableExistCommand(EngineType engineType, string databaseName, string tableName)
        {
            string sql = SqlHelper.GetIsTableExistSql(engineType);

            var parameters = new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>("@tableName", tableName ),
                new KeyValuePair<string, object>( "@databaseName", databaseName)
            };

            return new EngineCommand(sql, parameters);
        }

        public EngineCommand CreateSystemInfoRetrieveCommand(EngineType engineType)
        {
            string sql = SqlHelper.GetSystemInfoRetrieveSql(engineType);

            return new EngineCommand(sql);
        }

        public EngineCommand CreateSystemVersionUpdateCommand(EngineType engineType, string databaseName, int version)
        {
            string sql;
            List<KeyValuePair<string, object>> parameters;

            if (version == 1)
            {
                sql = SqlHelper.GetSystemInfoCreateSql(engineType);

                parameters = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("@databaseName", databaseName) };
            }
            else
            {
                sql = SqlHelper.GetSystemInfoUpdateVersionSql(engineType);

                parameters = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("@Value", version) };
            }

            return new EngineCommand(sql, parameters);
        }

        #endregion Management

        #region AddOrUpdate

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update. 且Version不变,不增长
        /// </summary>
        public EngineCommand CreateAddOrUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, T model, bool returnModel) where T : DbModel, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.AddOrUpdateModel, new DbModelDef[] { modelDef }, null, returnModel),
                model.ToDbParameters(modelDef, engineType, _modelDefFactory));
        }

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update，并且无法更新models
        /// </summary>
        public EngineCommand CreateBatchAddOrUpdateCommand<T>(EngineType engineType, DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            StringBuilder innerBuilder = new StringBuilder();
            //string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T model in models)
            {
                string addOrUpdateCommandText = SqlHelper.CreateAddOrUpdateSql(modelDef, engineType, false, number);

                parameters.AddRange(model.ToDbParameters(modelDef, engineType, _modelDefFactory, number));

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

            return new EngineCommand(commandTextBuilder.ToString(), parameters);
        }

        #endregion
    }
}
#pragma warning restore CA1822 // Mark members as static
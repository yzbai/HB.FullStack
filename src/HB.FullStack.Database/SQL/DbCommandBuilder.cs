using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

using Microsoft;

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

        private string GetCachedSql(SqlType commandTextType, DbModelDef[] modelDefs, IEnumerable<string>? propertyNames = null, bool addOrUpdateReturnModel = false)
        {
            string cacheKey = GetCommandTextCacheKey(commandTextType, modelDefs, propertyNames, addOrUpdateReturnModel);

            if (!_commandTextCache.TryGetValue(cacheKey, out string? commandText))
            {
                commandText = commandTextType switch
                {
                    SqlType.AddModel => SqlHelper.CreateAddModelSql(modelDefs[0], true),

                    SqlType.UpdateModel => SqlHelper.CreateUpdateModelSql(modelDefs[0]),
                    SqlType.UpdateProperties => SqlHelper.CreateUpdatePropertiesSql(modelDefs[0], propertyNames!),
                    //SqlType.UpdatePropertiesUsingTimestampCompare => SqlHelper.CreateUpdatePropertiesUsingTimestampCompareSql(modelDefs[0], propertyNames!),
                    SqlType.UpdatePropertiesUsingOldNewCompare => SqlHelper.CreateUpdatePropertiesUsingOldNewCompareSql(modelDefs[0], propertyNames!),

                    //SqlType.DeleteModel => SqlHelper.CreateDeleteModelSql(modelDefs[0]),
                    SqlType.UpdateDeletedFields => SqlHelper.CreateUpdateDeletedSql(modelDefs[0]),
                    SqlType.AddOrUpdateModel => SqlHelper.CreateAddOrUpdateSql(modelDefs[0], addOrUpdateReturnModel),

                    SqlType.SelectModel => SqlHelper.CreateSelectModelSql(modelDefs),

                    SqlType.Delete => SqlHelper.CreateDeleteSql(modelDefs[0]),
                    SqlType.DeleteByProperties => SqlHelper.CreateDeleteByPropertiesSql(modelDefs[0], propertyNames!),

                    _ => throw new NotSupportedException(),
                };

                _commandTextCache.TryAdd(cacheKey, commandText);
            }

            return commandText;

            static string GetCommandTextCacheKey(SqlType textType, DbModelDef[] modelDefs, IEnumerable<string>? propertyNames, bool addOrUpdateReturnModel)
            {
                StringBuilder builder = new StringBuilder(modelDefs[0].DbName ?? modelDefs[0].DbKind);

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

        public FromExpression<T> From<T>() where T : DbModel, new()
        {
            return new FromExpression<T>(_modelDefFactory, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>() where T : DbModel, new()
        {
            DbModelDef modelDef = _modelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DbModel, new()
        {
            DbModelDef modelDef = _modelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, _expressionVisitor).Where(sqlFilter, filterParams);
        }

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DbModel, new()
        {
            DbModelDef modelDef = _modelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, _expressionVisitor).Where(predicate);
        }

        #endregion

        #region 查询

        public EngineCommand CreateRetrieveCommand<T>(DbModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DbModel, new()
        {
            return AssembleRetrieveCommand(GetCachedSql(SqlType.SelectModel, new DbModelDef[] { modelDef }), fromCondition, whereCondition);
        }

        public EngineCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DbModel, new()
        {
            return AssembleRetrieveCommand("SELECT COUNT(1) ", fromCondition, whereCondition);
        }

        public EngineCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(SqlType.SelectModel, returnModelDefs),
                fromCondition,
                whereCondition);
        }

        public EngineCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : DbModel, new()
            where T2 : DbModel, new()
            where T3 : DbModel, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(SqlType.SelectModel, returnModelDefs),
                fromCondition,
                whereCondition);
        }

        public EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DbModelDef[] returnModelDefs)
            where TSelect : DbModel, new()
            where TFrom : DbModel, new()
            where TWhere : DbModel, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(SqlType.SelectModel, returnModelDefs),
                fromCondition,
                whereCondition);
        }

        private EngineCommand AssembleRetrieveCommand<TFrom, TWhere>(string selectText, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition)
            where TFrom : DbModel, new()
            where TWhere : DbModel, new()
        {
            string sql = selectText;
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            fromCondition ??= new FromExpression<TFrom>(_modelDefFactory, _expressionVisitor);

            sql += fromCondition.ToStatement();
            parameters.AddRange(fromCondition.GetParameters());

            if (whereCondition != null)
            {
                sql += whereCondition.ToStatement();

                parameters.AddRange(whereCondition.GetParameters());
            }

            return new EngineCommand(sql, parameters);
        }

        #endregion

        #region 更改 - Add

        public EngineCommand CreateAddCommand<T>(DbModelDef modelDef, T model) where T : DbModel, new()
        {
            return new EngineCommand(
                GetCachedSql(SqlType.AddModel, new DbModelDef[] { modelDef }),
                model.ToDbParameters(modelDef, _modelDefFactory));
        }

        public EngineCommand CreateBatchAddCommand<T>(DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new()
        {
            //TODO: 在不需要返回Id的DatabaseModel中，使用如下句式：
            //insert into user_info （user_id,user_name,status,years）values （123,‘你好’,1,15）,(456,“你好”,2,16)；

            ThrowIf.Empty(models, nameof(models));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            bool isIdAutoIncrement = modelDef.IsIdAutoIncrement;

            EngineType engineType = modelDef.EngineType;

            foreach (T model in models)
            {
                string addCommandText = SqlHelper.CreateAddModelSql(modelDef, false, number);

                parameters.AddRange(model.ToDbParameters(modelDef, _modelDefFactory, number));

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

        #endregion

        #region 更改 - Update

        public EngineCommand CreateUpdateCommand<T>(DbModelDef modelDef, T model, long oldTimestamp) where T : DbModel, new()
        {
            var paramters = model.ToDbParameters(modelDef, _modelDefFactory);

            if (modelDef.IsTimestampDBModel)
            {
                DbModelPropertyDef timestampProperty = modelDef.GetDbPropertyDef(nameof(TimestampDbModel.Timestamp))!;
                paramters.Add(new KeyValuePair<string, object>($"{timestampProperty.DbParameterizedName}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0", oldTimestamp));
            }

            return new EngineCommand(
                GetCachedSql(SqlType.UpdateModel, new DbModelDef[] { modelDef }),
                paramters);
        }

        public EngineCommand CreateBatchUpdateCommand<T>(DbModelDef modelDef, IEnumerable<T> models, IList<long> oldTimestamps, bool needTrans) where T : DbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            if (modelDef.IsTimestampDBModel)
            {
                ThrowIf.NotEqual(models.Count(), oldTimestamps.Count, nameof(models), nameof(oldTimestamps));
            }

            EngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            DbModelPropertyDef? timestampProperty = modelDef.IsTimestampDBModel ? modelDef.GetDbPropertyDef(nameof(TimestampDbModel.Timestamp))! : null;

            foreach (T model in models)
            {
                string updateCommandText = SqlHelper.CreateUpdateModelSql(modelDef, number);

                parameters.AddRange(model.ToDbParameters(modelDef, _modelDefFactory, number));

                //这里要添加 一些参数值，参考update
                if (modelDef.IsTimestampDBModel)
                {
                    parameters.Add(new KeyValuePair<string, object>($"{timestampProperty!.DbParameterizedName}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}", oldTimestamps[number]));
                }

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
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

        #endregion

        #region 更改 - UpdateProperties

        public EngineCommand CreateUpdatePropertiesCommand(
            DbModelDef modelDef,
            object id,
            IList<string> propertyNames,
            IList<object?> propertyValues,
            long? oldTimestamp,
            long? newTimestamp,
            string lastUser)
        {
            EngineType engineType = modelDef.EngineType;

            IList<string> curPropertyNames = new List<string>(propertyNames);
            IList<object?> curPropertyValues = new List<object?>(propertyValues);

            curPropertyNames.Add(nameof(TimestampLongIdDbModel.Id));
            curPropertyValues.Add(id);

            curPropertyNames.Add(nameof(DbModel.LastUser));
            curPropertyValues.Add(lastUser);

            if (modelDef.IsTimestampDBModel && !(oldTimestamp.HasValue && newTimestamp.HasValue))
            {
                throw DatabaseExceptions.TimestampNotExists(engineType: engineType, modelDef: modelDef, propertyNames: curPropertyNames);
            }

            if (newTimestamp.HasValue)
            {
                curPropertyNames.Add(nameof(TimestampDbModel.Timestamp));
                curPropertyValues.Add(newTimestamp.Value);
            }

            IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, curPropertyNames, curPropertyValues);

            if (oldTimestamp.HasValue)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0",
                    oldTimestamp.Value));
            }

            return new EngineCommand(
                GetCachedSql(SqlType.UpdateProperties, new DbModelDef[] { modelDef }, curPropertyNames),
                parameters);
        }

        public EngineCommand CreateBatchUpdatePropertiesCommand(
            DbModelDef modelDef,
            IList<(object id, IList<string> propertyNames, IList<object?> propertyValues, long? oldTimestamp, long? newTimestamp)> modelChanges,
            string lastUser,
            bool needTrans)
        {
            ThrowIf.Empty(modelChanges, nameof(modelChanges));

            EngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            DbModelPropertyDef? timestampProperty = modelDef.IsTimestampDBModel ? modelDef.GetDbPropertyDef(nameof(TimestampDbModel.Timestamp))! : null;

            foreach (var (id, propertyNames, propertyValues, oldTimestamp, newTimestamp) in modelChanges)
            {
                #region Parameters

                IList<string> curPropertyNames = new List<string>(propertyNames);
                IList<object?> curPropertyValues = new List<object?>(propertyValues);

                curPropertyNames.Add(nameof(TimestampLongIdDbModel.Id));
                curPropertyValues.Add(id);
                curPropertyNames.Add(nameof(DbModel.LastUser));
                curPropertyValues.Add(lastUser);

                if (modelDef.IsTimestampDBModel && !(oldTimestamp.HasValue && newTimestamp.HasValue))
                {
                    throw DatabaseExceptions.TimestampNotExists(engineType, modelDef, curPropertyNames);
                }

                if (newTimestamp.HasValue)
                {
                    curPropertyNames.Add(nameof(TimestampDbModel.Timestamp));
                    curPropertyValues.Add(newTimestamp.Value);
                }

                IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    curPropertyNames,
                    curPropertyValues,
                    number.ToString());

                if (oldTimestamp.HasValue)
                {
                    parameters.Add(new KeyValuePair<string, object>(
                        $"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}",
                        oldTimestamp.Value));
                }

                totalParameters.AddRange(parameters);

                #endregion

                string updatePropertiesSql = SqlHelper.CreateUpdatePropertiesSql(modelDef, curPropertyNames, number);

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{updatePropertiesSql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{updatePropertiesSql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
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

            return new EngineCommand(commandText, totalParameters);
        }

        public EngineCommand CreateUpdatePropertiesUsingOldNewCompareCommand(
            DbModelDef modelDef,
            object id,
            IList<string> propertyNames,
            IList<object?> oldPropertyValues,
            IList<object?> newPropertyValues,
            long newTimestamp,
            string lastUser)
        {
            List<string> curPropertyNames = new List<string>(propertyNames);
            List<object?> curNewPropertyValues = new List<object?>(newPropertyValues);

            var oldParameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, curPropertyNames, oldPropertyValues, $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0");

            curPropertyNames.Add(nameof(TimestampLongIdDbModel.Id));
            curNewPropertyValues.Add(id);

            curPropertyNames.Add(nameof(TimestampDbModel.LastUser));
            curNewPropertyValues.Add(lastUser);

            if (modelDef.IsTimestampDBModel)
            {
                curPropertyNames.Add(nameof(TimestampDbModel.Timestamp));
                curNewPropertyValues.Add(newTimestamp);
            }

            var newParameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, curPropertyNames, curNewPropertyValues, $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_0");

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(oldParameters);
            parameters.AddRange(newParameters);

            //使用propertyNames而不是curPropertyNames
            string sql = GetCachedSql(SqlType.UpdatePropertiesUsingOldNewCompare, new DbModelDef[] { modelDef }, propertyNames);

            return new EngineCommand(sql, parameters);
        }

        public EngineCommand CreateBatchUpdatePropertiesUsingOldNewCompareCommand(
            DbModelDef modelDef,
            IList<(object id, IList<string> propertyNames, IList<object?> oldPropertyValues, IList<object?> newPropertyValues, long newTimestamp)> modelChanges,
            string lastUser,
            bool needTrans)
        {
            ThrowIf.Empty(modelChanges, nameof(modelChanges));

            EngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (var (id, propertyNames, oldPropertyValues, newPropertyValues, newTimestamp) in modelChanges)
            {
                List<string> curPropertyNames = new List<string>(propertyNames);
                List<object?> curNewPropertyValues = new List<object?>(newPropertyValues);

                #region Parameters

                var oldParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    curPropertyNames,
                    oldPropertyValues,
                    $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}");

                totalParameters.AddRange(oldParameters);

                curPropertyNames.Add(nameof(TimestampLongIdDbModel.Id));
                curNewPropertyValues.Add(id);
                curPropertyNames.Add(nameof(DbModel.LastUser));
                curNewPropertyValues.Add(lastUser);

                if (modelDef.IsTimestampDBModel)
                {
                    curPropertyNames.Add(nameof(TimestampDbModel.Timestamp));
                    curNewPropertyValues.Add(newTimestamp);
                }

                var newParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                      _modelDefFactory,
                    curPropertyNames,
                    curNewPropertyValues,
                    $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_{number}");

                totalParameters.AddRange(newParameters);

                #endregion

                string sql = SqlHelper.CreateUpdatePropertiesUsingOldNewCompareSql(modelDef, propertyNames, number);

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundUpdateMatchedRows_Statement(engineType), engineType)}");
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

            return new EngineCommand(commandText, totalParameters);
        }

        #endregion

        #region 更改 - AddOrUpdate

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update. 且Version不变,不增长
        /// </summary>
        public EngineCommand CreateAddOrUpdateCommand<T>(DbModelDef modelDef, T model, bool returnModel) where T : DbModel, new()
        {
            //只在客户端开放，因为不检查Version就update. 且Version不变,不增长
            return new EngineCommand(
                GetCachedSql(SqlType.AddOrUpdateModel, new DbModelDef[] { modelDef }, null, returnModel),
                model.ToDbParameters(modelDef, _modelDefFactory));
        }

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update，并且无法更新models
        /// </summary>
        public EngineCommand CreateBatchAddOrUpdateCommand<T>(DbModelDef modelDef, IEnumerable<T> models, bool needTrans) where T : DbModel, new()
        {
            ThrowIf.Empty(models, nameof(models));

            EngineType engineType = modelDef.EngineType;

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

            return new EngineCommand(commandTextBuilder.ToString(), parameters);
        }

        #endregion

        #region 更改 - Delete

        public EngineCommand CreateDeleteCommand(
            DbModelDef modelDef,
            object id,
            string lastUser,
            bool trulyDeleted,
            long? oldTimestamp,
            long? newTimestamp)
        {
            EngineType engineType = modelDef.EngineType;

            if (!trulyDeleted)
            {
                return CreateUpdatePropertiesCommand(
                    modelDef,
                    id,
                    new List<string> { nameof(DbModel.Deleted) },
                    new List<object?> { true },
                    oldTimestamp,
                    newTimestamp,
                    lastUser);
            }

            List<string> propertyNames = new List<string> { nameof(TimestampLongIdDbModel.Id) };
            List<object?> propertyValues = new List<object?> { id };

            if (modelDef.IsTimestampDBModel && !oldTimestamp.HasValue)
            {
                throw DatabaseExceptions.TimestampNotExists(engineType, modelDef, propertyNames);
            }

            if (oldTimestamp.HasValue)
            {
                propertyNames.Add(nameof(TimestampLongIdDbModel.Timestamp));
                propertyValues.Add(oldTimestamp.Value);
            }

            string sql = GetCachedSql(SqlType.DeleteByProperties, new DbModelDef[] { modelDef }, propertyNames);

            IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, propertyNames, propertyValues);

            return new EngineCommand(sql, parameters);
        }

        public EngineCommand CreateDeleteCommand<T>(
            DbModelDef modelDef,
            WhereExpression<T> whereExpression,
            string lastUser,
            bool trulyDeleted) where T : TimelessDbModel, new()
        {
            Requires.NotNull(whereExpression, nameof(whereExpression));

            IList<KeyValuePair<string, object>> parameters = whereExpression.GetParameters();

            if (!trulyDeleted)
            {
                parameters.Add(new KeyValuePair<string, object>(
                    $"{SqlHelper.DbParameterName_LastUser}_0",
                    lastUser));
                parameters.Add(new KeyValuePair<string, object>(
                    $"{SqlHelper.DbParameterName_Deleted}_0",
                    true));

                string sql = GetCachedSql(SqlType.UpdateDeletedFields, new DbModelDef[] { modelDef }) + whereExpression.ToStatement();

                return new EngineCommand(sql, parameters);
            }

            string deleteSql = GetCachedSql(SqlType.Delete, new DbModelDef[] { modelDef }) + whereExpression.ToStatement();

            return new EngineCommand(deleteSql, parameters);
        }

        public EngineCommand CreateBatchDeleteCommand(
            DbModelDef modelDef,
            IList<object> ids,
            IList<long?> oldTimestamps,
            IList<long?> newTimestamps,
            string lastUser,
            bool trulyDeleted,
            bool needTrans)
        {
            int count = ids.Count;

            EngineType engineType = modelDef.EngineType;

            if (!trulyDeleted)
            {
                var propertyNames = new List<string> { nameof(DbModel.Deleted) };
                var propertyValues = new List<object?> { true };
                var modelChanges = new List<(object id, IList<string> propertyNames, IList<object?> propertyValues, long? oldTimestamp, long? newTimestamp)>();

                for (int i = 0; i < count; ++i)
                {
                    modelChanges.Add((ids[i], propertyNames, propertyValues, oldTimestamps[i], newTimestamps[i]));
                }

                return CreateBatchUpdatePropertiesCommand(modelDef, modelChanges, lastUser, needTrans);
            }

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            for (int i = 0; i < count; ++i)
            {
                List<string> propertyNames = new List<string> { nameof(TimestampLongIdDbModel.Id) };
                List<object?> propertyValues = new List<object?> { ids[i] };

                if (modelDef.IsTimestampDBModel && !oldTimestamps[i].HasValue)
                {
                    throw DatabaseExceptions.TimestampNotExists(engineType, modelDef, propertyNames);
                }

                if (oldTimestamps[i].HasValue)
                {
                    propertyNames.Add(nameof(TimestampLongIdDbModel.Timestamp));
                    propertyValues.Add(oldTimestamps[i]!.Value);
                }

                IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, propertyNames, propertyValues, number.ToString());

                totalParameters.AddRange(parameters);

                string sql = SqlHelper.CreateDeleteByPropertiesSql(modelDef, propertyNames, number);

#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundDeletedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{sql}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundDeletedRows_Statement(engineType), engineType)}");
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

            return new EngineCommand(commandText, totalParameters);
        }

        #endregion

        #region Management

        public EngineCommand CreateTableCreateCommand(DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength)
        {
            string sql = SqlHelper.GetTableCreateSql(modelDef, addDropStatement, varcharDefaultLength);

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
    }
}
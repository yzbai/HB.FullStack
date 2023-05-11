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

        private string GetCachedSql(SqlType sqlType, DbModelDef[] modelDefs, IEnumerable<string>? propertyNames = null, bool addOrUpdateReturnModel = false)
        {
            string cacheKey = GetCommandTextCacheKey(sqlType, modelDefs, propertyNames, addOrUpdateReturnModel);

            if (!_commandTextCache.TryGetValue(cacheKey, out string? commandText))
            {
                commandText = sqlType switch
                {
                    SqlType.Select => SqlHelper.CreateSelectSql(modelDefs),
                    SqlType.Insert => SqlHelper.CreateInsertSql(modelDefs[0]),

                    SqlType.UpdateIgnoreConflictCheck => SqlHelper.CreateUpdateIgnoreConflictCheckSql(modelDefs[0]) ,
                    SqlType.Update => SqlHelper.CreateUpdateModelSql(modelDefs[0]),
                    SqlType.UpdatePropertiesTimestamp => SqlHelper.CreateUpdatePropertiesSql(modelDefs[0], propertyNames!),
                    //SqlType.UpdatePropertiesUsingTimestampCompare => SqlHelper.CreateUpdatePropertiesUsingTimestampCompareSql(modelDefs[0], propertyNames!),
                    SqlType.UpdatePropertiesTimeless => SqlHelper.CreateUpdatePropertiesUsingCompareSql(modelDefs[0], propertyNames!),

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

        

        

        

        #region 更改 - UpdateProperties

        public DbEngineCommand CreateUpdatePropertiesTimestampCommand(DbModelDef modelDef, TimestampUpdatePack updatePack, string lastUser)
        {
            if (!modelDef.IsTimestamp)
            {
                throw DbExceptions.UpdatePropertiesMethodWrong("TimelessDbModel 应该使用值比较法", updatePack.PropertyNames, modelDef);
            }

            DbEngineType engineType = modelDef.EngineType;

            if (!updatePack.OldTimestamp.HasValue)
            {
                throw DbExceptions.TimestampNotExists(engineType: engineType, modelDef: modelDef, propertyNames: updatePack.PropertyNames);
            }

            updatePack.NewTimestamp ??= TimeUtil.Timestamp;

            IList<string> curPropertyNames = new List<string>(updatePack.PropertyNames)
            {
                nameof(DbModel2<long>.Id),
                nameof(BaseDbModel.LastUser),
                nameof(ITimestamp.Timestamp)
            };
            IList<object?> curPropertyValues = new List<object?>(updatePack.NewPropertyValues)
            {
                updatePack.Id,
                lastUser,
                updatePack.NewTimestamp.Value
            };

            IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, curPropertyNames, curPropertyValues);

            parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0", updatePack.OldTimestamp.Value));

            return new DbEngineCommand(
                GetCachedSql(SqlType.UpdatePropertiesTimestamp, new DbModelDef[] { modelDef }, curPropertyNames),
                parameters);
        }

        public DbEngineCommand CreateBatchUpdatePropertiesTimestampCommand(DbModelDef modelDef, IList<TimestampUpdatePack> updatePacks, string lastUser, bool needTrans)
        {
            if (!modelDef.IsTimestamp)
            {
                throw DbExceptions.UpdatePropertiesMethodWrong("Batch TimelessDbModel 应该使用值比较法", updatePacks[0].PropertyNames, modelDef);
            }

            DbEngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            DbModelPropertyDef? timestampProperty = modelDef.GetDbPropertyDef(nameof(ITimestamp.Timestamp))!;

            foreach (TimestampUpdatePack updatePack in updatePacks)
            {

                if (!updatePack.OldTimestamp.HasValue)
                {
                    throw DbExceptions.TimestampNotExists(engineType: engineType, modelDef: modelDef, propertyNames: updatePack.PropertyNames);
                }

                updatePack.NewTimestamp ??= TimeUtil.Timestamp;

                #region Parameters

                IList<string> curPropertyNames = new List<string>(updatePack.PropertyNames)
                {
                    nameof(TimestampLongIdDbModel.Id),
                    nameof(TimestampDbModel.LastUser),
                    nameof(TimestampDbModel.Timestamp)
                };
                IList<object?> curPropertyValues = new List<object?>(updatePack.NewPropertyValues)
                {
                    updatePack.Id,
                    lastUser,
                    updatePack.NewTimestamp.Value
                };

                IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    curPropertyNames,
                    curPropertyValues,
                    number.ToString());

                parameters.Add(new KeyValuePair<string, object>($"{SqlHelper.DbParameterName_Timestamp}_{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}", updatePack.OldTimestamp.Value));

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

            return new DbEngineCommand(commandText, totalParameters);
        }

        public DbEngineCommand CreateUpdatePropertiesTimelessCommand(DbModelDef modelDef, OldNewCompareUpdatePack updatePack, string lastUser)
        {
            if (modelDef.IsTimestamp)
            {
                throw DbExceptions.UpdatePropertiesMethodWrong("TimestampDBModel 应该使用 Timestamp解决冲突", updatePack.PropertyNames, modelDef);
            }

            List<string> curPropertyNames = new List<string>(updatePack.PropertyNames);
            List<object?> curNewPropertyValues = new List<object?>(updatePack.NewPropertyValues);

            var oldParameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                curPropertyNames,
                updatePack.OldPropertyValues,
                $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_0");

            curPropertyNames.Add(nameof(TimelessLongIdDbModel.Id));
            curPropertyNames.Add(nameof(TimelessLongIdDbModel.LastUser));
            curNewPropertyValues.Add(updatePack.Id);
            curNewPropertyValues.Add(lastUser);

            var newParameters = DbModelConvert.PropertyValuesToParameters(
                modelDef,
                _modelDefFactory,
                curPropertyNames,
                curNewPropertyValues,
                $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_0");

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(oldParameters);
            parameters.AddRange(newParameters);

            //使用propertyNames而不是curPropertyNames
            string sql = GetCachedSql(SqlType.UpdatePropertiesTimeless, new DbModelDef[] { modelDef }, updatePack.PropertyNames);

            return new DbEngineCommand(sql, parameters);
        }

        public DbEngineCommand CreateBatchUpdatePropertiesTimelessCommand(DbModelDef modelDef, IList<OldNewCompareUpdatePack> updatePacks, string lastUser, bool needTrans)
        {
            if (modelDef.IsTimestamp)
            {
                throw DbExceptions.UpdatePropertiesMethodWrong("TimestampDBModel 应该使用 Timestamp解决冲突", updatePacks[0].PropertyNames, modelDef);
            }

            ThrowIf.Empty(updatePacks, nameof(updatePacks));

            DbEngineType engineType = modelDef.EngineType;

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (OldNewCompareUpdatePack updatePack in updatePacks)
            {
                List<string> curPropertyNames = new List<string>(updatePack.PropertyNames);
                List<object?> curNewPropertyValues = new List<object?>(updatePack.NewPropertyValues);

                #region Parameters

                var oldParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    curPropertyNames,
                    updatePack.OldPropertyValues,
                    $"{SqlHelper.OLD_PROPERTY_VALUE_SUFFIX}_{number}");

                curPropertyNames.Add(nameof(TimestampLongIdDbModel.Id));
                curPropertyNames.Add(nameof(DbModel.LastUser));
                curNewPropertyValues.Add(updatePack.Id);
                curNewPropertyValues.Add(lastUser);

                var newParameters = DbModelConvert.PropertyValuesToParameters(
                    modelDef,
                    _modelDefFactory,
                    curPropertyNames,
                    curNewPropertyValues,
                    $"{SqlHelper.NEW_PROPERTY_VALUES_SUFFIX}_{number}");

                totalParameters.AddRange(oldParameters);
                totalParameters.AddRange(newParameters);

                #endregion

                string sql = SqlHelper.CreateUpdatePropertiesUsingCompareSql(modelDef, updatePack.PropertyNames, number);

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

            return new DbEngineCommand(commandText, totalParameters);
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

        #region 更改 - Delete

        public DbEngineCommand CreateDeleteCommand(
            DbModelDef modelDef,
            object id,
            string lastUser,
            bool trulyDeleted,
            long? oldTimestamp,
            long? newTimestamp)
        {
            DbEngineType engineType = modelDef.EngineType;

            if (!trulyDeleted)
            {
                return CreateUpdatePropertiesTimestampCommand(
                    modelDef,
                    new TimestampUpdatePack
                    {
                        Id = id,
                        OldTimestamp = oldTimestamp,
                        NewTimestamp = newTimestamp,
                        PropertyNames = new List<string> { nameof(DbModel.Deleted) },
                        NewPropertyValues = new List<object?> { true }
                    },
                    lastUser);
            }

            List<string> propertyNames = new List<string> { nameof(TimestampLongIdDbModel.Id) };
            List<object?> propertyValues = new List<object?> { id };

            if (modelDef.IsTimestamp && !oldTimestamp.HasValue)
            {
                throw DbExceptions.TimestampNotExists(engineType, modelDef, propertyNames);
            }

            if (oldTimestamp.HasValue)
            {
                propertyNames.Add(nameof(TimestampLongIdDbModel.Timestamp));
                propertyValues.Add(oldTimestamp.Value);
            }

            string sql = GetCachedSql(SqlType.DeleteByProperties, new DbModelDef[] { modelDef }, propertyNames);

            IList<KeyValuePair<string, object>> parameters = DbModelConvert.PropertyValuesToParameters(modelDef, _modelDefFactory, propertyNames, propertyValues);

            return new DbEngineCommand(sql, parameters);
        }

        public DbEngineCommand CreateDeleteCommand<T>(
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

                return new DbEngineCommand(sql, parameters);
            }

            string deleteSql = GetCachedSql(SqlType.Delete, new DbModelDef[] { modelDef }) + whereExpression.ToStatement();

            return new DbEngineCommand(deleteSql, parameters);
        }

        public DbEngineCommand CreateBatchDeleteCommand(
            DbModelDef modelDef,
            IList<object> ids,
            IList<long?> oldTimestamps,
            IList<long?> newTimestamps,
            string lastUser,
            bool trulyDeleted,
            bool needTrans)
        {
            int count = ids.Count;

            DbEngineType engineType = modelDef.EngineType;

            if (!trulyDeleted)
            {
                List<string> propertyNames = new List<string> { nameof(DbModel.Deleted) };
                List<object?> propertyValues = new List<object?> { true };
                List<TimestampUpdatePack> updatePacks = new List<TimestampUpdatePack>();

                for (int i = 0; i < count; ++i)
                {
                    updatePacks.Add(new TimestampUpdatePack
                    {
                        Id = ids[i],
                        OldTimestamp = oldTimestamps[i],
                        NewTimestamp = newTimestamps[i],
                        PropertyNames = propertyNames,
                        NewPropertyValues = propertyValues
                    });
                }

                return CreateBatchUpdatePropertiesTimestampCommand(modelDef, updatePacks, lastUser, needTrans);
            }

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> totalParameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            for (int i = 0; i < count; ++i)
            {
                List<string> propertyNames = new List<string> { nameof(TimestampLongIdDbModel.Id) };
                List<object?> propertyValues = new List<object?> { ids[i] };

                if (modelDef.IsTimestamp && !oldTimestamps[i].HasValue)
                {
                    throw DbExceptions.TimestampNotExists(engineType, modelDef, propertyNames);
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

            return new DbEngineCommand(commandText, totalParameters);
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
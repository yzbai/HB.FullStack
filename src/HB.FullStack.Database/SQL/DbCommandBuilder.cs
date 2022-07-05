

using HB.FullStack.Common;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;

using Microsoft;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

#pragma warning disable CA1822 // Mark members as static
namespace HB.FullStack.Database
{
    internal class DbCommandBuilder : IDbCommandBuilder
    {
        private readonly IEntityDefFactory _entityDefFactory;
        private readonly ISQLExpressionVisitor _expressionVisitor;
        private readonly ConcurrentDictionary<string, string> _commandTextCache = new ConcurrentDictionary<string, string>();

        public DbCommandBuilder(IEntityDefFactory entityDefFactory, ISQLExpressionVisitor expressionVisitor)
        {
            _entityDefFactory = entityDefFactory;
            _expressionVisitor = expressionVisitor;
        }

        private string GetCachedSql(EngineType engineType, SqlType commandTextType, EntityDef[] entityDefs, IEnumerable<string>? propertyNames = null)
        {
            string cacheKey = GetCommandTextCacheKey(commandTextType, entityDefs, propertyNames);

            if (!_commandTextCache.TryGetValue(cacheKey, out string? commandText))
            {
                commandText = commandTextType switch
                {
                    SqlType.AddEntity => SqlHelper.CreateAddEntitySql(entityDefs[0], engineType, true),
                    SqlType.UpdateEntity => SqlHelper.CreateUpdateEntitySql(entityDefs[0]),
                    SqlType.UpdateFieldsUsingVersionCompare => SqlHelper.CreateUpdateFieldsUsingVersionCompareSql(entityDefs[0], propertyNames!),
                    SqlType.UpdateFieldsUsingOldNewCompare => SqlHelper.CreateUpdateFieldsUsingOldNewCompareSql(entityDefs[0], propertyNames!),
                    SqlType.DeleteEntity => SqlHelper.CreateDeleteEntitySql(entityDefs[0]),
                    SqlType.SelectEntity => SqlHelper.CreateSelectEntitySql(entityDefs),
                    SqlType.Delete => SqlHelper.CreateDeleteSql(entityDefs[0]),
                    SqlType.AddOrUpdateEntity => SqlHelper.CreateAddOrUpdateSql(entityDefs[0], engineType, true),
                    _ => throw new NotSupportedException(),
                };

                _commandTextCache.TryAdd(cacheKey, commandText);
            }

            return commandText;

            static string GetCommandTextCacheKey(SqlType textType, EntityDef[] entityDefs, IEnumerable<string>? propertyNames)
            {
                StringBuilder builder = new StringBuilder(entityDefs[0].DatabaseName);

                foreach (EntityDef entityDef in entityDefs)
                {
                    builder.Append(entityDef.TableName);
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

        public FromExpression<T> From<T>(EngineType engineType) where T : DatabaseEntity, new()
        {
            return new FromExpression<T>(engineType, _entityDefFactory, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>(EngineType engineType) where T : DatabaseEntity, new()
        {
            return new WhereExpression<T>(engineType, _entityDefFactory, _expressionVisitor);
        }

        public WhereExpression<T> Where<T>(EngineType engineType, string sqlFilter, params object[] filterParams) where T : DatabaseEntity, new()
        {
            return new WhereExpression<T>(engineType, _entityDefFactory, _expressionVisitor).Where(sqlFilter, filterParams);
        }

        public WhereExpression<T> Where<T>(EngineType engineType, Expression<Func<T, bool>> predicate) where T : DatabaseEntity, new()
        {
            return new WhereExpression<T>(engineType, _entityDefFactory, _expressionVisitor).Where(predicate);
        }

        #endregion

        #region 查询

        public EngineCommand CreateRetrieveCommand<T>(EngineType engineType, EntityDef entityDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand(GetCachedSql(engineType, SqlType.SelectEntity, new EntityDef[] { entityDef }), fromCondition, whereCondition, engineType);
        }

        public EngineCommand CreateCountCommand<T>(EngineType engineType, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand("SELECT COUNT(1) ", fromCondition, whereCondition, engineType);
        }

        public EngineCommand CreateRetrieveCommand<T1, T2>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(engineType, SqlType.SelectEntity, returnEntityDefs),
                fromCondition,
                whereCondition,
                engineType);
        }

        public EngineCommand CreateRetrieveCommand<T1, T2, T3>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
            where T3 : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(engineType, SqlType.SelectEntity, returnEntityDefs),
                fromCondition,
                whereCondition,
                engineType);
        }

        public EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType engineType, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params EntityDef[] returnEntityDefs)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(engineType, SqlType.SelectEntity, returnEntityDefs),
                fromCondition,
                whereCondition,
                engineType);
        }

        private EngineCommand AssembleRetrieveCommand<TFrom, TWhere>(string selectText, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, EngineType engineType)
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            string sql = selectText;
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            if (fromCondition == null)
            {
                fromCondition = new FromExpression<TFrom>(engineType, _entityDefFactory, _expressionVisitor);
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

        public EngineCommand CreateAddCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.AddEntity, new EntityDef[] { entityDef }),
                entity.EntityToParameters(entityDef, engineType, _entityDefFactory));
        }

        public EngineCommand CreateUpdateCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.UpdateEntity, new EntityDef[] { entityDef }),
                entity.EntityToParameters(entityDef, engineType, _entityDefFactory));
        }

        public EngineCommand CreateUpdateFieldsCommand(EngineType engineType, EntityDef entityDef, object id, int updateToVersion, string lastUser,
            IList<string> propertyNames, IList<object?> propertyValues)
        {
            propertyNames.Add(nameof(LongIdEntity.Id));
            propertyNames.Add(nameof(Entity.Version));
            propertyNames.Add(nameof(Entity.LastUser));
            propertyNames.Add(nameof(Entity.LastTime));

            propertyValues.Add(id);
            propertyValues.Add(updateToVersion);
            propertyValues.Add(lastUser);
            propertyValues.Add(TimeUtil.UtcNow);

            return new EngineCommand(
                GetCachedSql(engineType, SqlType.UpdateFieldsUsingVersionCompare, new EntityDef[] { entityDef }, propertyNames),
                EntityMapper.PropertyValuesToParameters(entityDef, engineType, _entityDefFactory, propertyNames, propertyValues));
        }

        private const string OldPropertyValueSuffix = "old";
        private const string NewPropertyValueSuffix = "new";

        public EngineCommand CreateUpdateFieldsCommand(EngineType engineType, EntityDef entityDef, object id, string lastUser, IList<string> propertyNames, IList<object?> oldPropertyValues,
            IList<object?> newPropertyValues)
        {
            string sql = GetCachedSql(engineType, SqlType.UpdateFieldsUsingOldNewCompare, new EntityDef[] { entityDef }, propertyNames);

            var oldParameters = EntityMapper.PropertyValuesToParameters(entityDef, engineType, _entityDefFactory, propertyNames, oldPropertyValues, $"{OldPropertyValueSuffix}_0));

            propertyNames.Add(nameof(LongIdEntity.Id));
            propertyNames.Add(nameof(Entity.LastUser));
            propertyNames.Add(nameof(Entity.LastTime));

            newPropertyValues.Add(id);
            newPropertyValues.Add(lastUser);
            newPropertyValues.Add(TimeUtil.UtcNow);

            var newParameters = EntityMapper.PropertyValuesToParameters(entityDef, engineType, _entityDefFactory, propertyNames, newPropertyValues, $"{NewPropertyValueSuffix}_0");

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>(oldParameters);
            parameters.AddRange(newParameters);

            return new EngineCommand(sql, parameters);
        }

        public EngineCommand CreateDeleteCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.DeleteEntity, new EntityDef[] { entityDef }),
                entity.EntityToParameters(entityDef, engineType, _entityDefFactory));
        }

        public EngineCommand CreateDeleteCommand<T>(EngineType engineType, EntityDef entityDef, WhereExpression<T> whereExpression) where T : DatabaseEntity, new()
        {
            Requires.NotNull(whereExpression, nameof(whereExpression));

            string sql = GetCachedSql(engineType, SqlType.Delete, new EntityDef[] { entityDef }) + whereExpression.ToStatement(engineType);

            return new EngineCommand(sql, whereExpression.GetParameters());
        }

        public EngineCommand CreateBatchAddCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            bool isIdAutoIncrement = entityDef.IsIdAutoIncrement;

            foreach (T entity in entities)
            {
                string addCommandText = SqlHelper.CreateAddEntitySql(entityDef, engineType, false, number);

                parameters.AddRange(entity.EntityToParameters(entityDef, engineType, _entityDefFactory, number));

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

        public EngineCommand CreateBatchUpdateCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string updateCommandText = SqlHelper.CreateUpdateEntitySql(entityDef, number);

                parameters.AddRange(entity.EntityToParameters(entityDef, engineType, _entityDefFactory, number));

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

        public EngineCommand CreateBatchDeleteCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string deleteCommandText = SqlHelper.CreateDeleteEntitySql(entityDef, number);

                parameters.AddRange(entity.EntityToParameters(entityDef, engineType, _entityDefFactory, number));
#if NET6_0_OR_GREATER
                innerBuilder.Append(CultureInfo.InvariantCulture, $"{deleteCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundMatchedRows_Statement(engineType), engineType)}");
#elif NETSTANDARD2_1
                innerBuilder.Append($"{deleteCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundMatchedRows_Statement(engineType), engineType)}");
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

        #region Management


        public EngineCommand CreateTableCreateCommand(EngineType engineType, EntityDef entityDef, bool addDropStatement, int varcharDefaultLength)
        {
            string sql = SqlHelper.GetTableCreateSql(entityDef, addDropStatement, varcharDefaultLength, engineType);

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
        public EngineCommand CreateAddOrUpdateCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.AddOrUpdateEntity, new EntityDef[] { entityDef }),
                entity.EntityToParameters(entityDef, engineType, _entityDefFactory));
        }

        /// <summary>
        /// 只在客户端开放，因为不检查Version就update，并且无法更新entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="engineType"></param>
        /// <param name="entityDef"></param>
        /// <param name="entities"></param>
        /// <returns></returns>

        public EngineCommand CreateBatchAddOrUpdateCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities, bool needTrans) where T : DatabaseEntity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            //string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string addOrUpdateCommandText = SqlHelper.CreateAddOrUpdateSql(entityDef, engineType, false, number);

                parameters.AddRange(entity.EntityToParameters(entityDef, engineType, _entityDefFactory, number));

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
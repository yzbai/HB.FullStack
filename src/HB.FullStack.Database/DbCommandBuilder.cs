#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    internal class DbCommandBuilder : IDbCommandBuilder
    {
        private readonly ConcurrentDictionary<string, string> _commandTextCache;
        private readonly IDatabaseEngine _databaseEngine;

        public DbCommandBuilder(IDatabaseEngine databaseEngine)
        {
            _databaseEngine = databaseEngine;
            _commandTextCache = new ConcurrentDictionary<string, string>();
        }

        private string GetCachedSql(SqlType commandTextType, params EntityDef[] entityDefs)
        {
            string cacheKey = GetCommandTextCacheKey(commandTextType, entityDefs);

            if (!_commandTextCache.TryGetValue(cacheKey, out string commandText))
            {
                commandText = commandTextType switch
                {
                    SqlType.ADD => SqlHelper.CreateAddSql(entityDefs[0], _databaseEngine.EngineType, true),
                    SqlType.UPDATE => SqlHelper.CreateUpdateSql(entityDefs[0]),
                    SqlType.DELETE => SqlHelper.CreateDeleteSql(entityDefs[0]),
                    SqlType.SELECT => SqlHelper.CreateSelectSql(entityDefs),
                    _ => throw new NotImplementedException(),
                };

                _commandTextCache.TryAdd(cacheKey, commandText);
            }

            return commandText;

            static string GetCommandTextCacheKey(SqlType textType, params EntityDef[] entityDefs)
            {
                StringBuilder builder = new StringBuilder(entityDefs[0].DatabaseName);

                foreach (EntityDef entityDef in entityDefs)
                {
                    builder.Append($"{entityDef.TableName}_");
                }

                builder.Append(textType.ToString());

                return builder.ToString();
            }
        }

        #region 查询

        public FromExpression<T> NewFrom<T>() where T : Entity, new()
        {
            return new FromExpression<T>(_databaseEngine.EngineType);
        }

        public WhereExpression<T> NewWhere<T>() where T : Entity, new()
        {
            return new WhereExpression<T>(_databaseEngine.EngineType);
        }

        public IDbCommand CreateRetrieveCommand<T>(EntityDef entityDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : Entity, new()
        {
            return AssembleRetrieveCommand(GetCachedSql(SqlType.SELECT, entityDef), fromCondition, whereCondition);
        }

        public IDbCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : Entity, new()
        {
            return AssembleRetrieveCommand("SELECT COUNT(1) ", fromCondition, whereCondition);
        }

        public IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(SqlType.SELECT, returnEntityDefs),
                fromCondition,
                whereCondition);
        }

        public IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new()
            where T3 : Entity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(SqlType.SELECT, returnEntityDefs),
                fromCondition,
                whereCondition);
        }

        public IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params EntityDef[] returnEntityDefs)
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(SqlType.SELECT, returnEntityDefs),
                fromCondition,
                whereCondition);
        }

        private IDbCommand AssembleRetrieveCommand<TFrom, TWhere>(string selectText, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition)
            where TFrom : Entity, new()
            where TWhere : Entity, new()
        {
            string sql = selectText;
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            if (fromCondition == null)
            {
                fromCondition = NewFrom<TFrom>();
            }

            sql += fromCondition.ToString();
            parameters.AddRange(fromCondition.GetParameters());

            if (whereCondition != null)
            {
                sql += whereCondition.ToString(_databaseEngine.EngineType);

                parameters.AddRange(whereCondition.GetParameters());
            }

            return _databaseEngine.CreateTextCommand(sql, parameters);
        }

        #endregion 查询

        #region 更改

        public IDbCommand CreateAddCommand<T>(EntityDef entityDef, T entity) where T : Entity, new()
        {
            return _databaseEngine.CreateTextCommand(
                GetCachedSql(SqlType.ADD, entityDef),
                entity.ToParameters(entityDef, _databaseEngine.EngineType));
        }

        public IDbCommand CreateUpdateCommand<T>(EntityDef entityDef, T entity) where T : Entity, new()
        {
            return _databaseEngine.CreateTextCommand(
                GetCachedSql(SqlType.UPDATE, entityDef),
                entity.ToParameters(entityDef, _databaseEngine.EngineType));
        }

        public IDbCommand CreateDeleteCommand<T>(EntityDef entityDef, T entity) where T : Entity, new()
        {
            return _databaseEngine.CreateTextCommand(
                GetCachedSql(SqlType.DELETE, entityDef),
                entity.ToParameters(entityDef, _databaseEngine.EngineType));
        }

        public IDbCommand CreateBatchAddCommand<T>(EntityDef entityDef, IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string addCommandText = SqlHelper.CreateAddSql(entityDef, _databaseEngine.EngineType, false, number);

                parameters.AddRange(entity.ToParameters(entityDef, _databaseEngine.EngineType, number));

                innerBuilder.Append($"{addCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.GetLastInsertIdStatement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string commandText = $"{SqlHelper.TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{SqlHelper.TempTable_Create_Id(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{SqlHelper.TempTable_Select_Id(tempTableName, _databaseEngine.EngineType)}{SqlHelper.TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return _databaseEngine.CreateTextCommand(commandText, parameters);
        }

        public IDbCommand CreateBatchUpdateCommand<T>(EntityDef entityDef, IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string updateCommandText = SqlHelper.CreateUpdateSql(entityDef, number);

                parameters.AddRange(entity.ToParameters(entityDef, _databaseEngine.EngineType, number));

                innerBuilder.Append($"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundChanges_Statement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string commandText = $"{SqlHelper.TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{SqlHelper.TempTable_Create_Id(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{SqlHelper.TempTable_Select_Id(tempTableName, _databaseEngine.EngineType)}{SqlHelper.TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return _databaseEngine.CreateTextCommand(commandText, parameters);
        }

        public IDbCommand CreateBatchDeleteCommand<T>(EntityDef entityDef, IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string deleteCommandText = SqlHelper.CreateDeleteSql(entityDef, number);

                parameters.AddRange(entity.ToParameters(entityDef, _databaseEngine.EngineType, number));

                innerBuilder.Append($"{deleteCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundChanges_Statement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string commandText = $"{SqlHelper.TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{SqlHelper.TempTable_Create_Id(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{SqlHelper.TempTable_Select_Id(tempTableName, _databaseEngine.EngineType)}{SqlHelper.TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return _databaseEngine.CreateTextCommand(commandText, parameters);
        }

        #endregion 更改

        #region Management

        public IDbCommand CreateTableCreateCommand(EntityDef entityDef, bool addDropStatement)
        {
            string sql = SqlHelper.GetTableCreateSql(entityDef, addDropStatement, EntityDefFactory.VarcharDefaultLength, _databaseEngine.EngineType);

            return _databaseEngine.CreateTextCommand(sql);
        }

        public IDbCommand CreateIsTableExistCommand(string databaseName, string tableName)
        {
            string sql = SqlHelper.GetIsTableExistSql(_databaseEngine.EngineType);

            KeyValuePair<string, object>[] parameters = new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>("@tableName", tableName ),
                new KeyValuePair<string, object>( "@databaseName", databaseName)
            };

            return _databaseEngine.CreateTextCommand(sql, parameters);
        }

        public IDbCommand CreateSystemInfoRetrieveCommand()
        {
            string sql = SqlHelper.GetSystemInfoRetrieveSql(_databaseEngine.EngineType);

            return _databaseEngine.CreateTextCommand(sql);
        }

        public IDbCommand CreateSystemVersionUpdateCommand(string databaseName, int version)
        {
            string sql;
            KeyValuePair<string, object>[] parameters;

            if (version == 1)
            {
                sql = SqlHelper.GetSystemInfoCreateSql(_databaseEngine.EngineType);

                parameters = new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("@databaseName", databaseName) };
            }
            else
            {
                sql = SqlHelper.GetSystemInfoUpdateVersionSql(_databaseEngine.EngineType);

                parameters = new KeyValuePair<string, object>[] { new KeyValuePair<string, object>("@Value", version) };
            }

            return _databaseEngine.CreateTextCommand(sql, parameters);
        }

        #endregion Management
    }
}
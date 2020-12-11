#nullable enable

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.FullStack.Database.SQL
{
    internal enum CommandTextType
    {
        ADD,
        UPDATE,
        DELETE,
        SELECT
    }

    internal partial class SQLBuilder : ISQLBuilder
    {
        private readonly ConcurrentDictionary<string, string> _commandTextCache;
        private readonly IDatabaseEntityDefFactory _entityDefFactory;
        private readonly IDatabaseEngine _databaseEngine;

        public SQLBuilder(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory)
        {
            _databaseEngine = databaseEngine;
            _entityDefFactory = entityDefFactory;
            _commandTextCache = new ConcurrentDictionary<string, string>();
        }

        private string GetCachedCommandText(CommandTextType commandTextType, params DatabaseEntityDef[] entityDefs)
        {
            string cacheKey = GetCommandTextCacheKey(commandTextType, entityDefs);

            if (!_commandTextCache.TryGetValue(cacheKey, out string commandText))
            {
                commandText = commandTextType switch
                {
                    CommandTextType.ADD => CreateAddCommandText(entityDefs[0], _databaseEngine.EngineType, true),
                    CommandTextType.UPDATE => CreateUpdateCommandText(entityDefs[0]),
                    CommandTextType.DELETE => CreateDeleteCommandText(entityDefs[0]),
                    CommandTextType.SELECT => CreateSelectCommandText(entityDefs),
                    _ => throw new NotImplementedException(),
                };

                _commandTextCache.TryAdd(cacheKey, commandText);
            }

            return commandText;

            static string GetCommandTextCacheKey(CommandTextType textType, params DatabaseEntityDef[] entityDefs)
            {
                StringBuilder builder = new StringBuilder(entityDefs[0].DatabaseName);

                foreach (DatabaseEntityDef entityDef in entityDefs)
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
            return new FromExpression<T>(_databaseEngine, _entityDefFactory);
        }

        public WhereExpression<T> NewWhere<T>() where T : Entity, new()
        {
            return new WhereExpression<T>(_databaseEngine, _entityDefFactory);
        }

        public IDbCommand CreateRetrieveCommand<T>(DatabaseEntityDef entityDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : Entity, new()
        {
            return AssembleRetrieveCommand(GetCachedCommandText(CommandTextType.SELECT, entityDef), fromCondition, whereCondition);
        }

        public IDbCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : Entity, new()
        {
            return AssembleRetrieveCommand("SELECT COUNT(1) ", fromCondition, whereCondition);
        }

        public IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DatabaseEntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedCommandText(CommandTextType.SELECT, returnEntityDefs),
                fromCondition,
                whereCondition);
        }

        public IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DatabaseEntityDef[] returnEntityDefs)
            where T1 : Entity, new()
            where T2 : Entity, new()
            where T3 : Entity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedCommandText(CommandTextType.SELECT, returnEntityDefs),
                fromCondition,
                whereCondition);
        }

        public IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DatabaseEntityDef[] returnEntityDefs)
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedCommandText(CommandTextType.SELECT, returnEntityDefs),
                fromCondition,
                whereCondition);
        }

        private IDbCommand AssembleRetrieveCommand<TFrom, TWhere>(string selectText, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition)
            where TFrom : Entity, new()
            where TWhere : Entity, new()
        {
            string commandText = selectText;
            List<IDataParameter> commandParameters = new List<IDataParameter>();

            if (fromCondition == null)
            {
                fromCondition = NewFrom<TFrom>();
            }

            commandText += fromCondition.ToString();

            foreach (KeyValuePair<string, object> pair in fromCondition.GetParameters())
            {
                IDataParameter param = _databaseEngine.CreateParameter(pair.Key, pair.Value);
                commandParameters.Add(param);
            }

            if (whereCondition != null)
            {
                commandText += whereCondition.ToString();

                foreach (KeyValuePair<string, object> pair in whereCondition.GetParameters())
                {
                    IDataParameter param = _databaseEngine.CreateParameter(pair.Key, pair.Value);
                    commandParameters.Add(param);
                }
            }

            return _databaseEngine.CreateTextCommand(commandText, commandParameters.ToArray());
        }

        #endregion

        #region 更改

        public IDbCommand CreateAddCommand<T>(DatabaseEntityDef entityDef, T entity) where T : Entity, new()
        {
            return _databaseEngine.CreateTextCommand(
                GetCachedCommandText(CommandTextType.ADD, entityDef),
                GetCommandParameters(_databaseEngine, entityDef, entity));
        }

        public IDbCommand CreateUpdateCommand<T>(DatabaseEntityDef entityDef, T entity) where T : Entity, new()
        {
            return _databaseEngine.CreateTextCommand(
                GetCachedCommandText(CommandTextType.UPDATE, entityDef),
                GetCommandParameters(_databaseEngine, entityDef, entity));
        }

        public IDbCommand CreateDeleteCommand<T>(DatabaseEntityDef entityDef, T entity) where T : Entity, new()
        {

            return _databaseEngine.CreateTextCommand(
                GetCachedCommandText(CommandTextType.DELETE, entityDef),
                GetCommandParameters(_databaseEngine, entityDef, entity));
        }

        public IDbCommand CreateBatchAddCommand<T>(DatabaseEntityDef entityDef, IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                string addCommandText = CreateAddCommandText(entityDef, _databaseEngine.EngineType, false, number);

                parameters.AddRange(GetCommandParameters(_databaseEngine, entityDef, entity, number));

                innerBuilder.Append($"{addCommandText}{TempTable_Insert_Id(tempTableName, GetLastInsertIdStatement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string commandText = $"{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{TempTable_Create_Id(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{TempTable_Select_Id(tempTableName, _databaseEngine.EngineType)}{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return _databaseEngine.CreateTextCommand(commandText, parameters.ToArray());
        }

        public IDbCommand CreateBatchUpdateCommand<T>(DatabaseEntityDef entityDef, IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                string updateCommandText = CreateUpdateCommandText(entityDef, number);

                parameters.AddRange(GetCommandParameters(_databaseEngine, entityDef, entity, number));

                innerBuilder.Append($"{updateCommandText}{TempTable_Insert_Id(tempTableName, FoundChanges_Statement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string commandText = $"{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{TempTable_Create_Id(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{TempTable_Select_Id(tempTableName, _databaseEngine.EngineType)}{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return _databaseEngine.CreateTextCommand(commandText, parameters.ToArray());
        }

        public IDbCommand CreateBatchDeleteCommand<T>(DatabaseEntityDef entityDef, IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                string deleteCommandText = CreateDeleteCommandText(entityDef, number);

                parameters.AddRange(GetCommandParameters(_databaseEngine, entityDef, entity, number));

                innerBuilder.Append($"{deleteCommandText}{TempTable_Insert_Id(tempTableName, FoundChanges_Statement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string commandText = $"{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{TempTable_Create_Id(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{TempTable_Select_Id(tempTableName, _databaseEngine.EngineType)}{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return _databaseEngine.CreateTextCommand(commandText, parameters.ToArray());
        }

        #endregion

        #region Management

        public IDbCommand CreateTableCreateCommand(DatabaseEntityDef entityDef, bool addDropStatement)
        {

            string commandText = _databaseEngine.EngineType switch
            {
                DatabaseEngineType.MySQL => MySQL_Table_Create_Statement(entityDef, addDropStatement, _databaseEngine, _entityDefFactory.GetVarcharDefaultLength()),
                DatabaseEngineType.SQLite => SQLite_Table_Create_Statement(entityDef, addDropStatement, _databaseEngine),
                _ => throw new DatabaseException(ErrorCode.DatabaseUnSupportedType)
            };

            return _databaseEngine.CreateTextCommand(commandText);
        }

        public IDbCommand CreateIsTableExistCommand(string databaseName, string tableName)
        {
            string commandText = _databaseEngine.EngineType switch
            {
                DatabaseEngineType.MySQL => _mysql_isTableExistsStatement,
                DatabaseEngineType.SQLite => _sqlite_isTableExistsStatement,
                _ => string.Empty
            };

            IDataParameter[] parameters = new IDataParameter[] {
                _databaseEngine.CreateParameter("@tableName", tableName),
                _databaseEngine.CreateParameter("@databaseName", databaseName)
            };

            return _databaseEngine.CreateTextCommand(commandText, parameters);
        }

        public IDbCommand CreateSystemInfoRetrieveCommand()
        {
            string commandText = _databaseEngine.EngineType switch
            {
                DatabaseEngineType.MySQL => _mysql_tbSysInfoRetrieve,
                DatabaseEngineType.SQLite => _sqlite_tbSysInfoRetrieve,
                _ => string.Empty
            };

            return _databaseEngine.CreateTextCommand(commandText);
        }

        public IDbCommand CreateSystemVersionUpdateCommand(string databaseName, int version)
        {
            string commandText;
            IDataParameter[] parameters;

            if (version == 1)
            {
                commandText = _databaseEngine.EngineType switch
                {
                    DatabaseEngineType.MySQL => _mysql_tbSysInfoCreate,
                    DatabaseEngineType.SQLite => _sqlite_tbSysInfoCreate,
                    _ => string.Empty
                };

                parameters = new IDataParameter[] { _databaseEngine.CreateParameter("@databaseName", databaseName) };
            }
            else
            {
                commandText = _databaseEngine.EngineType switch
                {
                    DatabaseEngineType.MySQL => _mysql_tbSysInfoUpdateVersion,
                    DatabaseEngineType.SQLite => _sqlite_tbSysInfoUpdateVersion,
                    _ => string.Empty
                };

                parameters = new IDataParameter[] { _databaseEngine.CreateParameter("@Value", version) };
            }

            return _databaseEngine.CreateTextCommand(commandText, parameters);
        }

        #endregion
    }
}
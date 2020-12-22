#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Mapper;
using HB.FullStack.Database.SQL;
using Microsoft;

namespace HB.FullStack.Database
{
    internal static class DbCommandBuilder
    {
        private static readonly ConcurrentDictionary<string, string> _commandTextCache = new ConcurrentDictionary<string, string>();

        private static string GetCachedSql(EngineType engineType, SqlType commandTextType, params EntityDef[] entityDefs)
        {
            string cacheKey = GetCommandTextCacheKey(commandTextType, entityDefs);

            if (!_commandTextCache.TryGetValue(cacheKey, out string commandText))
            {
                commandText = commandTextType switch
                {
                    SqlType.AddEntity => SqlHelper.CreateAddEntitySql(entityDefs[0], engineType, true),
                    SqlType.UpdateEntity => SqlHelper.CreateUpdateEntitySql(entityDefs[0]),
                    SqlType.DeleteEntity => SqlHelper.CreateDeleteEntitySql(entityDefs[0]),
                    SqlType.SelectEntity => SqlHelper.CreateSelectEntitySql(entityDefs),
                    SqlType.Delete => SqlHelper.CreateDeleteSql(entityDefs[0]),
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

        public static EngineCommand CreateRetrieveCommand<T>(EngineType engineType, EntityDef entityDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand(GetCachedSql(engineType, SqlType.SelectEntity, entityDef), fromCondition, whereCondition, engineType);
        }

        public static EngineCommand CreateCountCommand<T>(EngineType engineType, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand("SELECT COUNT(1) ", fromCondition, whereCondition, engineType);
        }

        public static EngineCommand CreateRetrieveCommand<T1, T2>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
        {
            return AssembleRetrieveCommand(
                GetCachedSql(engineType, SqlType.SelectEntity, returnEntityDefs),
                fromCondition,
                whereCondition,
                engineType);
        }

        public static EngineCommand CreateRetrieveCommand<T1, T2, T3>(EngineType engineType, FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params EntityDef[] returnEntityDefs)
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

        public static EngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType engineType, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params EntityDef[] returnEntityDefs)
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

        private static EngineCommand AssembleRetrieveCommand<TFrom, TWhere>(string selectText, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, EngineType engineType)
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            string sql = selectText;
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            if (fromCondition == null)
            {
                fromCondition = new FromExpression<TFrom>(engineType);
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

        public static EngineCommand CreateAddCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.AddEntity, entityDef),
                entity.ToParameters(entityDef, engineType));
        }

        public static EngineCommand CreateUpdateCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.UpdateEntity, entityDef),
                entity.ToParameters(entityDef, engineType));
        }

        public static EngineCommand CreateDeleteCommand<T>(EngineType engineType, EntityDef entityDef, T entity) where T : DatabaseEntity, new()
        {
            return new EngineCommand(
                GetCachedSql(engineType, SqlType.DeleteEntity, entityDef),
                entity.ToParameters(entityDef, engineType));
        }

        public static EngineCommand CreateDeleteCommand<T>(EngineType engineType, EntityDef entityDef, WhereExpression<T> whereExpression) where T : DatabaseEntity, new()
        {
            Requires.NotNull(whereExpression, nameof(whereExpression));

            string sql = GetCachedSql(engineType, SqlType.Delete, entityDef) + whereExpression.ToStatement(engineType);

            return new EngineCommand(sql, whereExpression.GetParameters());
        }

        public static EngineCommand CreateBatchAddCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities) where T : DatabaseEntity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string addCommandText = SqlHelper.CreateAddEntitySql(entityDef, engineType, false, number);

                parameters.AddRange(entity.ToParameters(entityDef, engineType, number));

                innerBuilder.Append($"{addCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.GetLastInsertIdStatement(engineType), engineType)}");

                number++;
            }

            string commandText = $"{SqlHelper.TempTable_Drop(tempTableName, engineType)}{SqlHelper.TempTable_Create_Id(tempTableName, engineType)}{innerBuilder}{SqlHelper.TempTable_Select_Id(tempTableName, engineType)}{SqlHelper.TempTable_Drop(tempTableName, engineType)}";

            return new EngineCommand(commandText, parameters);
        }

        public static EngineCommand CreateBatchUpdateCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities) where T : DatabaseEntity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string updateCommandText = SqlHelper.CreateUpdateEntitySql(entityDef, number);

                parameters.AddRange(entity.ToParameters(entityDef, engineType, number));

                innerBuilder.Append($"{updateCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundChanges_Statement(engineType), engineType)}");

                number++;
            }

            string commandText = $"{SqlHelper.TempTable_Drop(tempTableName, engineType)}{SqlHelper.TempTable_Create_Id(tempTableName, engineType)}{innerBuilder}{SqlHelper.TempTable_Select_Id(tempTableName, engineType)}{SqlHelper.TempTable_Drop(tempTableName, engineType)}";

            return new EngineCommand(commandText, parameters);
        }

        public static EngineCommand CreateBatchDeleteCommand<T>(EngineType engineType, EntityDef entityDef, IEnumerable<T> entities) where T : DatabaseEntity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();
            int number = 0;

            foreach (T entity in entities)
            {
                string deleteCommandText = SqlHelper.CreateDeleteEntitySql(entityDef, number);

                parameters.AddRange(entity.ToParameters(entityDef, engineType, number));

                innerBuilder.Append($"{deleteCommandText}{SqlHelper.TempTable_Insert_Id(tempTableName, SqlHelper.FoundChanges_Statement(engineType), engineType)}");

                number++;
            }

            string commandText = $"{SqlHelper.TempTable_Drop(tempTableName, engineType)}{SqlHelper.TempTable_Create_Id(tempTableName, engineType)}{innerBuilder}{SqlHelper.TempTable_Select_Id(tempTableName, engineType)}{SqlHelper.TempTable_Drop(tempTableName, engineType)}";

            return new EngineCommand(commandText, parameters);
        }

        #endregion 更改

        #region Management

        public static EngineCommand CreateTableCreateCommand(EngineType engineType, EntityDef entityDef, bool addDropStatement)
        {
            string sql = SqlHelper.GetTableCreateSql(entityDef, addDropStatement, EntityDefFactory.VarcharDefaultLength, engineType);

            return new EngineCommand(sql);
        }

        public static EngineCommand CreateIsTableExistCommand(EngineType engineType, string databaseName, string tableName)
        {
            string sql = SqlHelper.GetIsTableExistSql(engineType);

            var parameters = new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>("@tableName", tableName ),
                new KeyValuePair<string, object>( "@databaseName", databaseName)
            };

            return new EngineCommand(sql, parameters);
        }

        public static EngineCommand CreateSystemInfoRetrieveCommand(EngineType engineType)
        {
            string sql = SqlHelper.GetSystemInfoRetrieveSql(engineType);

            return new EngineCommand(sql);
        }

        public static EngineCommand CreateSystemVersionUpdateCommand(EngineType engineType, string databaseName, int version)
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
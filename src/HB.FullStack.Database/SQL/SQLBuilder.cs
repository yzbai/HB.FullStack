#nullable enable

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// 生成SQL语句与Command
    /// 多线程复用
    /// 目前只适用MYSQL
    /// 对以下字段考虑：
    /// ID：新增时自动生成
    /// Deleted：每次都带上Deleted=0条件
    /// LastUser：
    /// LastTime：不用动
    /// Version： 新增为0，更改时加1，删除时加1.
    /// 单例
    /// </summary>
    internal partial class SQLBuilder : ISQLBuilder
    {
        /// <summary>
        /// sql字典. 数据库名:TableName:操作-SQL语句
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _sqlStatementDict;
        private readonly IDatabaseEntityDefFactory _entityDefFactory;
        private readonly IDatabaseEngine _databaseEngine;

        public SQLBuilder(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory)
        {
            _databaseEngine = databaseEngine;
            _entityDefFactory = entityDefFactory;
            _sqlStatementDict = new ConcurrentDictionary<string, string>();
        }

        private IDbCommand AssembleCommand<TFrom, TWhere>(bool isRetrieve, string selectClause, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, IList<IDataParameter>? parameters)
            where TFrom : Entity, new()
            where TWhere : Entity, new()
        {
            IDbCommand command = _databaseEngine.CreateEmptyCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = selectClause;

            if (isRetrieve)
            {
                if (fromCondition == null)
                {
                    fromCondition = NewFrom<TFrom>();
                }

                command.CommandText += fromCondition.ToString();

                foreach (KeyValuePair<string, object> pair in fromCondition.GetParameters())
                {
                    IDataParameter param = _databaseEngine.CreateParameter(pair.Key, pair.Value);
                    command.Parameters.Add(param);
                }
            }

            if (whereCondition != null)
            {
                command.CommandText += whereCondition.ToString();

                foreach (KeyValuePair<string, object> pair in whereCondition.GetParameters())
                {
                    IDataParameter param = _databaseEngine.CreateParameter(pair.Key, pair.Value);
                    command.Parameters.Add(param);
                }
            }

            if (parameters != null)
            {
                foreach (IDataParameter param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            return command;
        }

        private object DbParameterValue_Statement(object propertyValue, DatabaseEntityPropertyDef info)
        {
            if (propertyValue == null)
            {
                return DBNull.Value;
            }

            return info.TypeConverter == null ?
                _databaseEngine.GetDbValueStatement(propertyValue, needQuoted: false) :
                info.TypeConverter.TypeValueToDbValue(propertyValue);
        }

        #region 单表查询

        private string GetSelectClauseStatement<T>()
        {
            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();
            string cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_SELECT", modelDef.DatabaseName, modelDef.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1},", modelDef.DbTableReservedName, info.DbReservedName);
                    //argsBuilder.AppendFormat("{0},", info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format(CultureInfo.InvariantCulture, "SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T>(SelectExpression<T>? selectCondition = null, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : Entity, new()
        {
            if (selectCondition == null)
            {
                return AssembleCommand(true, GetSelectClauseStatement<T>(), fromCondition, whereCondition, null);
            }
            else
            {
                return AssembleCommand(true, selectCondition.ToString(), fromCondition, whereCondition, null);
            }
        }

        public IDbCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : Entity, new()
        {
            return AssembleCommand(true, "SELECT COUNT(1) ", fromCondition, whereCondition, null);
        }

        #endregion

        #region 双表查询

        private string GetSelectClauseStatement<T1, T2>()
        {
            DatabaseEntityDef modelDef1 = _entityDefFactory.GetDef<T1>();
            DatabaseEntityDef modelDef2 = _entityDefFactory.GetDef<T2>();

            string cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_SELECT", modelDef1.DatabaseName, modelDef1.TableName, modelDef2.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef1.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1},", modelDef1.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef2.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1},", modelDef2.DbTableReservedName, info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format(CultureInfo.InvariantCulture, "SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : Entity, new()
            where T2 : Entity, new()
        {
            return AssembleCommand(true, GetSelectClauseStatement<T1, T2>(), fromCondition, whereCondition, null);
        }

        #endregion

        #region 三表查询

        private string GetSelectClauseStatement<T1, T2, T3>()
        {
            DatabaseEntityDef modelDef1 = _entityDefFactory.GetDef<T1>();
            DatabaseEntityDef modelDef2 = _entityDefFactory.GetDef<T2>();
            DatabaseEntityDef modelDef3 = _entityDefFactory.GetDef<T3>();

            string cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_SELECT", modelDef1.DatabaseName, modelDef1.TableName, modelDef2.TableName, modelDef3.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef1.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1},", modelDef1.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef2.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1},", modelDef2.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef3.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1},", modelDef3.DbTableReservedName, info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format(CultureInfo.InvariantCulture, "SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : Entity, new()
            where T2 : Entity, new()
            where T3 : Entity, new()
        {
            return AssembleCommand(true, GetSelectClauseStatement<T1, T2, T3>(), fromCondition, whereCondition, null);
        }

        public IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(SelectExpression<TSelect>? selectCondition, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition)
            where TSelect : Entity, new()
            where TFrom : Entity, new()
            where TWhere : Entity, new()
        {
            if (selectCondition == null)
            {
                return AssembleCommand(true, GetSelectClauseStatement<TSelect, TFrom, TWhere>(), fromCondition, whereCondition, null);
            }
            else
            {
                return AssembleCommand(true, selectCondition.ToString(), fromCondition, whereCondition, null);
            }
        }

        #endregion

        #region 单体更改

        public IDbCommand CreateAddCommand<T>(T entity) where T : Entity, new()
        {
            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();
            List<IDataParameter> parameters = new List<IDataParameter>();

            string cacheKey = modelDef.DatabaseName + ":" + modelDef.TableName + ":ADD";

            if (!_sqlStatementDict.TryGetValue(cacheKey, out string addTemplate))
            {
                addTemplate = CreateAddTemplate(modelDef, _databaseEngine.EngineType);
                _sqlStatementDict.TryAdd(cacheKey, addTemplate);
            }

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey)
                    {
                        continue;
                    }

                    //当IsTableProperty为true时，DbParameterizedName一定不为null
                    if (info.PropertyInfo.Name == "Version")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, /*entity.Version + 1*/0, info.DbFieldType));
                    }
                    else if (info.PropertyInfo.Name == "Deleted")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, 0, info.DbFieldType));
                    }
                    else
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, DbParameterValue_Statement(info.PropertyInfo.GetValue(entity), info), info.DbFieldType));
                    }
                }
            }

            return AssembleCommand<T, T>(false, addTemplate, null, null, parameters);
        }

        public IDbCommand CreateAddOrUpdateCommand<T>(T entity) where T : Entity, new()
        {
            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();
            List<IDataParameter> parameters = new List<IDataParameter>();

            string cacheKey = modelDef.DatabaseName + ":" + modelDef.TableName + ":ADDORUPDATE";

            if (!_sqlStatementDict.TryGetValue(cacheKey, out string addOrUpdateTemplate))
            {
                addOrUpdateTemplate = CreateAddOrUpdateTemplate(modelDef, _databaseEngine.EngineType);
                _sqlStatementDict.TryAdd(cacheKey, addOrUpdateTemplate);
            }

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey)
                    {
                        continue;
                    }

                    //当IsTableProperty为true时，DbParameterizedName一定不为null
                    if (info.PropertyInfo.Name == "Version")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, /*entity.Version + 1*/0, info.DbFieldType));
                    }
                    else if (info.PropertyInfo.Name == "Deleted")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, 0, info.DbFieldType));
                    }
                    else
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, DbParameterValue_Statement(info.PropertyInfo.GetValue(entity), info), info.DbFieldType));
                    }
                }
            }

            return AssembleCommand<T, T>(false, addOrUpdateTemplate, null, null, parameters);
        }

        public IDbCommand CreateUpdateCommand<T>(WhereExpression<T> condition, T entity) where T : Entity, new()
        {
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            List<IDataParameter> parameters = new List<IDataParameter>();

            string cacheKey = definition.DatabaseName + ":" + definition.TableName + ":UPDATE";

            if (!_sqlStatementDict.TryGetValue(cacheKey, out string updateTemplate))
            {
                updateTemplate = CreateUpdateTemplate(definition);
                _sqlStatementDict.TryAdd(cacheKey, updateTemplate);
            }

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey || info.PropertyInfo.Name == "Deleted" || info.PropertyInfo.Name == "Guid")
                    {
                        continue;
                    }

                    //当IsTableProperty为true时，DbParameterizedName一定不为null
                    if (info.PropertyInfo.Name == "Version")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, entity.Version + 1, info.DbFieldType));
                    }
                    else
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName!, DbParameterValue_Statement(info.PropertyInfo.GetValue(entity), info), info.DbFieldType));
                    }
                }
            }

            return AssembleCommand<T, T>(false, updateTemplate, null, condition, parameters);
        }

        public IDbCommand CreateDeleteCommand<T>(WhereExpression<T> condition, int currentVersion, string lastUser) where T : Entity, new()
        {
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();

            string cacheKey = definition.DatabaseName + ":" + definition.TableName + ":DELETE";

            if (!_sqlStatementDict.TryGetValue(cacheKey, out string deleteTemplate))
            {
                deleteTemplate = CreateDeleteTemplate(definition);
                _sqlStatementDict.TryAdd(cacheKey, deleteTemplate);
            }

            DatabaseEntityPropertyDef lastUserProperty = definition.GetProperty("LastUser")!;
            DatabaseEntityPropertyDef lastTimeProperty = definition.GetProperty("LastTime")!;
            DatabaseEntityPropertyDef versionProperty = definition.GetProperty("Version")!;

            List<IDataParameter> parameters = new List<IDataParameter>
            {
                _databaseEngine.CreateParameter(versionProperty.DbParameterizedName!, DbParameterValue_Statement(currentVersion+1, versionProperty), versionProperty.DbFieldType),
                _databaseEngine.CreateParameter(lastUserProperty.DbParameterizedName!, DbParameterValue_Statement(lastUser, lastUserProperty), lastUserProperty.DbFieldType),
                _databaseEngine.CreateParameter(lastTimeProperty.DbParameterizedName!, DbParameterValue_Statement(TimeUtil.UtcNow, lastTimeProperty), lastTimeProperty.DbFieldType)
            };

            return AssembleCommand<T, T>(false, deleteTemplate, null, condition, parameters);
        }

        #endregion

        #region Batch

        public IDbCommand CreateBatchAddOrUpdateCommand<T>(IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();
            DatabaseEntityPropertyDef versionPropertyDef = modelDef.GetProperty("Version")!;
            DatabaseEntityPropertyDef guidPropertyDef = modelDef.GetProperty("Guid")!;
            DatabaseEntityPropertyDef idPropertyDef = modelDef.GetProperty("Id")!;

            StringBuilder innerBuilder = new StringBuilder();

            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            IList<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                StringBuilder args = new StringBuilder();
                StringBuilder values = new StringBuilder();
                StringBuilder exceptGuidAndFixedVersionUpdatePairs = new StringBuilder();

                foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
                {
                    string parameterizedName = info.DbParameterizedName + number.ToString(CultureInfo.InvariantCulture);

                    if (info.IsTableProperty)
                    {
                        if (info.IsAutoIncrementPrimaryKey)
                        {
                            continue;
                        }

                        args.AppendFormat(CultureInfo.InvariantCulture, " {0},", info.DbReservedName);

                        if (info.PropertyInfo.Name == "Version")
                        {
                            values.AppendFormat(CultureInfo.InvariantCulture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, /*entity.Version + 1*/0, info.DbFieldType));
                        }
                        else if (info.PropertyInfo.Name == "Deleted")
                        {
                            values.AppendFormat(CultureInfo.InvariantCulture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, 0, info.DbFieldType));
                        }
                        else
                        {
                            values.AppendFormat(CultureInfo.InvariantCulture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, DbParameterValue_Statement(info.PropertyInfo.GetValue(entity), info), info.DbFieldType));
                        }

                        //update pairs
                        if (info.PropertyInfo.Name == "Version" || info.PropertyInfo.Name == "Guid" || info.PropertyInfo.Name == "Deleted")
                        {
                            continue;
                        }

                        exceptGuidAndFixedVersionUpdatePairs.Append($" {info.DbReservedName}={parameterizedName},");
                    }
                }

                exceptGuidAndFixedVersionUpdatePairs.Append($" {versionPropertyDef.DbReservedName}={versionPropertyDef.DbReservedName}+1,");


                if (args.Length > 0)
                {
                    args.Remove(args.Length - 1, 1);
                }

                if (values.Length > 0)
                {
                    values.Remove(values.Length - 1, 1);
                }

                if (exceptGuidAndFixedVersionUpdatePairs.Length > 0)
                {
                    exceptGuidAndFixedVersionUpdatePairs.Remove(exceptGuidAndFixedVersionUpdatePairs.Length - 1, 1);
                }

                innerBuilder.Append($"insert into {modelDef.DbTableReservedName}({args}) values ({values}) {OnDuplicateKeyUpdateStatement(_databaseEngine.EngineType)} {exceptGuidAndFixedVersionUpdatePairs};{TempTable_Insert_Select(tempTableName, _databaseEngine.EngineType)} select {idPropertyDef.DbReservedName}, {versionPropertyDef.DbReservedName} from {modelDef.DbTableReservedName} where {guidPropertyDef.DbReservedName}={guidPropertyDef.DbParameterizedName}{number}; ");

                number++;
            }

            string sql = $"{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{TempTable_Create_IdAndVersion(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{TempTable_Select_IdAndVersion(tempTableName, _databaseEngine.EngineType)}{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return AssembleCommand<T, T>(false, sql, null, null, parameters);
        }

        public IDbCommand CreateBatchAddCommand<T>(IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            IList<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                StringBuilder args = new StringBuilder();
                StringBuilder values = new StringBuilder();

                foreach (DatabaseEntityPropertyDef info in definition.Properties)
                {
                    string parameterizedName = info.DbParameterizedName + number.ToString(CultureInfo.InvariantCulture);

                    if (info.IsTableProperty)
                    {
                        if (info.IsAutoIncrementPrimaryKey)
                        {
                            continue;
                        }

                        args.AppendFormat(CultureInfo.InvariantCulture, " {0},", info.DbReservedName);

                        if (info.PropertyInfo.Name == "Version")
                        {
                            values.AppendFormat(CultureInfo.InvariantCulture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, /*entity.Version + 1*/0, info.DbFieldType));
                        }
                        else if (info.PropertyInfo.Name == "Deleted")
                        {
                            values.AppendFormat(CultureInfo.InvariantCulture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, 0, info.DbFieldType));
                        }
                        else
                        {
                            values.AppendFormat(CultureInfo.InvariantCulture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, DbParameterValue_Statement(info.PropertyInfo.GetValue(entity), info), info.DbFieldType));
                        }
                    }
                }

                if (args.Length > 0)
                {
                    args.Remove(args.Length - 1, 1);
                }

                if (values.Length > 0)
                {
                    values.Remove(values.Length - 1, 1);
                }

                innerBuilder.Append($"insert into {definition.DbTableReservedName}({args}) values ({values});{TempTable_Insert(tempTableName, GetLastInsertIdStatement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string sql = $"{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{TempTable_Create(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{TempTable_Select_All(tempTableName, _databaseEngine.EngineType)}{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return AssembleCommand<T, T>(false, sql, null, null, parameters);
        }

        public IDbCommand CreateBatchUpdateCommand<T>(IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            IList<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                StringBuilder args = new StringBuilder();

                foreach (DatabaseEntityPropertyDef info in definition.Properties)
                {
                    string parameterizedName = info.DbParameterizedName + number.ToString(CultureInfo.InvariantCulture);

                    if (info.IsTableProperty)
                    {
                        if (info.IsAutoIncrementPrimaryKey) continue;

                        if (info.PropertyInfo.Name == "Deleted") continue;

                        if (info.PropertyInfo.Name == "Version")
                        {
                            args.AppendFormat(CultureInfo.InvariantCulture, " {0}={1},", info.DbReservedName, parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, entity.Version + 1, info.DbFieldType));

                        }
                        else
                        {
                            args.AppendFormat(CultureInfo.InvariantCulture, " {0}={1},", info.DbReservedName, parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, DbParameterValue_Statement(info.PropertyInfo.GetValue(entity), info), info.DbFieldType));
                        }
                    }
                }

                if (args.Length > 0)
                    args.Remove(args.Length - 1, 1);

                innerBuilder.Append($"update {definition.DbTableReservedName} set {args} WHERE `Id`={entity.Id} and `Version`={entity.Version} and `Deleted`=0;{TempTable_Insert(tempTableName, FoundChanges_Statement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");

                number++;
            }

            string sql = $"{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{TempTable_Create(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{TempTable_Select_All(tempTableName, _databaseEngine.EngineType)}{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return AssembleCommand<T, T>(false, sql, null, null, parameters);
        }

        public IDbCommand CreateBatchDeleteCommand<T>(IEnumerable<T> entities) where T : Entity, new()
        {
            ThrowIf.Empty(entities, nameof(entities));

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();

            foreach (T entity in entities)
            {
                string lastUserValue = _databaseEngine.GetDbValueStatement(entity.LastUser, needQuoted: true);
                string lastTimeValue = _databaseEngine.GetDbValueStatement(entity.LastTime, true);

                string args = $"`Deleted` = 1, `LastUser` = {lastUserValue}, `LastTime`={lastTimeValue}, `Version` = {entity.Version + 1}";
                innerBuilder.Append(
                    $"UPDATE {definition.DbTableReservedName} set {args} WHERE `Id`={entity.Id} AND `Version`={entity.Version} and `Deleted`=0;{TempTable_Insert(tempTableName, FoundChanges_Statement(_databaseEngine.EngineType), _databaseEngine.EngineType)}");
            }

            string sql = $"{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}{TempTable_Create(tempTableName, _databaseEngine.EngineType)}{innerBuilder}{TempTable_Select_All(tempTableName, _databaseEngine.EngineType)}{TempTable_Drop(tempTableName, _databaseEngine.EngineType)}";

            return AssembleCommand<T, T>(false, sql, null, null, null);
        }

        #endregion

        #region Create Table

        /// <summary>
        /// CreateTableCommand
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addDropStatement"></param>
        /// <returns></returns>

        public IDbCommand CreateTableCommand(Type type, bool addDropStatement)
        {
            string sql = _databaseEngine.EngineType switch
            {
                DatabaseEngineType.MySQL => MySQL_Table_Create_Statement(type, addDropStatement),
                DatabaseEngineType.SQLite => SQLite_Table_Create_Statement(type, addDropStatement),
                _ => string.Empty
            };

            IDbCommand command = _databaseEngine.CreateEmptyCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = sql;

            return command;
        }

        /// <summary>
        /// SQLite_Table_Create_Statement
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addDropStatement"></param>
        /// <returns></returns>

        private string SQLite_Table_Create_Statement(Type type, bool addDropStatement)
        {
            StringBuilder sql = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef(type);

            if (definition.DbTableReservedName.IsNullOrEmpty())
            {
                throw new DatabaseException($"Type : {definition.EntityFullName} has null or empty DbTableReservedName");
            }

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (!info.IsTableProperty)
                {
                    continue;
                }

                if (info.PropertyInfo.Name.IsIn("Id", "Deleted", "LastUser", "LastTime", "Version"))
                {
                    continue;
                }

                string dbTypeStatement = info.TypeConverter == null
                    ? _databaseEngine.GetDbTypeStatement(info.PropertyInfo.PropertyType)
                    : info.TypeConverter.TypeToDbTypeStatement(info.PropertyInfo.PropertyType);

                string nullable = info.IsNullable ? "" : " NOT NULL ";

                string defaultValue = info.DbDefaultValue.IsNullOrEmpty() ? "" : " DEFAULT " + info.DbDefaultValue;

                string unique = info.IsUnique ? " UNIQUE " : "";

                sql.AppendLine($" {info.DbReservedName} {dbTypeStatement} {nullable} {unique} {defaultValue} ,");
            }

            string dropStatement = addDropStatement ? $"Drop table if exists {definition.DbTableReservedName};" : string.Empty;

            return
    $@"{dropStatement}
CREATE TABLE {definition.DbTableReservedName} (
	""Id""    INTEGER PRIMARY KEY AUTOINCREMENT,
	{sql}
	""Deleted""   NUMERIC NOT NULL DEFAULT 0,
	""LastUser"" TEXT,
	""LastTime"" INTEGER NOT NULL,
	""Version"" INTEGER NOT NULL
);";
        }

        /// <summary>
        /// MySQL_Table_Create_Statement
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addDropStatement"></param>
        /// <returns></returns>

        private string MySQL_Table_Create_Statement(Type type, bool addDropStatement)
        {
            StringBuilder sql = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef(type);

            if (definition.DbTableReservedName.IsNullOrEmpty())
            {
                throw new DatabaseException($"Type : {definition.EntityFullName} has null or empty DbTableReservedName");
            }

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (!info.IsTableProperty)
                {
                    continue;
                }

                if (info.PropertyInfo.Name.IsIn("Id", "Deleted", "LastUser", "LastTime", "Version"))
                {
                    continue;
                }

                int length = 0;

                if (info.DbLength == null || info.DbLength == 0)
                {
                    if (info.DbFieldType == DbType.String
                        || info.PropertyInfo.PropertyType == typeof(string)
                        || info.PropertyInfo.PropertyType == typeof(char)
                        || info.PropertyInfo.PropertyType.IsEnum
                        || info.PropertyInfo.PropertyType.IsAssignableFrom(typeof(IList<string>))
                        || info.PropertyInfo.PropertyType.IsAssignableFrom(typeof(IDictionary<string, string>)))
                    {
                        length = _entityDefFactory.GetVarcharDefaultLength();
                    }
                }
                else
                {
                    length = info.DbLength.Value;
                }

                string binary = "";

                if (info.PropertyInfo.PropertyType == typeof(string) || info.PropertyInfo.PropertyType == typeof(char) || info.PropertyInfo.PropertyType == typeof(char?))
                {
                    binary = "";
                }

                string dbTypeStatement = info.TypeConverter == null
                    ? _databaseEngine.GetDbTypeStatement(info.PropertyInfo.PropertyType)
                    : info.TypeConverter.TypeToDbTypeStatement(info.PropertyInfo.PropertyType);

                if (length >= 16383) //因为utf8mb4编码，一个汉字4个字节
                {
                    dbTypeStatement = "MEDIUMTEXT";
                }

                if (length >= 4194303)
                {
                    throw new DatabaseException($"字段长度太长。{info.EntityDef.EntityFullName} : {info.PropertyInfo.Name}");
                }

                if (info.IsLengthFixed)
                {
                    dbTypeStatement = "CHAR";
                }

                sql.AppendFormat(CultureInfo.InvariantCulture, " {0} {1}{2} {6} {3} {4} {5},",
                    info.DbReservedName,
                    dbTypeStatement,
                    (length == 0 || dbTypeStatement == "MEDIUMTEXT") ? "" : "(" + length + ")",
                    info.IsNullable == true ? "" : " NOT NULL ",
                    string.IsNullOrEmpty(info.DbDefaultValue) ? "" : "DEFAULT " + info.DbDefaultValue,
                    !info.IsAutoIncrementPrimaryKey && !info.IsForeignKey && info.IsUnique ? " UNIQUE " : "",
                    binary
                    );
                sql.AppendLine();
            }

            string dropStatement = string.Empty;

            if (addDropStatement)
            {
                dropStatement = string.Format(CultureInfo.InvariantCulture, "Drop table if exists {0};" + Environment.NewLine, definition.DbTableReservedName);
            }

            return string.Format(CultureInfo.InvariantCulture,
                "{2}" +
                "CREATE TABLE {0} (" + Environment.NewLine +
                "`Id` bigint(20) NOT NULL AUTO_INCREMENT," + Environment.NewLine +
                "`Deleted` bit(1) NOT NULL DEFAULT b'0'," + Environment.NewLine +
                "`LastUser` varchar(100) DEFAULT NULL," + Environment.NewLine +
                //"`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," + Environment.NewLine +
                "`LastTime` bigint(20) NOT NULL ," + Environment.NewLine +
                "`Version` int(11) NOT NULL DEFAULT '0'," + Environment.NewLine +
                " {1} " +
                " PRIMARY KEY (`Id`) " + Environment.NewLine +
                " ) ENGINE=InnoDB   DEFAULT CHARSET=utf8mb4;",
                definition.DbTableReservedName, sql.ToString(), dropStatement);
        }

        #endregion

        #region SystemInfo

        private const string _mysql_tbSysInfoCreate =
    @"CREATE TABLE `tb_sys_info` (
	`Id` int (11) NOT NULL AUTO_INCREMENT, 
	`Name` varchar(100) DEFAULT NULL, 
	`Value` varchar(1024) DEFAULT NULL,
	PRIMARY KEY(`Id`),
	UNIQUE KEY `Name_UNIQUE` (`Name`)
);
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('Version', '1');
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('DatabaseName', @databaseName);";

        private const string _mysql_tbSysInfoUpdateVersion = @"UPDATE `tb_sys_info` SET `Value` = @Value WHERE `Name` = 'Version';";

        private const string _mysql_tbSysInfoRetrieve = @"SELECT * FROM `tb_sys_info`;";

        private const string _mysql_isTableExistsStatement = "SELECT count(1) FROM information_schema.TABLES WHERE table_name =@tableName and table_schema=@databaseName;";

        private const string _sqlite_tbSysInfoCreate =
    @"CREATE TABLE ""tb_sys_info"" (
	""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
	""Name"" TEXT UNIQUE, 
	""Value"" TEXT
);
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('Version', '1');
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('DatabaseName', @databaseName);";

        private const string _sqlite_tbSysInfoUpdateVersion = @"UPDATE ""tb_sys_info"" SET ""Value"" = @Value WHERE ""Name"" = 'Version';";

        private const string _sqlite_tbSysInfoRetrieve = @"SELECT * FROM ""tb_sys_info"";";

        private const string _sqlite_isTableExistsStatement = "SELECT count(1) FROM sqlite_master where type='table' and name=@tableName;";

        public IDbCommand CreateIsTableExistCommand(string databaseName, string tableName)
        {
            IDbCommand command = _databaseEngine.CreateEmptyCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = _databaseEngine.EngineType switch
            {
                DatabaseEngineType.MySQL => _mysql_isTableExistsStatement,
                DatabaseEngineType.SQLite => _sqlite_isTableExistsStatement,
                _ => string.Empty
            };
            command.Parameters.Add(_databaseEngine.CreateParameter("@tableName", tableName));
            command.Parameters.Add(_databaseEngine.CreateParameter("@databaseName", databaseName));

            return command;
        }

        public IDbCommand CreateRetrieveSystemInfoCommand()
        {
            IDbCommand command = _databaseEngine.CreateEmptyCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = _databaseEngine.EngineType switch
            {
                DatabaseEngineType.MySQL => _mysql_tbSysInfoRetrieve,
                DatabaseEngineType.SQLite => _sqlite_tbSysInfoRetrieve,
                _ => string.Empty
            };

            return command;
        }

        public IDbCommand CreateUpdateSystemVersionCommand(string databaseName, int version)
        {
            IDbCommand command = _databaseEngine.CreateEmptyCommand();
            command.CommandType = CommandType.Text;

            if (version == 1)
            {
                command.CommandText = _databaseEngine.EngineType switch
                {
                    DatabaseEngineType.MySQL => _mysql_tbSysInfoCreate,
                    DatabaseEngineType.SQLite => _sqlite_tbSysInfoCreate,
                    _ => string.Empty
                };

                command.Parameters.Add(_databaseEngine.CreateParameter("@databaseName", databaseName));
            }
            else
            {
                command.CommandText = _databaseEngine.EngineType switch
                {
                    DatabaseEngineType.MySQL => _mysql_tbSysInfoUpdateVersion,
                    DatabaseEngineType.SQLite => _sqlite_tbSysInfoUpdateVersion,
                    _ => string.Empty
                };

                command.Parameters.Add(_databaseEngine.CreateParameter("@Value", version));
            }

            return command;
        }

        #endregion

        #region Create SelectCondition, FromCondition, WhereCondition

        public SelectExpression<T> NewSelect<T>() where T : Entity, new()
        {
            return new SelectExpression<T>(_databaseEngine, _entityDefFactory);
        }

        public FromExpression<T> NewFrom<T>() where T : Entity, new()
        {
            return new FromExpression<T>(_databaseEngine, _entityDefFactory);
        }

        public WhereExpression<T> NewWhere<T>() where T : Entity, new()
        {
            return new WhereExpression<T>(_databaseEngine, _entityDefFactory);
        }

        #endregion
    }
}
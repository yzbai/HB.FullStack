using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using HB.Framework.Database.Entity;
using HB.Framework.Database.Engine;

namespace HB.Framework.Database.SQL
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
    public class SQLBuilder : ISQLBuilder
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

        private IDbCommand AssembleCommand<TFrom, TWhere>(bool isRetrieve, string selectClause, FromExpression<TFrom> fromCondition, WhereExpression<TWhere> whereCondition, IList<IDataParameter> parameters)
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
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
            }

            if (whereCondition != null)
            {           
                command.CommandText += whereCondition.ToString();

                foreach (KeyValuePair<string, object> pair in whereCondition.Params)
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

        #region 单表查询

        private string GetSelectClauseStatement<T>()
        {
            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();
            string cacheKey = string.Format(GlobalSettings.Culture, "{0}_{1}_SELECT", modelDef.DatabaseName, modelDef.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(GlobalSettings.Culture, "{0}.{1},", modelDef.DbTableReservedName, info.DbReservedName);
                    //argsBuilder.AppendFormat("{0},", info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format(GlobalSettings.Culture, "SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T>(SelectExpression<T> selectCondition=null, FromExpression<T> fromCondition = null, WhereExpression<T> whereCondition = null)
            where T : DatabaseEntity, new()
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

        public IDbCommand CreateCountCommand<T>(FromExpression<T> fromCondition = null, WhereExpression<T> whereCondition = null) 
            where T : DatabaseEntity, new()
        {
            return AssembleCommand(true,  "SELECT COUNT(1) ", fromCondition, whereCondition, null);
        }

        #endregion

        #region 双表查询

        private string GetSelectClauseStatement<T1, T2>()
        {
            DatabaseEntityDef modelDef1 = _entityDefFactory.GetDef<T1>();
            DatabaseEntityDef modelDef2 = _entityDefFactory.GetDef<T2>();

            string cacheKey = string.Format(GlobalSettings.Culture, "{0}_{1}_{2}_SELECT", modelDef1.DatabaseName, modelDef1.TableName, modelDef2.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef1.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(GlobalSettings.Culture, "{0}.{1},", modelDef1.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef2.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(GlobalSettings.Culture, "{0}.{1},", modelDef2.DbTableReservedName, info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format(GlobalSettings.Culture, "SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
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

            string cacheKey = string.Format(GlobalSettings.Culture, "{0}_{1}_{2}_{3}_SELECT", modelDef1.DatabaseName, modelDef1.TableName, modelDef2.TableName, modelDef3.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef1.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(GlobalSettings.Culture, "{0}.{1},", modelDef1.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef2.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(GlobalSettings.Culture, "{0}.{1},", modelDef2.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef3.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat(GlobalSettings.Culture, "{0}.{1},", modelDef3.DbTableReservedName, info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format(GlobalSettings.Culture, "SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
            where T3 : DatabaseEntity, new()
        {
            return AssembleCommand(true, GetSelectClauseStatement<T1, T2, T3>(), fromCondition, whereCondition, null);
        }

        public IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(SelectExpression<TSelect> selectCondition, FromExpression<TFrom> fromCondition, WhereExpression<TWhere> whereCondition)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
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

        #region 增加

        private string GetAddStatement(DatabaseEntityDef definition)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (info.IsTableProperty)
                {
                    selectArgs.AppendFormat(GlobalSettings.Culture, "{0},", info.DbReservedName);

                    if (info.IsAutoIncrementPrimaryKey || info.PropertyName == "LastTime")
                    {
                        continue;
                    }

                    args.AppendFormat(GlobalSettings.Culture, "{0},", info.DbReservedName);
                }
            }

            if (selectArgs.Length > 0)
            {
                selectArgs.Remove(selectArgs.Length - 1, 1);
            }

            if (args.Length > 0)
            {
                args.Remove(args.Length - 1, 1);
            }

            string statement = string.Format(GlobalSettings.Culture,
                "insert into {0}({1}) values({{0}});select {2} from {0} where {3} = last_insert_id();",
                definition.DbTableReservedName, args.ToString(), selectArgs.ToString(), _databaseEngine.GetReservedStatement("Id"));

            return statement;
        }

        public IDbCommand CreateAddCommand<T>(T entity, string lastUser) where T : DatabaseEntity, new()
        {
            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();
            StringBuilder values = new StringBuilder();
            List<IDataParameter> parameters = new List<IDataParameter>();

            #region 获取Add Template

            string cacheKey = modelDef.DatabaseName + ":" + modelDef.TableName + ":ADD";
            string addTemplate;

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                addTemplate = _sqlStatementDict[cacheKey];
            }
            else
            {
                addTemplate = GetAddStatement(modelDef);
                _sqlStatementDict.TryAdd(cacheKey, addTemplate);
            }

            #endregion

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey || info.PropertyName == "LastTime")
                    {
                        continue;
                    }

                    if (info.PropertyName == "Version")
                    {
                        values.AppendFormat(GlobalSettings.Culture, " {0},", info.DbParameterizedName);
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, entity.Version + 1, info.DbFieldType));
                    }
                    else if (info.PropertyName == "Deleted")
                    {
                        values.AppendFormat(GlobalSettings.Culture, " {0},", info.DbParameterizedName);
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, 0, info.DbFieldType));
                    }
                    else if (info.PropertyName == "LastUser")
                    {
                        values.AppendFormat(GlobalSettings.Culture, " {0},", info.DbParameterizedName);
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, lastUser, info.DbFieldType));
                    }
                    else
                    {
                        values.AppendFormat(GlobalSettings.Culture, " {0},", info.DbParameterizedName);

                        string dbValueStatement = info.TypeConverter == null ?
                            _databaseEngine.GetDbValueStatement(info.GetValue(entity), needQuoted: false) :
                            info.TypeConverter.TypeValueToDbValue(info.GetValue(entity));

                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, dbValueStatement, info.DbFieldType));
                    }
                }
            }

            if (values.Length > 0) values.Remove(values.Length - 1, 1);

            string mainClause = string.Format(GlobalSettings.Culture, addTemplate, values.ToString());

            return AssembleCommand<T, T>(false, mainClause, null, null, parameters);
        }

        #endregion

        #region 修改

        private static string GetUpdateStatement(DatabaseEntityDef modelDef)
        {
            StringBuilder args = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey || info.PropertyName == "LastTime" || info.PropertyName == "Deleted")
                    {
                        continue;
                    }

                    args.AppendFormat(GlobalSettings.Culture, " {0}={1},", info.DbReservedName, info.DbParameterizedName);
                }
            }

            if (args.Length > 0)
            {
                args.Remove(args.Length - 1, 1);
            }

            string statement = string.Format(GlobalSettings.Culture, "UPDATE {0} SET {1}", modelDef.DbTableReservedName, args.ToString());

            return statement;
        }

        public IDbCommand CreateUpdateCommand<T>(WhereExpression<T> condition, T entity, string lastUser) where T : DatabaseEntity, new()
        {
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            List<IDataParameter> parameters = new List<IDataParameter>();

            string updateTemplate = GetUpdateStatement(definition);

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsAutoIncrementPrimaryKey || info.PropertyName == "LastTime" || info.PropertyName == "Deleted")
                    {
                        continue;
                    }

                    if (info.PropertyName == "Version")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, entity.Version + 1, info.DbFieldType));
                    }
                    else if (info.PropertyName == "LastUser")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, lastUser, info.DbFieldType));
                    }
                    else
                    {
                        string dbValueStatement = info.TypeConverter == null ? 
                            _databaseEngine.GetDbValueStatement(info.GetValue(entity), needQuoted: false) :
                            info.TypeConverter.TypeValueToDbValue(info.GetValue(entity));

                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, dbValueStatement, info.DbFieldType));
                    }
                }
            }

            return AssembleCommand<T, T>(false, updateTemplate, null, condition, parameters);
        }

        #endregion

        #region Update Key

        private string GetUpdateKeyStatement(DatabaseEntityDef modelDef, string[] keys, object[] values, string lastUser)
        {
            StringBuilder args = new StringBuilder();

            int length = keys.Length;

            for (int i = 0; i < length; i++)
            {
                string key = keys[i];
                DatabaseEntityPropertyDef info = modelDef.GetProperty(key);
                string dbValueStatement = info.TypeConverter == null ? 
                    _databaseEngine.GetDbValueStatement(values[i], needQuoted: true) :
                    _databaseEngine.GetQuotedStatement(info.TypeConverter.TypeValueToDbValue(values[i]));
                args.AppendFormat(GlobalSettings.Culture, " {0}={1},", _databaseEngine.GetReservedStatement(key), dbValueStatement);
            }

            args.AppendFormat(GlobalSettings.Culture, " {0}={1},", _databaseEngine.GetReservedStatement("Version"), _databaseEngine.GetReservedStatement("Version") + " + 1");
            args.AppendFormat(GlobalSettings.Culture, " {0}={1}", _databaseEngine.GetReservedStatement("LastUser"), _databaseEngine.GetDbValueStatement(lastUser, needQuoted: true));

            string statement = string.Format(GlobalSettings.Culture, "UPDATE {0} SET {1} ", modelDef.DbTableReservedName, args.ToString());

            return statement;
        }

        public IDbCommand CreateUpdateKeyCommand<T>(WhereExpression<T> condition, string[] keys, object[] values, string lastUser) where T : DatabaseEntity, new()
        {
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
        
            //List<IDataParameter> parameters = new List<IDataParameter>();

            string updateKeyStatement = GetUpdateKeyStatement(definition, keys, values, lastUser);

            return AssembleCommand<T,T>(false, updateKeyStatement, null, condition, null);
        }

        #endregion

        #region 删除

        public IDbCommand GetDeleteCommand<T>(WhereExpression<T> condition, string lastUser) where T : DatabaseEntity, new()
        {
            return CreateUpdateKeyCommand(condition, new string[] { "Deleted" }, new object[] { 1 }, lastUser);
        }

        #endregion

        #region Batch

        public IDbCommand CreateBatchAddStatement<T>(IEnumerable<T> entities, string lastUser) where T : DatabaseEntity, new()
        {
            if (entities == null || entities.Count() == 0)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            string tempTableName = "A" + DateTimeOffset.UtcNow.Ticks.ToString();

            IList<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                StringBuilder args = new StringBuilder();
                StringBuilder values = new StringBuilder();

                foreach (DatabaseEntityPropertyDef info in definition.Properties)
                {
                    string parameterizedName = info.DbParameterizedName + number.ToString();

                    if (info.IsTableProperty)
                    {
                        if (info.IsAutoIncrementPrimaryKey)
                        {
                            continue;
                        }

                        if (info.PropertyName == "LastTime")
                        {
                            continue;
                        }

                        args.AppendFormat(GlobalSettings.Culture, " {0},", info.DbReservedName);

                        if (info.PropertyName == "Version")
                        {
                            values.AppendFormat(GlobalSettings.Culture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, entity.Version + 1, info.DbFieldType));
                        }
                        else if (info.PropertyName == "Deleted")
                        {
                            values.AppendFormat(GlobalSettings.Culture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, 0, info.DbFieldType));
                        }
                        else if (info.PropertyName == "LastUser")
                        {
                            values.AppendFormat(GlobalSettings.Culture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, lastUser, info.DbFieldType));
                        }
                        else
                        {
                            string dbValueStatement = info.TypeConverter == null ? 
                                _databaseEngine.GetDbValueStatement(info.GetValue(entity), needQuoted: false) : 
                                info.TypeConverter.TypeValueToDbValue(info.GetValue(entity));

                            values.AppendFormat(GlobalSettings.Culture, " {0},", parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, dbValueStatement, info.DbFieldType));
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

                innerBuilder.AppendFormat(GlobalSettings.Culture, "insert into {0}({1}) values ({2});insert into {3}(`id`) values(last_insert_id());",
                    definition.DbTableReservedName, args.ToString(), values.ToString(), tempTableName);

                number++;
            }

            string sql = string.Format(
                GlobalSettings.Culture,
                "drop temporary table if exists `{0}`;create temporary table `{0}` ( `id` int not null);{1}select `id` from `{0}`;drop temporary table `{0}`;",
                tempTableName,
                innerBuilder.ToString());

            return AssembleCommand<T,T>(false, sql, null, null, parameters);
        }

        public IDbCommand CreateBatchUpdateStatement<T>(IEnumerable<T> entities, string lastUser) where T : DatabaseEntity, new()
        {
            if (entities == null || entities.Count() == 0)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            string tempTableName = "U" + DateTimeOffset.UtcNow.Ticks.ToString();
            IList<IDataParameter> parameters = new List<IDataParameter>();
            int number = 0;

            foreach (T entity in entities)
            {
                StringBuilder args = new StringBuilder();
                
                foreach (DatabaseEntityPropertyDef info in definition.Properties)
                {
                    string parameterizedName = info.DbParameterizedName + number.ToString();

                    if (info.IsTableProperty)
                    {
                        if (info.IsAutoIncrementPrimaryKey) continue;

                        if (info.PropertyName == "LastTime") continue;

                        if (info.PropertyName == "Deleted") continue;

                        if (info.PropertyName == "Version")
                        {
                            args.AppendFormat(GlobalSettings.Culture, " {0}={1},", info.DbReservedName, parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, entity.Version + 1, info.DbFieldType));
                            
                        }
                        else if(info.PropertyName == "LastUser")
                        {
                            args.AppendFormat(GlobalSettings.Culture, " {0}={1},", info.DbReservedName, parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, lastUser, info.DbFieldType));
                        }
                        else
                        {
                            string dbValueStatement = info.TypeConverter == null ?
                                _databaseEngine.GetDbValueStatement(info.GetValue(entity), needQuoted: false) :
                                info.TypeConverter.TypeValueToDbValue(info.GetValue(entity));

                            args.AppendFormat(GlobalSettings.Culture, " {0}={1},", info.DbReservedName, parameterizedName);
                            parameters.Add(_databaseEngine.CreateParameter(parameterizedName, dbValueStatement, info.DbFieldType));
                        }
                    }
                }

                if (args.Length > 0)
                    args.Remove(args.Length - 1, 1);

                innerBuilder.AppendFormat(GlobalSettings.Culture,
                    "update {0} set {1} WHERE `Id`={2} and `Version`={3};insert into {4}(`c`) values(found_rows());",
                    definition.DbTableReservedName, args.ToString(), entity.Id, entity.Version, tempTableName);

                number++;
            }


            string sql = string.Format(GlobalSettings.Culture, "drop temporary table if exists `{0}`;create temporary table `{0}`( `c` int not null);{1}select `c` from `{0}`;drop temporary table `{0}`;",
                tempTableName, innerBuilder.ToString());

            return AssembleCommand<T, T>(false, sql, null, null, parameters);
        }       

        public IDbCommand CreateBatchDeleteStatement<T>(IEnumerable<T> entities, string lastUser) where T : DatabaseEntity, new()
        {
            if (entities == null || entities.Count() == 0)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef<T>();
            string tempTableName = "D" + DateTimeOffset.UtcNow.Ticks.ToString();

            foreach (T entity in entities)
            {
                innerBuilder.AppendFormat(GlobalSettings.Culture, "UPDATE {0} set `Deleted` = 1, `LastUser` = {1}, `Version` = {2}  WHERE `Id`={3} AND `Version`={4};insert into {5}(`c`) values(row_count());",
                    definition.DbTableReservedName, _databaseEngine.GetDbValueStatement(lastUser, needQuoted: true), entity.Version + 1, entity.Id, entity.Version, tempTableName);
            }

            string sql = string.Format(GlobalSettings.Culture, "drop temporary table if exists `{0}`;create temporary table `{0}`( `c` int not null);{1}select `c` from `{0}`;drop temporary table `{0}`;",
                tempTableName, innerBuilder.ToString());

            return AssembleCommand<T, T>(false, sql, null, null, null);
        }

        #endregion

        #region Create Table

        //TODO: 目前只适用Mysql，需要后期改造
        //TODO: 处理长文本 Text， MediumText， LongText
        public string GetTableCreateStatement(Type type, bool addDropStatement)
        {
            StringBuilder sql = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.GetDef(type);

            if (definition.DbTableReservedName.IsNullOrEmpty())
            {
                return null;
            }

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (!info.IsTableProperty)
                {
                    continue;
                }

                if (info.PropertyName.IsIn("Id", "Deleted", "LastUser", "LastTime", "Version"))
                {
                    continue;
                }

                int length = 0;

                if (info.DbLength == null || info.DbLength == 0)
                {
                    if (info.DbFieldType == DbType.String
                        || info.PropertyType == typeof(string)
                        || info.PropertyType == typeof(char)
                        || info.PropertyType.IsEnum
                        || info.PropertyType.IsAssignableFrom(typeof(IList<string>))
                        || info.PropertyType.IsAssignableFrom(typeof(IDictionary<string, string>)))
                    {
                        length = _entityDefFactory.GetVarcharDefaultLength();
                    }
                }
                else
                {
                    length = info.DbLength.Value;
                }

                string binary = "";

                if (info.PropertyType == typeof(string) || info.PropertyType == typeof(char) || info.PropertyType == typeof(char?))
                {
                    binary = "";
                }

                string dbTypeStatement = info.TypeConverter == null 
                    ? _databaseEngine.GetDbTypeStatement(info.PropertyType) 
                    : info.TypeConverter.TypeToDbTypeStatement(info.PropertyType);

                sql.AppendFormat(GlobalSettings.Culture, " {0} {1}{2} {6} {3} {4} {5},",
                    info.DbReservedName,
                    info.IsLengthFixed ? "CHAR" : length >= 21845 ? "TEXT" : dbTypeStatement,
                    length == 0 ? "" : "(" + length + ")",
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
                dropStatement = string.Format(GlobalSettings.Culture, "Drop table if exists {0};" + Environment.NewLine, definition.DbTableReservedName);
            }

            return string.Format(GlobalSettings.Culture,
                "{2}" +
                "CREATE TABLE {0} (" + Environment.NewLine +
                "`Id` bigint(20) NOT NULL AUTO_INCREMENT," + Environment.NewLine +
                "`Deleted` bit(1) NOT NULL DEFAULT b'0'," + Environment.NewLine +
                "`LastUser` varchar(100) DEFAULT NULL," + Environment.NewLine +
                "`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," + Environment.NewLine +
                "`Version` bigint(20) NOT NULL DEFAULT '0'," + Environment.NewLine +
                " {1} " + 
                " PRIMARY KEY (`Id`) " + Environment.NewLine +
                " ) ENGINE=InnoDB   DEFAULT CHARSET=utf8mb4;",
                definition.DbTableReservedName, sql.ToString(), dropStatement);
        }

        #endregion              

        #region Create SelectCondition, FromCondition, WhereCondition

        public SelectExpression<T> NewSelect<T>() where T : DatabaseEntity, new()
        {
            return new SelectExpression<T>(_databaseEngine, _entityDefFactory);
        }

        public FromExpression<T> NewFrom<T>() where T : DatabaseEntity, new()
        {
            return new FromExpression<T>(_databaseEngine, _entityDefFactory);
        }

        public WhereExpression<T> NewWhere<T>() where T : DatabaseEntity, new()
        {
            return new WhereExpression<T>(_databaseEngine, _entityDefFactory);
        }

        

        #endregion
    }
}

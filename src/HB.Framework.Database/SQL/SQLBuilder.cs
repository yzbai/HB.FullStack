using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using HB.Framework.Database.Entity;
using HB.Framework.Database.Engine;
using HB.Framework.Common;

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
        private ConcurrentDictionary<string, string> _sqlStatementDict;
        private IDatabaseEntityDefFactory _entityDefFactory;
        private IDatabaseEngine _databaseEngine;

        

        public SQLBuilder(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory entityDefFactory)
        {
            _databaseEngine = databaseEngine;
            _entityDefFactory = entityDefFactory;
            _sqlStatementDict = new ConcurrentDictionary<string, string>();
        }

        private IDbCommand assembleCommand<TFrom, TWhere>(bool isRetrieve, string selectClause, From<TFrom> fromCondition, Where<TWhere> whereCondition, IList<IDataParameter> parameters)
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            IDbCommand command = _databaseEngine.CreateEmptyCommand();

            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = selectClause;

            if (isRetrieve)
            {
                if (fromCondition == null)
                {
                    fromCondition = this.NewFrom<TFrom>();
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

        private string getSelectClauseStatement<T>()
        {
            DatabaseEntityDef modelDef = _entityDefFactory.Get<T>();
            string cacheKey = string.Format("{0}_{1}_SELECT", modelDef.DatabaseName, modelDef.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat("{0}.{1},", modelDef.DbTableReservedName, info.DbReservedName);
                    //argsBuilder.AppendFormat("{0},", info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format("SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T>(Select<T> selectCondition=null, From<T> fromCondition = null, Where<T> whereCondition = null)
            where T : DatabaseEntity, new()
        {
            if (selectCondition == null)
            {
                return assembleCommand(true, getSelectClauseStatement<T>(), fromCondition, whereCondition, null);
            }
            else
            {
                return assembleCommand(true, selectCondition.ToString(), fromCondition, whereCondition, null);
            }
        }

        public IDbCommand CreateCountCommand<T>(From<T> fromCondition = null, Where<T> whereCondition = null) 
            where T : DatabaseEntity, new()
        {
            return assembleCommand(true,  "SELECT COUNT(1) ", fromCondition, whereCondition, null);
        }

        #endregion

        #region 双表查询

        private string getSelectClauseStatement<T1, T2>()
        {
            DatabaseEntityDef modelDef1 = _entityDefFactory.Get<T1>();
            DatabaseEntityDef modelDef2 = _entityDefFactory.Get<T2>();

            string cacheKey = string.Format("{0}_{1}_{2}_SELECT", modelDef1.DatabaseName, modelDef1.TableName, modelDef2.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef1.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat("{0}.{1},", modelDef1.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef2.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat("{0}.{1},", modelDef2.DbTableReservedName, info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format("SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T1, T2>(From<T1> fromCondition, Where<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
        {
            return assembleCommand(true, getSelectClauseStatement<T1, T2>(), fromCondition, whereCondition, null);
        }

        #endregion

        #region 三表查询

        private string getSelectClauseStatement<T1, T2, T3>()
        {
            DatabaseEntityDef modelDef1 = _entityDefFactory.Get<T1>();
            DatabaseEntityDef modelDef2 = _entityDefFactory.Get<T2>();
            DatabaseEntityDef modelDef3 = _entityDefFactory.Get<T3>();

            string cacheKey = string.Format("{0}_{1}_{2}_{3}_SELECT", modelDef1.DatabaseName, modelDef1.TableName, modelDef2.TableName, modelDef3.TableName);

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                return _sqlStatementDict[cacheKey];
            }

            StringBuilder argsBuilder = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef1.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat("{0}.{1},", modelDef1.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef2.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat("{0}.{1},", modelDef2.DbTableReservedName, info.DbReservedName);
                }
            }

            foreach (DatabaseEntityPropertyDef info in modelDef3.Properties)
            {
                if (info.IsTableProperty)
                {
                    argsBuilder.AppendFormat("{0}.{1},", modelDef3.DbTableReservedName, info.DbReservedName);
                }
            }

            if (argsBuilder.Length > 0)
            {
                argsBuilder.Remove(argsBuilder.Length - 1, 1);
            }

            string selectClause = string.Format("SELECT {0} ", argsBuilder.ToString());

            _sqlStatementDict.TryAdd(cacheKey, selectClause);

            return selectClause;
        }

        public IDbCommand CreateRetrieveCommand<T1, T2, T3>(From<T1> fromCondition, Where<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
            where T3 : DatabaseEntity, new()
        {
            return assembleCommand(true, getSelectClauseStatement<T1, T2, T3>(), fromCondition, whereCondition, null);
        }

        public IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(Select<TSelect> selectCondition, From<TFrom> fromCondition, Where<TWhere> whereCondition)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            if (selectCondition == null)
            {
                return assembleCommand(true, getSelectClauseStatement<TSelect, TFrom, TWhere>(), fromCondition, whereCondition, null);
            }
            else
            {
                return assembleCommand(true, selectCondition.ToString(), fromCondition, whereCondition, null);
            }
        }

        #endregion

        #region 增加

        private string getAddStatement(DatabaseEntityDef definition)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (info.IsTableProperty)
                {
                    selectArgs.AppendFormat("{0},", info.DbReservedName);

                    if (info.AutoIncrement || info.IsPrimaryKey || info.PropertyName == "LastTime")
                    {
                        continue;
                    }

                    args.AppendFormat("{0},", info.DbReservedName);
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

            string statement = string.Format(
                "insert into {0}({1}) values({{0}});select {2} from {0} where {3} = last_insert_id();",
                definition.DbTableReservedName, args.ToString(), selectArgs.ToString(), _databaseEngine.GetReservedStatement("Id"));

            return statement;
        }

        public IDbCommand CreateAddCommand<T>(T domain, string lastUser) where T : DatabaseEntity, new()
        {
            DatabaseEntityDef modelDef = _entityDefFactory.Get<T>();
            StringBuilder values = new StringBuilder();
            List<IDataParameter> parameters = new List<IDataParameter>();

            #region 获取Add Template

            string cacheKey = modelDef.DatabaseName + ":" + modelDef.TableName + ":ADD";
            string addTemplate = string.Empty;

            if (_sqlStatementDict.ContainsKey(cacheKey))
            {
                addTemplate = _sqlStatementDict[cacheKey];
            }
            else
            {
                addTemplate = getAddStatement(modelDef);
                _sqlStatementDict.TryAdd(cacheKey, addTemplate);
            }

            #endregion

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.AutoIncrement || info.IsPrimaryKey || info.PropertyName == "LastTime")
                    {
                        continue;
                    }

                    if (info.PropertyName == "Version")
                    {
                        values.AppendFormat(" {0},", info.DbParameterizedName);
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, domain.Version + 1, info.DbFieldType));
                    }
                    else if (info.PropertyName == "Deleted")
                    {
                        values.AppendFormat(" {0},", info.DbParameterizedName);
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, 0, info.DbFieldType));
                    }
                    else if (info.PropertyName == "LastUser")
                    {
                        values.AppendFormat(" {0},", info.DbParameterizedName);
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, lastUser, info.DbFieldType));
                    }
                    else
                    {
                        values.AppendFormat(" {0},", info.DbParameterizedName);
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, _databaseEngine.GetDbValueStatement(info.GetValue(domain), needQuoted: false), info.DbFieldType));
                    }
                }
            }

            if (values.Length > 0) values.Remove(values.Length - 1, 1);

            string mainClause = string.Format(addTemplate, values.ToString());

            return assembleCommand<T, T>(false, mainClause, null, null, parameters);
        }

        #endregion

        #region 修改

        private string getUpdateStatement(DatabaseEntityDef modelDef)
        {
            StringBuilder args = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in modelDef.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsPrimaryKey || info.AutoIncrement || info.PropertyName == "LastTime" || info.PropertyName == "Deleted")
                    {
                        continue;
                    }

                    args.AppendFormat(" {0}={1},", info.DbReservedName, info.DbParameterizedName);
                }
            }

            if (args.Length > 0)
            {
                args.Remove(args.Length - 1, 1);
            }

            string statement = string.Format("UPDATE {0} SET {1}", modelDef.DbTableReservedName, args.ToString());

            return statement;
        }

        public IDbCommand CreateUpdateCommand<T>(Where<T> condition, T domain, string lastUser) where T : DatabaseEntity, new()
        {
            DatabaseEntityDef definition = _entityDefFactory.Get<T>();
            List<IDataParameter> parameters = new List<IDataParameter>();

            string updateTemplate = getUpdateStatement(definition);

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (info.IsTableProperty)
                {
                    if (info.IsPrimaryKey || info.AutoIncrement || info.PropertyName == "LastTime" || info.PropertyName == "Deleted")
                    {
                        continue;
                    }

                    if (info.PropertyName == "Version")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, domain.Version + 1, info.DbFieldType));
                    }
                    else if (info.PropertyName == "LastUser")
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, lastUser, info.DbFieldType));
                    }
                    else
                    {
                        parameters.Add(_databaseEngine.CreateParameter(info.DbParameterizedName, _databaseEngine.GetDbValueStatement(info.GetValue(domain), needQuoted: false), info.DbFieldType));
                    }
                }
            }

            return assembleCommand<T, T>(false, updateTemplate, null, condition, parameters);
        }

        #endregion

        #region Update Key

        private string getUpdateKeyStatement(DatabaseEntityDef modelDef, string[] keys, object[] values, string lastUser)
        {
            StringBuilder args = new StringBuilder();

            int length = keys.Length;

            for (int i = 0; i < length; i++)
            {
                args.AppendFormat(" {0}={1},", _databaseEngine.GetReservedStatement(keys[i]), _databaseEngine.GetDbValueStatement(values[i], needQuoted: true));
            }

            args.AppendFormat(" {0}={1},", _databaseEngine.GetReservedStatement("Version"), _databaseEngine.GetReservedStatement("Version") + " + 1");
            args.AppendFormat(" {0}={1}", _databaseEngine.GetReservedStatement("LastUser"), _databaseEngine.GetDbValueStatement(lastUser, needQuoted: true));

            string statement = string.Format("UPDATE {0} SET {1} ", modelDef.DbTableReservedName, args.ToString());

            return statement;
        }

        public IDbCommand CreateUpdateKeyCommand<T>(Where<T> condition, string[] keys, object[] values, string lastUser) where T : DatabaseEntity, new()
        {
            DatabaseEntityDef definition = _entityDefFactory.Get<T>();
        
            List<IDataParameter> parameters = new List<IDataParameter>();

            string updateKeyStatement = getUpdateKeyStatement(definition, keys, values, lastUser);

            return assembleCommand<T,T>(false, updateKeyStatement, null, condition, null);
        }

        #endregion

        #region 删除

        public IDbCommand GetDeleteCommand<T>(Where<T> condition, string lastUser) where T : DatabaseEntity, new()
        {
            return CreateUpdateKeyCommand<T>(condition, new string[] { "Deleted" }, new object[] { 1 }, lastUser);
        }

        #endregion

        #region Batch

        public string GetBatchAddStatement<T>(IList<T> domains, string lastUser) where T : DatabaseEntity
        {
            if (domains == null || domains.Count == 0)
            {
                throw new ArgumentNullException(nameof(domains));
            }

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.Get<T>();
            StringBuilder args = null;
            StringBuilder values = null;

            foreach (T domain in domains)
            {
                args = new StringBuilder();
                values = new StringBuilder();

                foreach (DatabaseEntityPropertyDef info in definition.Properties)
                {
                    if (info.IsTableProperty)
                    {
                        if (info.AutoIncrement || info.IsPrimaryKey)
                        {
                            continue;
                        }

                        if (info.PropertyName == "LastTime")
                        {
                            continue;
                        }

                        args.AppendFormat(" {0},", info.DbReservedName);

                        if (info.PropertyName == "Version")
                        {
                            values.AppendFormat(" {0},", domain.Version + 1);
                        }
                        else if (info.PropertyName == "Deleted")
                        {
                            values.AppendFormat(" {0},", 0);
                        }
                        else if (info.PropertyName == "LastUser")
                        {
                            values.AppendFormat(" {0},", _databaseEngine.GetDbValueStatement(lastUser, needQuoted: true));
                        }
                        else
                        {
                            values.AppendFormat(" {0},", _databaseEngine.GetDbValueStatement(info.GetValue(domain), needQuoted: true));
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

                innerBuilder.AppendFormat("insert into {0}({1}) values ({2});insert into tb_tmp_batchAddID(id) values(last_insert_id());",
                    definition.DbTableReservedName, args.ToString(), values.ToString());
            }

            return string.Format("drop temporary table if exists `tb_tmp_batchAddID`;create temporary table `tb_tmp_batchAddID` ( `id` int not null);start transaction;{0}commit;select `id` from `tb_tmp_batchAddID`;drop temporary table `tb_tmp_batchAddID`;",
                innerBuilder.ToString());
        }

        public string GetBatchUpdateStatement<T>(IList<T> entities, string lastUser) where T : DatabaseEntity
        {
            if (entities == null || entities.Count == 0)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.Get<T>();
            StringBuilder args = null;

            foreach (T entity in entities)
            {
                args = new StringBuilder();

                foreach (DatabaseEntityPropertyDef info in definition.Properties)
                {
                    if (info.IsTableProperty)
                    {
                        if (info.IsPrimaryKey) continue;

                        if (info.AutoIncrement) continue;

                        if (info.PropertyName == "LastTime") continue;

                        if (info.PropertyName == "Deleted") continue;

                        if (info.PropertyName == "Version")
                        {
                            args.AppendFormat(" {0}={1},", info.DbReservedName, entity.Version + 1);
                            
                        }
                        else if(info.PropertyName == "LastUser")
                        {
                            args.AppendFormat(" {0}={1},", info.DbReservedName, _databaseEngine.GetDbValueStatement(lastUser, needQuoted: true));
                        }
                        else
                        {
                            args.AppendFormat(" {0}={1},", info.DbReservedName, _databaseEngine.GetDbValueStatement(info.GetValue(entity), needQuoted: true));
                        }
                    }
                }

                if (args.Length > 0)
                    args.Remove(args.Length - 1, 1);

                innerBuilder.AppendFormat(
                    "update {0} set {1} WHERE `Id`={2} and `Version`={3};insert into tb_tmp_batchUpdateCount(AffectedRowCount) values(row_count());",
                    definition.DbTableReservedName, args.ToString(), entity.Id, entity.Version);
            }


            return string.Format("drop temporary table if exists `tb_tmp_batchUpdateCount`;create temporary table `tb_tmp_batchUpdateCount`( `AffectedRowCount` int not null);start transaction;{0}commit;select `AffectedRowCount` from `tb_tmp_batchUpdateCount`;drop temporary table `tb_tmp_batchUpdateCount`;",
                innerBuilder.ToString());
        }       

        public string GetBatchDeleteStatement<T>(IList<T> domains, string lastUser) where T : DatabaseEntity
        {
            if (domains == null || domains.Count == 0)
            {
                throw new ArgumentNullException(nameof(domains));
            }

            StringBuilder innerBuilder = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.Get<T>();

            foreach (T domain in domains)
            {
                innerBuilder.AppendFormat("UPDATE {0} set `Deleted` = 1, `LastUser` = {1}, `Version` = {2}  WHERE `Id`={3} AND `Version`={4};insert into tb_tmp_batchDeleteCount(AffectedRowCount) values(row_count());",
                    definition.DbTableReservedName, _databaseEngine.GetDbValueStatement(lastUser, needQuoted: true), domain.Version + 1, domain.Id, domain.Version);
            }

            return string.Format("drop temporary table if exists `tb_tmp_batchDeleteCount`;create temporary table `tb_tmp_batchDeleteCount`( `AffectedRowCount` int not null);start transaction;{0}commit;select `AffectedRowCount` from `tb_tmp_batchDeleteCount`;drop temporary table `tb_tmp_batchDeleteCount`;",
                innerBuilder.ToString());
        }

        #endregion

        #region Create

        //TODO: 目前只适用Mysql，需要后期改造
        public string GetCreateStatement(Type type, bool addDropStatement)
        {
            StringBuilder sql = new StringBuilder();
            DatabaseEntityDef definition = _entityDefFactory.Get(type);

            foreach (DatabaseEntityPropertyDef info in definition.Properties)
            {
                if (!info.IsTableProperty)
                {
                    continue;
                }

                if (info.PropertyName.IsIn( "Id", "Deleted", "LastUser", "LastTime", "Version" ))
                {
                    continue;
                }

                int length = 0;

                if (info.DbLength == null || info.DbLength == 0)
                {
                    if (info.PropertyType == typeof(string) || info.PropertyType == typeof(char))
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

                sql.AppendFormat(" {0} {1}{2} {6} {3} {4} {5},",
                    info.DbReservedName,
                    length >= 21845 ? "TEXT" : _databaseEngine.GetDbTypeStatement(info.PropertyType),
                    length == 0 ? "" : "(" + length + ")",
                    info.IsNullable == true ? "" : " NOT NULL ",
                    string.IsNullOrEmpty(info.DbDefaultValue) ? "" : "DEFAULT " + info.DbDefaultValue,
                    info.IsUnique ? " UNIQUE " : "",
                    binary
                    );
                sql.AppendLine();              
            }

            string dropStatement = string.Empty;

            if (addDropStatement)
            {
                dropStatement = string.Format("Drop table {0};" + Environment.NewLine, definition.DbTableReservedName);
            }

            return string.Format(
                "{2}" +
                "CREATE TABLE {0} (" + Environment.NewLine +
                "`Id` bigint(20) NOT NULL AUTO_INCREMENT," + Environment.NewLine +
                "`Deleted` bit(1) NOT NULL DEFAULT b'0'," + Environment.NewLine +
                "`LastUser` varchar(100) DEFAULT NULL," + Environment.NewLine +
                "`LastTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," + Environment.NewLine +
                "`Version` bigint(20) NOT NULL DEFAULT '0'," + Environment.NewLine +
                " {1} " + 
                " PRIMARY KEY (`Id`) " + Environment.NewLine +
                " ) ENGINE=InnoDB   DEFAULT CHARSET=utf8;",
                definition.DbTableReservedName, sql.ToString(), dropStatement);
        }

        #endregion              

        #region Create SelectCondition, FromCondition, WhereCondition

        public Select<T> NewSelect<T>() where T : DatabaseEntity, new()
        {
            return new Select<T>(_databaseEngine, _entityDefFactory);
        }

        public From<T> NewFrom<T>() where T : DatabaseEntity, new()
        {
            return new From<T>(_databaseEngine, _entityDefFactory);
        }

        public Where<T> NewWhere<T>() where T : DatabaseEntity, new()
        {
            return new Where<T>(_databaseEngine, _entityDefFactory);
        }

        

        #endregion
    }
}

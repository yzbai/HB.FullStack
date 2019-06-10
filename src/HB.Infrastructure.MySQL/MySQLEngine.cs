using HB.Framework.Database.Engine;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;

namespace HB.Infrastructure.MySQL
{
    /// <summary>
    /// MySql数据库
    /// </summary>
    internal partial class MySQLEngine : IDatabaseEngine
    {
        #region 自身 & 构建

        private readonly MySQLEngineOptions _options;
        private Dictionary<string, string> _connectionStringDict;

        private MySQLEngine() { }

        public MySQLEngine(ILoggerFactory loggerFactory, IOptions<MySQLEngineOptions> optionsAccessor) : this()
        {
            MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);

            _options = optionsAccessor.Value;

            SetConnectionStrings();
        }

        public MySQLEngineOptions Options { get { return _options; } }

        private void SetConnectionStrings()
        {
            _connectionStringDict = new Dictionary<string, string>();

            foreach (MySQLDatabaseSetting ds in _options.DatabaseSettings)
            {
                if (ds.IsMaster)
                {
                    _connectionStringDict[ds.DatabaseName + "_1"] = ds.ConnectionString;

                    if (!_connectionStringDict.ContainsKey(ds.DatabaseName + "_0"))
                    {
                        _connectionStringDict[ds.DatabaseName + "_0"] = ds.ConnectionString;
                    }
                }
                else
                {
                    _connectionStringDict[ds.DatabaseName + "_0"] = ds.ConnectionString;
                }
            }
        }

        private string GetConnectionString(string dbName, bool isMaster)
        {
            if (isMaster)
            {
                return _connectionStringDict[dbName + "_1"];
            }

            return _connectionStringDict[dbName + "_0"];
        }

        #endregion

        #region SP执行功能
        
        /// <summary>
        /// 使用完毕后必须Dispose
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        public IDataReader ExecuteSPReader(IDbTransaction Transaction, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPReader(GetConnectionString(dbName, useMaster), spName, dbParameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPReader((MySqlTransaction)Transaction, spName, dbParameters);
            }
        }

        public object ExecuteSPScalar(IDbTransaction Transaction, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPScalar(GetConnectionString(dbName, useMaster), spName, parameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPScalar((MySqlTransaction)Transaction, spName, parameters);
            }
        }

        public int ExecuteSPNonQuery(IDbTransaction Transaction, string dbName, string spName, IList<IDataParameter> parameters)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPNonQuery(GetConnectionString(dbName, true), spName, parameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPNonQuery((MySqlTransaction)Transaction, spName, parameters);
            }
        }

        #endregion

        #region Command执行功能

        public int ExecuteCommandNonQuery(IDbTransaction Transaction, string dbName, IDbCommand dbCommand)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandNonQuery(GetConnectionString(dbName, true), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandNonQuery((MySqlTransaction)Transaction, dbCommand);
            }
        }
        
        /// <summary>
        /// 使用完毕后必须Dispose，必须使用using
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        public IDataReader ExecuteCommandReader(IDbTransaction Transaction, string dbName, IDbCommand dbCommand, bool useMaster)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandReader(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandReader((MySqlTransaction)Transaction, dbCommand);
            }
        }

        public object ExecuteCommandScalar(IDbTransaction Transaction, string dbName, IDbCommand dbCommand, bool useMaster)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandScalar(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandScalar((MySqlTransaction)Transaction, dbCommand);
            }
        }

        #endregion

        #region 创建功能

        public IDataParameter CreateParameter(string name, object value, DbType dbType)
        {
            MySqlParameter parameter = new MySqlParameter
            {
                ParameterName = name,
                Value = value,
                DbType = dbType
            };
            return parameter;
        }

        public IDataParameter CreateParameter(string name, object value)
        {
            MySqlParameter parameter = new MySqlParameter
            {
                ParameterName = name,
                Value = value
            };
            return parameter;
        }

        public IDbCommand CreateEmptyCommand()
        {
            MySqlCommand command = new MySqlCommand();
            return command;
        }

        #endregion

        #region 方言
        
        public string ParameterizedChar { get { return MySQLUtility.ParameterizedChar; } }

        public string QuotedChar { get { return MySQLUtility.QuotedChar; } }

        public string ReservedChar { get { return MySQLUtility.ReservedChar; } }

        public string GetQuotedStatement(string name)
        {
            return MySQLUtility.GetQuoted(name);
        }

        public string GetParameterizedStatement(string name)
        {
            return MySQLUtility.GetParameterized(name);
        }

        public string GetReservedStatement(string name)
        {
            return MySQLUtility.GetReserved(name);
        }

        public DbType GetDbType(Type type)
        {
            return MySQLUtility.GetDbType(type);
        }

        public string GetDbTypeStatement(Type type)
        {
            return MySQLUtility.GetDbTypeStatement(type);
        }

        public string GetDbValueStatement(object value, bool needQuoted)
        {
            return MySQLUtility.GetDbValueStatement(value, needQuoted);
        }

        public bool IsValueNeedQuoted(Type type)
        {
            return MySQLUtility.IsValueNeedQuoted(type);
        }

        #endregion

        #region 事务

        public IDbTransaction BeginTransaction(string dbName, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            MySqlConnection conn = new MySqlConnection(GetConnectionString(dbName, true));
            conn.Open();

            return conn.BeginTransaction(isolationLevel);
        }

        public void Commit(IDbTransaction transaction)
        {
            transaction.Commit();
        }

        public void Rollback(IDbTransaction transaction)
        {
            transaction.Rollback();
        }

        #endregion
    }
}
//public DataTable CreateEmptyDataTable(string dbName, string tableName)
//{
//    return MySQLTableCache.CreateEmptyDataTable(GetConnectionString(dbName, true), tableName);
//}
//#region SQL执行功能-Unsafe

///// <summary>
///// 使用后必须Dispose，必须使用using. 在MySql中，IDataReader.Close工作不正常。解决之前不要用
///// </summary>
//public IDataReader ExecuteSqlReader(IDbTransaction Transaction, string dbName, string SQL, bool useMaster)
//{

//    if (Transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlReader(GetConnectionString(dbName, useMaster), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlReader((MySqlTransaction)Transaction, SQL);
//    }
//}

//public object ExecuteSqlScalar(IDbTransaction Transaction, string dbName, string SQL, bool useMaster)
//{
//    if (Transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlScalar(GetConnectionString(dbName, useMaster), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlScalar((MySqlTransaction)Transaction, SQL);
//    }
//}

//public int ExecuteSqlNonQuery(IDbTransaction Transaction, string dbName, string SQL)
//{
//    if (Transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlNonQuery(GetConnectionString(dbName, true), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlNonQuery((MySqlTransaction)Transaction, SQL);
//    }
//}

//public DataTable ExecuteSqlDataTable(IDbTransaction transaction, string dbName, string SQL)
//{
//    if (transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlDataTable(GetConnectionString(dbName, true), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlDataTable((MySqlTransaction)transaction, SQL);
//    }
//}

//#endregion

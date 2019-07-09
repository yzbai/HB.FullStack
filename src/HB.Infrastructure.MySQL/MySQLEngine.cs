using HB.Framework.Database.Engine;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;
using HB.Framework.Database;
using System.Linq;

namespace HB.Infrastructure.MySQL
{
    /// <summary>
    /// MySql数据库
    /// </summary>
    internal partial class MySQLEngine : IDatabaseEngine
    {
        #region 自身 & 构建

        private readonly MySQLOptions _options;
        private Dictionary<string, string> _connectionStringDict;

        public IDatabaseSettings DatabaseSettings => _options.DatabaseSettings;

        public DatabaseEngineType EngineType => DatabaseEngineType.MySQL;

        public string FirstDefaultDatabaseName { get; private set; }

        private MySQLEngine() { }

        public MySQLEngine(MySQLOptions options) : this()
        {
            //MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);

            _options = options;

            SetConnectionStrings();
        }

        private void SetConnectionStrings()
        {
            _connectionStringDict = new Dictionary<string, string>();

            foreach (SchemaInfo schemaInfo in _options.Schemas)
            {
                if (FirstDefaultDatabaseName.IsNullOrEmpty())
                {
                    FirstDefaultDatabaseName = schemaInfo.SchemaName;
                }

                if (schemaInfo.IsMaster)
                {
                    _connectionStringDict[schemaInfo.SchemaName + "_1"] = schemaInfo.ConnectionString;

                    if (!_connectionStringDict.ContainsKey(schemaInfo.SchemaName + "_0"))
                    {
                        _connectionStringDict[schemaInfo.SchemaName + "_0"] = schemaInfo.ConnectionString;
                    }
                }
                else
                {
                    _connectionStringDict[schemaInfo.SchemaName + "_0"] = schemaInfo.ConnectionString;
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

        #region SQL 执行能力

        public int ExecuteSqlNonQuery(IDbTransaction Transaction, string dbName, string SQL)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSqlNonQuery(GetConnectionString(dbName, true), SQL);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlNonQuery((MySqlTransaction)Transaction, SQL);
            }
        }

        /// <summary>
        /// 使用后必须Dispose，必须使用using.
        /// </summary>
        public IDataReader ExecuteSqlReader(IDbTransaction Transaction, string dbName, string SQL, bool useMaster)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSqlReader(GetConnectionString(dbName, useMaster), SQL);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlReader((MySqlTransaction)Transaction, SQL);
            }
        }

        public object ExecuteSqlScalar(IDbTransaction Transaction, string dbName, string SQL, bool useMaster)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSqlScalar(GetConnectionString(dbName, useMaster), SQL);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlScalar((MySqlTransaction)Transaction, SQL);
            }
        }

        #endregion

        #region 创建功能

        public IDataParameter CreateParameter(string name, object value, DbType dbType)
        {
            MySqlParameter parameter = new MySqlParameter {
                ParameterName = name,
                Value = value,
                DbType = dbType
            };
            return parameter;
        }

        public IDataParameter CreateParameter(string name, object value)
        {
            MySqlParameter parameter = new MySqlParameter {
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

        #region SystemInfo

        private static string tbSysInfoCreate =
@"CREATE TABLE `tb_sys_info` (
	`Id` int (11) NOT NULL AUTO_INCREMENT, 
	`Name` varchar(100) DEFAULT NULL, 
	`Value` varchar(1024) DEFAULT NULL,
	PRIMARY KEY(`Id`),
	UNIQUE KEY `Name_UNIQUE` (`Name`)
);
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('Version', '1');
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('DatabaseName', '{0}');";

        private static string tbSysInfoRetrieve = @"SELECT * FROM `tb_sys_info`;";

        private static string tbSysInfoUpdateVersion = @"UPDATE `tb_sys_info` SET `Value` = '{0}' WHERE `Name` = 'Version';";

        public IEnumerable<string> GetDatabaseNames()
        {
            return _options.Schemas.Select(s => s.SchemaName);
        }

        public SystemInfo GetSystemInfo(string databaseName, IDbTransaction transaction)
        {
            IDataReader reader = null;

            try
            {
                reader = ExecuteSqlReader(transaction, databaseName, tbSysInfoRetrieve, false);

                SystemInfo systemInfo = new SystemInfo { DatabaseName = databaseName };

                while (reader.Read())
                {
                    systemInfo.Add(reader["Name"].ToString(), reader["Value"].ToString());
                }

                return systemInfo;
            }
            catch (Exception)
            {
                return new SystemInfo 
                {
                    DatabaseName = databaseName,
                    Version = 0
                };
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        public void UpdateSystemVersion(string databaseName, int version, IDbTransaction transaction)
        {
            if (version == 1)
            {
                //创建SystemInfo
                ExecuteSqlNonQuery(transaction, databaseName, string.Format(tbSysInfoCreate, databaseName));
            }
            else
            {
                ExecuteSqlNonQuery(transaction, databaseName, string.Format(tbSysInfoUpdateVersion, version));
            }
        }

        #endregion
    }
}
//public DataTable CreateEmptyDataTable(string dbName, string tableName)
//{
//    return MySQLTableCache.CreateEmptyDataTable(GetConnectionString(dbName, true), tableName);
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

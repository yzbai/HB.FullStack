using HB.FullStack.Database;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MySqlConnector;
using MySqlConnector.Logging;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Infrastructure.MySQL
{
    /// <summary>
    /// MySql数据库
    /// </summary>
    internal class MySQLEngine : IDatabaseEngine
    {
        #region 自身 & 构建

        private readonly MySQLOptions _options;

        private readonly ILogger _logger;

        private readonly Dictionary<string, string> _connectionStringDict = new Dictionary<string, string>();

        public DatabaseCommonSettings DatabaseSettings => _options.CommonSettings;

        public DatabaseEngineType EngineType => DatabaseEngineType.MySQL;

        [NotNull, DisallowNull] public string? FirstDefaultDatabaseName { get; private set; }

        public MySQLEngine(IOptions<MySQLOptions> options, ILoggerFactory loggerFactory, ILogger<MySQLEngine> logger)
        {
            try
            {
                MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);
            }
            catch (InvalidOperationException ex)
            {
                GlobalSettings.Logger.LogError(ex, $"Connections:{SerializeUtil.ToJson(options.Value.Connections)}");
            }

            _options = options.Value;
            _logger = logger;

            SetConnectionStrings();

            _logger.LogInformation($"MySQLEngine初始化完成");
        }

        private void SetConnectionStrings()
        {
            foreach (DatabaseConnectionSettings connection in _options.Connections)
            {
                if (FirstDefaultDatabaseName.IsNullOrEmpty())
                {
                    FirstDefaultDatabaseName = connection.DatabaseName;
                }

                if (connection.IsMaster)
                {
                    _connectionStringDict[connection.DatabaseName + "_1"] = connection.ConnectionString;

                    if (!_connectionStringDict.ContainsKey(connection.DatabaseName + "_0"))
                    {
                        _connectionStringDict[connection.DatabaseName + "_0"] = connection.ConnectionString;
                    }
                }
                else
                {
                    _connectionStringDict[connection.DatabaseName + "_0"] = connection.ConnectionString;
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

        public IEnumerable<string> GetDatabaseNames()
        {
            return _options.Connections.Select(s => s.DatabaseName);
        }

        #endregion 自身 & 构建

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

        #endregion 创建功能

        #region 方言

        public string ParameterizedChar { get { return MySQLLocalism.ParameterizedChar; } }

        public string QuotedChar { get { return MySQLLocalism.QuotedChar; } }

        public string ReservedChar { get { return MySQLLocalism.ReservedChar; } }

        public string GetQuotedStatement(string name)
        {
            return MySQLLocalism.GetQuoted(name);
        }

        public string GetParameterizedStatement(string name)
        {
            return MySQLLocalism.GetParameterized(name);
        }

        public string GetReservedStatement(string name)
        {
            return MySQLLocalism.GetReserved(name);
        }

        public DbType GetDbType(Type type)
        {
            return MySQLLocalism.GetDbType(type);
        }

        public string GetDbTypeStatement(Type type)
        {
            return MySQLLocalism.GetDbTypeStatement(type);
        }

        [return: NotNullIfNotNull("value")]
        public string? GetDbValueStatement(object? value, bool needQuoted)
        {
            return MySQLLocalism.GetDbValueStatement(value, needQuoted);
        }

        public bool IsValueNeedQuoted(Type type)
        {
            return MySQLLocalism.IsValueNeedQuoted(type);
        }

        #endregion 方言

        #region SP 能力

        /// <summary>
        /// ExecuteSPReaderAsync
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbName"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <param name="useMaster"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<Tuple<IDbCommand, IDataReader>> ExecuteSPReaderAsync(IDbTransaction? Transaction, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPReaderAsync(GetConnectionString(dbName, useMaster), spName, dbParameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPReaderAsync((MySqlTransaction)Transaction, spName, dbParameters);
            }
        }

        /// <summary>
        /// ExecuteSPScalarAsync
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbName"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <param name="useMaster"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<object> ExecuteSPScalarAsync(IDbTransaction? Transaction, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPScalarAsync(GetConnectionString(dbName, useMaster), spName, parameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPScalarAsync((MySqlTransaction)Transaction, spName, parameters);
            }
        }

        /// <summary>
        /// ExecuteSPNonQueryAsync
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbName"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<int> ExecuteSPNonQueryAsync(IDbTransaction? Transaction, string dbName, string spName, IList<IDataParameter> parameters)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPNonQueryAsync(GetConnectionString(dbName, true), spName, parameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPNonQueryAsync((MySqlTransaction)Transaction, spName, parameters);
            }
        }

        #endregion SP 能力

        #region Command 能力

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbName"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<int> ExecuteCommandNonQueryAsync(IDbTransaction? Transaction, string dbName, IDbCommand dbCommand)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandNonQueryAsync(GetConnectionString(dbName, true), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandNonQueryAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbName"></param>
        /// <param name="dbCommand"></param>
        /// <param name="useMaster"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction? Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandReaderAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandReaderAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbName"></param>
        /// <param name="dbCommand"></param>
        /// <param name="useMaster"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<object> ExecuteCommandScalarAsync(IDbTransaction? Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandScalarAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandScalarAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        #endregion Command 能力

        #region 事务

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "<Pending>")]
        public async Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel? isolationLevel = null)
        {
            MySqlConnection conn = new MySqlConnection(GetConnectionString(dbName, true));
            await conn.OpenAsync().ConfigureAwait(false);

            return await conn.BeginTransactionAsync(isolationLevel ?? IsolationLevel.RepeatableRead).ConfigureAwait(false);
        }

        public async Task CommitAsync(IDbTransaction transaction)
        {
            MySqlTransaction mySqlTransaction = (MySqlTransaction)transaction;

            MySqlConnection? connection = mySqlTransaction.Connection;

            try
            {
                await mySqlTransaction.CommitAsync().ConfigureAwait(false);
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task RollbackAsync(IDbTransaction transaction)
        {
            MySqlTransaction mySqlTransaction = (MySqlTransaction)transaction;

            MySqlConnection? connection = mySqlTransaction.Connection;

            try
            {
                await mySqlTransaction.RollbackAsync().ConfigureAwait(false);
            }
            finally
            {
                if (connection != null)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion 事务
    }
}
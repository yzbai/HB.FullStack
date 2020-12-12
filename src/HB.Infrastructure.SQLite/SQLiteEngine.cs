using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Database.Engine;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.SQLite
{
    internal class SQLiteEngine : IDatabaseEngine
    {
        #region 自身 & 构建

        private readonly SQLiteOptions _options;
        private readonly Dictionary<string, string> _connectionStringDict = new Dictionary<string, string>();

        public DatabaseCommonSettings DatabaseSettings => _options.CommonSettings;

        public DatabaseEngineType EngineType => DatabaseEngineType.SQLite;

        [NotNull, DisallowNull] public string? FirstDefaultDatabaseName { get; private set; }

        public SQLiteEngine(IOptions<SQLiteOptions> options)
        {
            //MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);

            _options = options.Value;

            SetConnectionStrings();
        }

        private void SetConnectionStrings()
        {
            foreach (DatabaseConnectionSettings schemaInfo in _options.Connections)
            {
                if (FirstDefaultDatabaseName.IsNullOrEmpty())
                {
                    FirstDefaultDatabaseName = schemaInfo.DatabaseName;
                }

                if (schemaInfo.IsMaster)
                {
                    _connectionStringDict[schemaInfo.DatabaseName + "_1"] = schemaInfo.ConnectionString;

                    if (!_connectionStringDict.ContainsKey(schemaInfo.DatabaseName + "_0"))
                    {
                        _connectionStringDict[schemaInfo.DatabaseName + "_0"] = schemaInfo.ConnectionString;
                    }
                }
                else
                {
                    _connectionStringDict[schemaInfo.DatabaseName + "_0"] = schemaInfo.ConnectionString;
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

        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        public IDbCommand CreateTextCommand(string commandText, IList<KeyValuePair<string, object>>? parameterPairs = null)
        {
            SqliteCommand command = new SqliteCommand(commandText)
            {
                CommandType = CommandType.Text
            };

            if (parameterPairs == null)
            {
                return command;
            }

            foreach (var pair in parameterPairs)
            {
                command.Parameters.Add(new SqliteParameter(pair.Key, pair.Value));
            }

            return command;
        }


        #region SP

        public Task<int> ExecuteSPNonQueryAsync(IDbTransaction? trans, string dbName, string spName, IList<IDataParameter> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IDbCommand, IDataReader>> ExecuteSPReaderAsync(IDbTransaction? trans, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster)
        {
            throw new NotImplementedException();
        }

        public Task<object> ExecuteSPScalarAsync(IDbTransaction? trans, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster)
        {
            throw new NotImplementedException();
        }

        #endregion SP

        #region Command 能力

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="Transaction"></param>
        /// <param name="dbName"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>

        public Task<int> ExecuteCommandNonQueryAsync(IDbTransaction? Transaction, string dbName, IDbCommand dbCommand)
        {
            if (Transaction == null)
            {
                return SQLiteExecuter.ExecuteCommandNonQueryAsync(GetConnectionString(dbName, true), dbCommand);
            }
            else
            {
                return SQLiteExecuter.ExecuteCommandNonQueryAsync((SqliteTransaction)Transaction, dbCommand);
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

        public Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction? Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return SQLiteExecuter.ExecuteCommandReaderAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return SQLiteExecuter.ExecuteCommandReaderAsync((SqliteTransaction)Transaction, dbCommand);
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

        public Task<object> ExecuteCommandScalarAsync(IDbTransaction? Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return SQLiteExecuter.ExecuteCommandScalarAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return SQLiteExecuter.ExecuteCommandScalarAsync((SqliteTransaction)Transaction, dbCommand);
            }
        }

        #endregion Command 能力

        #region 事务

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        [SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "<Pending>")]
        public async Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel? isolationLevel = null)
        {
            SqliteConnection conn = new SqliteConnection(GetConnectionString(dbName, true));
            await conn.OpenAsync().ConfigureAwait(false);

            return conn.BeginTransaction(isolationLevel ?? IsolationLevel.Serializable);
        }

        public async Task CommitAsync(IDbTransaction transaction)
        {
            SqliteTransaction sqliteTransaction = (SqliteTransaction)transaction;

            SqliteConnection connection = sqliteTransaction.Connection;

            try
            {
                await sqliteTransaction.CommitAsync().ConfigureAwait(false);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task RollbackAsync(IDbTransaction transaction)
        {
            SqliteTransaction sqliteTransaction = (SqliteTransaction)transaction;

            SqliteConnection connection = sqliteTransaction.Connection;

            try
            {
                await sqliteTransaction.RollbackAsync().ConfigureAwait(false);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        #endregion 事务
    }
}
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
    internal class MySQLEngine : IDatabaseEngine
    {
        #region 自身 & 构建

        private readonly MySQLOptions _options;

        private readonly ILogger _logger;

        private readonly Dictionary<string, string> _connectionStringDict = new Dictionary<string, string>();

        public DatabaseCommonSettings DatabaseSettings => _options.CommonSettings;

        public EngineType EngineType => EngineType.MySQL;

        [NotNull, DisallowNull] public string? FirstDefaultDatabaseName { get; private set; }

        public IEnumerable<string> DatabaseNames { get; private set; }

        public MySQLEngine(IOptions<MySQLOptions> options, /*ILoggerFactory loggerFactory,*/ ILogger<MySQLEngine> logger)
        {
            try
            {
                //MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);
            }
            catch (InvalidOperationException ex)
            {
                GlobalSettings.Logger.LogError(ex, $"Connections:{SerializeUtil.ToJson(options.Value.Connections)}");
            }

            _options = options.Value;
            _logger = logger;

            DatabaseNames = _options.Connections.Select(s => s.DatabaseName);

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


        #endregion 自身 & 构建


        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        private static MySqlCommand CreateTextCommand(EngineCommand engineCommand)
        {
            MySqlCommand command = new MySqlCommand(engineCommand.CommandText)
            {
                CommandType = CommandType.Text
            };

            if (engineCommand.Parameters == null)
            {
                return command;
            }

            foreach (var pair in engineCommand.Parameters)
            {
                command.Parameters.Add(new MySqlParameter(pair.Key, pair.Value));
            }

            return command;
        }


        #region Command 能力

        public async Task<int> ExecuteCommandNonQueryAsync(IDbTransaction? Transaction, string dbName, EngineCommand engineCommand)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            if (Transaction == null)
            {
                return await MySQLExecuter.ExecuteCommandNonQueryAsync(GetConnectionString(dbName, true), command).ConfigureAwait(false);
            }
            else
            {
                return await MySQLExecuter.ExecuteCommandNonQueryAsync((MySqlTransaction)Transaction, command).ConfigureAwait(false);
            }
        }

        public async Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction? Transaction, string dbName, EngineCommand engineCommand, bool useMaster = false)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            if (Transaction == null)
            {
                return await MySQLExecuter.ExecuteCommandReaderAsync(GetConnectionString(dbName, useMaster), command).ConfigureAwait(false);
            }
            else
            {
                return await MySQLExecuter.ExecuteCommandReaderAsync((MySqlTransaction)Transaction, command).ConfigureAwait(false);
            }
        }

        public async Task<object> ExecuteCommandScalarAsync(IDbTransaction? Transaction, string dbName, EngineCommand engineCommand, bool useMaster = false)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            if (Transaction == null)
            {
                return await MySQLExecuter.ExecuteCommandScalarAsync(GetConnectionString(dbName, useMaster), command).ConfigureAwait(false);
            }
            else
            {
                return await MySQLExecuter.ExecuteCommandScalarAsync((MySqlTransaction)Transaction, command).ConfigureAwait(false);
            }
        }

        #endregion Command 能力

        #region 事务

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
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
                    await connection.DisposeAsync().ConfigureAwait(false);
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
                    await connection.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion 事务
    }
}
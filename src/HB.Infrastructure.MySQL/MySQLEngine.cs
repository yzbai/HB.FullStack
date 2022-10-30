using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MySqlConnector;
using MySqlConnector.Logging;

namespace HB.Infrastructure.MySQL
{
    public class MySQLEngine : IDatabaseEngine
    {
        public EngineType EngineType => EngineType.MySQL;

        public MySQLEngine(ILoggerFactory loggerFactory)
        {
            MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);
        }

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

        public async Task<int> ExecuteCommandNonQueryAsync(string connectionString, EngineCommand engineCommand)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);
            return await MySQLExecuter.ExecuteCommandNonQueryAsync(connectionString, command).ConfigureAwait(false);
        }

        public async Task<int> ExecuteCommandNonQueryAsync(IDbTransaction trans, EngineCommand engineCommand)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            return await MySQLExecuter.ExecuteCommandNonQueryAsync((MySqlTransaction)trans, command).ConfigureAwait(false);
        }

        public async Task<IDataReader> ExecuteCommandReaderAsync(string connectionString, EngineCommand engineCommand, bool useMaster = false)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            return await MySQLExecuter.ExecuteCommandReaderAsync(connectionString, command).ConfigureAwait(false);
        }

        public async Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction trans, EngineCommand engineCommand, bool useMaster = false)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            return await MySQLExecuter.ExecuteCommandReaderAsync((MySqlTransaction)trans, command).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteCommandScalarAsync(string connectionString, EngineCommand engineCommand, bool useMaster = false)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            return await MySQLExecuter.ExecuteCommandScalarAsync(connectionString, command).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteCommandScalarAsync(IDbTransaction trans, EngineCommand engineCommand, bool useMaster = false)
        {
            using MySqlCommand command = CreateTextCommand(engineCommand);

            return await MySQLExecuter.ExecuteCommandScalarAsync((MySqlTransaction)trans, command).ConfigureAwait(false);
        }

        #endregion Command 能力

        #region 事务

        public async Task<IDbTransaction> BeginTransactionAsync(string connectionString, IsolationLevel? isolationLevel = null)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);
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
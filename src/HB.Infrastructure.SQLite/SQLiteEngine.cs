using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Database.Engine;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.SQLite
{
    internal class SQLiteEngine : IDatabaseEngine
    {
        public EngineType EngineType => EngineType.SQLite;

        public SQLiteEngine()
        {
        }

        public static SqliteCommand CreateTextCommand(EngineCommand engineCommand)
        {
            SqliteCommand command = new SqliteCommand(engineCommand.CommandText)
            {
                CommandType = CommandType.Text
            };

            if (engineCommand.Parameters == null)
            {
                return command;
            }

            foreach (var pair in engineCommand.Parameters)
            {
                command.Parameters.Add(new SqliteParameter(pair.Key, pair.Value));
            }

            return command;
        }

        #region Command 能力

        public async Task<int> ExecuteCommandNonQueryAsync(ConnectionString connectionString, EngineCommand engineCommand)
        {
            using SqliteCommand dbCommand = CreateTextCommand(engineCommand);

            return await SQLiteExecuter.ExecuteCommandNonQueryAsync(connectionString, dbCommand).ConfigureAwait(false);
        }

        public async Task<int> ExecuteCommandNonQueryAsync(IDbTransaction Transaction, EngineCommand engineCommand)
        {
            using SqliteCommand dbCommand = CreateTextCommand(engineCommand);

            return await SQLiteExecuter.ExecuteCommandNonQueryAsync((SqliteTransaction)Transaction, dbCommand).ConfigureAwait(false);
        }

        public async Task<IDataReader> ExecuteCommandReaderAsync(ConnectionString connectionString, EngineCommand engineCommand)
        {
            //使用using的话，会同时关闭reader.
            //在Microsoft.Data.Sqlite实现中， dipose connection后，会自动dispose command
            SqliteCommand dbCommand = CreateTextCommand(engineCommand);
            return await SQLiteExecuter.ExecuteCommandReaderAsync(connectionString, dbCommand).ConfigureAwait(false);
        }

        public async Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction Transaction, EngineCommand engineCommand)
        {
            //使用using的话，会同时关闭reader.
            //在Microsoft.Data.Sqlite实现中， dipose connection后，会自动dispose command

            SqliteCommand dbCommand = CreateTextCommand(engineCommand);
            return await SQLiteExecuter.ExecuteCommandReaderAsync((SqliteTransaction)Transaction, dbCommand).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteCommandScalarAsync(ConnectionString connectionString, EngineCommand engineCommand)
        {
            using SqliteCommand dbCommand = CreateTextCommand(engineCommand);

            return await SQLiteExecuter.ExecuteCommandScalarAsync(connectionString, dbCommand).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteCommandScalarAsync(IDbTransaction Transaction, EngineCommand engineCommand)
        {
            using SqliteCommand dbCommand = CreateTextCommand(engineCommand);

            return await SQLiteExecuter.ExecuteCommandScalarAsync((SqliteTransaction)Transaction, dbCommand).ConfigureAwait(false);
        }

        #endregion

        #region 事务

        //TODO: 解决问题
        //SQLite Error 1: 'cannot start a transaction within a transaction'.
        //SQLite Error 5: 'database is locked'.
        //private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public async Task<IDbTransaction> BeginTransactionAsync(ConnectionString connectionString, IsolationLevel? isolationLevel = null)
        {
            //if (!await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false))
            //{
            //    throw DatabaseExceptions.TransactionError("等待sqlite事务超过5秒钟", null, 0);
            //}

            SqliteConnection conn = new SqliteConnection(connectionString.ToString());

            await conn.OpenAsync().ConfigureAwait(false);

            return conn.BeginTransaction(isolationLevel ?? IsolationLevel.Unspecified);
        }

        public async Task CommitAsync(IDbTransaction transaction)
        {
            //_semaphoreSlim.Release();

            SqliteTransaction sqliteTransaction = (SqliteTransaction)transaction;

            SqliteConnection connection = sqliteTransaction.Connection!;

            try
            {
                await sqliteTransaction.CommitAsync().ConfigureAwait(false);
            }
            finally
            {
                await connection.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task RollbackAsync(IDbTransaction transaction)
        {
            //_semaphoreSlim.Release();

            SqliteTransaction sqliteTransaction = (SqliteTransaction)transaction;

            SqliteConnection connection = sqliteTransaction.Connection!;

            try
            {
                await sqliteTransaction.RollbackAsync().ConfigureAwait(false);
            }
            finally
            {
                await connection.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion
    }
}
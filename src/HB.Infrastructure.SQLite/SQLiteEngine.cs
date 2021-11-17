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

        public EngineType EngineType => EngineType.SQLite;

        public string FirstDefaultDatabaseName { get; private set; } = null!;

        public IEnumerable<string> DatabaseNames { get; private set; }

        public SQLiteEngine(IOptions<SQLiteOptions> options)
        {
            _options = options.Value;

            DatabaseNames = _options.Connections.Select(s => s.DatabaseName).ToList();

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


        #endregion

        public static SqliteCommand CreateTextCommand(EngineCommand engineCommand)
        {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            SqliteCommand command = new SqliteCommand(engineCommand.CommandText)
            {
                CommandType = CommandType.Text
            };
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

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

        public async Task<int> ExecuteCommandNonQueryAsync(IDbTransaction? Transaction, string dbName, EngineCommand engineCommand)
        {
            using SqliteCommand dbCommand = CreateTextCommand(engineCommand);

            if (Transaction == null)
            {
                return await SQLiteExecuter.ExecuteCommandNonQueryAsync(GetConnectionString(dbName, true), dbCommand).ConfigureAwait(false);
            }
            else
            {
                return await SQLiteExecuter.ExecuteCommandNonQueryAsync((SqliteTransaction)Transaction, dbCommand).ConfigureAwait(false);
            }
        }

        public async Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction? Transaction, string dbName, EngineCommand engineCommand, bool useMaster = false)
        {
            //使用using的话，会同时关闭reader. 
            //在Microsoft.Data.Sqlite实现中， dipose connection后，会自动dispose command
#pragma warning disable CA2000 // Dispose objects before losing scope
            SqliteCommand dbCommand = CreateTextCommand(engineCommand);
#pragma warning restore CA2000 // Dispose objects before losing scope

            if (Transaction == null)
            {
                return await SQLiteExecuter.ExecuteCommandReaderAsync(GetConnectionString(dbName, useMaster), dbCommand).ConfigureAwait(false);
            }
            else
            {
                return await SQLiteExecuter.ExecuteCommandReaderAsync((SqliteTransaction)Transaction, dbCommand).ConfigureAwait(false);
            }
        }

        public async Task<object?> ExecuteCommandScalarAsync(IDbTransaction? Transaction, string dbName, EngineCommand engineCommand, bool useMaster = false)
        {
            using SqliteCommand dbCommand = CreateTextCommand(engineCommand);

            if (Transaction == null)
            {
                return await SQLiteExecuter.ExecuteCommandScalarAsync(GetConnectionString(dbName, useMaster), dbCommand).ConfigureAwait(false);
            }
            else
            {
                return await SQLiteExecuter.ExecuteCommandScalarAsync((SqliteTransaction)Transaction, dbCommand).ConfigureAwait(false);
            }
        }

        #endregion

        #region 事务

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        public async Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel? isolationLevel = null)
        {
            SqliteConnection conn = new SqliteConnection(GetConnectionString(dbName, true));
            await conn.OpenAsync().ConfigureAwait(false);

            return conn.BeginTransaction(isolationLevel ?? IsolationLevel.Serializable);
        }

        public async Task CommitAsync(IDbTransaction transaction)
        {
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

        #endregion 事务
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.Sqlite;

namespace HB.Infrastructure.SQLite
{
    internal partial class SQLiteExecuter
    {
        #region Command Reader

        public static Task<IDataReader> ExecuteCommandReaderAsync(SqliteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandReaderAsync(sqliteTransaction.Connection, false, (SqliteCommand)dbCommand);
        }

        public static Task<IDataReader> ExecuteCommandReaderAsync(string connectString, IDbCommand dbCommand)
        {
            SqliteConnection conn = new SqliteConnection(connectString);
            return ExecuteCommandReaderAsync(conn, true, (SqliteCommand)dbCommand);
        }

        private static async Task<IDataReader> ExecuteCommandReaderAsync(SqliteConnection connection, bool isOwnedConnection, SqliteCommand command)
        {
            SqliteDataReader reader = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = connection;

                if (isOwnedConnection)
                {
                    reader = (SqliteDataReader)await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
                }
                else
                {
                    reader = (SqliteDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
                }

                return reader;
            }
            catch 
            {
                if (isOwnedConnection)
                {
                    connection.Close();
                }

                if (reader != null)
                {
                    reader.Close();
                }

                throw;
            }
        }

        #endregion

        #region Command Scalar

        public static Task<object> ExecuteCommandScalarAsync(string connectString, IDbCommand dbCommand)
        {
            SqliteConnection conn = new SqliteConnection(connectString);
            return ExecuteCommandScalarAsync(conn, true, (SqliteCommand)dbCommand);
        }

        public static Task<object> ExecuteCommandScalarAsync(SqliteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandScalarAsync(sqliteTransaction.Connection, false, (SqliteCommand)dbCommand);
        }

        private static async Task<object> ExecuteCommandScalarAsync(SqliteConnection connection, bool isOwnedConnection, SqliteCommand command)
        {
            object rtObj = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = connection;

                rtObj = await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            catch 
            {
                throw;
            }
            finally
            {
                if (isOwnedConnection)
                {
                    connection.Close();
                }
            }

            return rtObj;
        }

        #endregion

        #region Comand NonQuery

        public static Task<int> ExecuteCommandNonQueryAsync(string connectString, IDbCommand dbCommand)
        {
            SqliteConnection conn = new SqliteConnection(connectString);

            return ExecuteCommandNonQueryAsync(conn, true, (SqliteCommand)dbCommand);
        }

        public static Task<int> ExecuteCommandNonQueryAsync(SqliteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandNonQueryAsync(sqliteTransaction.Connection, false, (SqliteCommand)dbCommand);
        }

        private static async Task<int> ExecuteCommandNonQueryAsync(SqliteConnection conn, bool isOwnedConnection, SqliteCommand command)
        {
            int rtInt = -1;

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = conn;

                rtInt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (isOwnedConnection)
                {
                    conn.Close();
                }
            }

            return rtInt;
        }

        #endregion

    }
}
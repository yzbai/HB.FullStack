using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace HB.Infrastructure.SQLite
{
    internal partial class SQLiteExecuter
    {
        #region Command Reader

        public static Task<IDataReader> ExecuteCommandReaderAsync(SQLiteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandReaderAsync(sqliteTransaction.Connection, false, (SQLiteCommand)dbCommand);
        }

        public static Task<IDataReader> ExecuteCommandReaderAsync(string connectString, IDbCommand dbCommand)
        {
            SQLiteConnection conn = new SQLiteConnection(connectString);
            return ExecuteCommandReaderAsync(conn, true, (SQLiteCommand)dbCommand);
        }

        private static async Task<IDataReader> ExecuteCommandReaderAsync(SQLiteConnection connection, bool isOwnedConnection, SQLiteCommand command)
        {
            SQLiteDataReader reader = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = connection;

                if (isOwnedConnection)
                {
                    reader = (SQLiteDataReader)await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
                }
                else
                {
                    reader = (SQLiteDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
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
            SQLiteConnection conn = new SQLiteConnection(connectString);
            return ExecuteCommandScalarAsync(conn, true, (SQLiteCommand)dbCommand);
        }

        public static Task<object> ExecuteCommandScalarAsync(SQLiteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandScalarAsync(sqliteTransaction.Connection, false, (SQLiteCommand)dbCommand);
        }

        private static async Task<object> ExecuteCommandScalarAsync(SQLiteConnection connection, bool isOwnedConnection, SQLiteCommand command)
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
            SQLiteConnection conn = new SQLiteConnection(connectString);

            return ExecuteCommandNonQueryAsync(conn, true, (SQLiteCommand)dbCommand);
        }

        public static Task<int> ExecuteCommandNonQueryAsync(SQLiteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandNonQueryAsync(sqliteTransaction.Connection, false, (SQLiteCommand)dbCommand);
        }

        private static async Task<int> ExecuteCommandNonQueryAsync(SQLiteConnection conn, bool isOwnedConnection, SQLiteCommand command)
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
using System;
using System.Data;
using System.Threading.Tasks;

using HB.FullStack.Database;

using Microsoft.Data.Sqlite;

namespace HB.Infrastructure.SQLite
{
    /// <summary>
    /// 动态SQL和SP执行
    /// 具体执行步骤都要有异常捕捉，直接抛出给上一层
    /// </summary>
    internal static class SQLiteExecuter
    {
        #region Command Reader

        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="sqliteTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static async Task<IDataReader> ExecuteCommandReaderAsync(SqliteTransaction sqliteTransaction, SqliteCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return await ExecuteCommandReaderAsync(sqliteTransaction.Connection!, false, dbCommand).ConfigureAwait(false);
        }

        /// <returns></returns>
        
        public static async Task<IDataReader> ExecuteCommandReaderAsync(string connectString, SqliteCommand dbCommand)
        {
            //这里无法用Using，因为reader要用
            SqliteConnection conn = new SqliteConnection(connectString);
            return await ExecuteCommandReaderAsync(conn, true, dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="isOwnedConnection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        
        private static async Task<IDataReader> ExecuteCommandReaderAsync(SqliteConnection connection, bool isOwnedConnection, SqliteCommand command)
        {
            SqliteDataReader? reader = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = connection;

                if (isOwnedConnection)
                {
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
                }
                else
                {
                    reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                }

                return reader;
            }
            catch (Exception ex)
            {
                if (reader != null)
                {
                    await connection.DisposeAsync().ConfigureAwait(false);
                }

                if (isOwnedConnection)
                {
                    await connection.DisposeAsync().ConfigureAwait(false);
                }

                throw DatabaseExceptions.ExecuterError(command.CommandText, ex);
            }
        }

        #endregion Command Reader

        #region Command Scalar

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static async Task<object?> ExecuteCommandScalarAsync(string connectString, SqliteCommand dbCommand)
        {
            using SqliteConnection conn = new SqliteConnection(connectString);
            return await ExecuteCommandScalarAsync(conn, dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="sqliteTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static async Task<object?> ExecuteCommandScalarAsync(SqliteTransaction sqliteTransaction, SqliteCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return await ExecuteCommandScalarAsync(sqliteTransaction.Connection!, dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        
        private static async Task<object?> ExecuteCommandScalarAsync(SqliteConnection connection, SqliteCommand command)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = connection;

                return await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw DatabaseExceptions.ExecuterError(command.CommandText, ex);
            }
        }

        #endregion Command Scalar

        #region Comand NonQuery



        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static async Task<int> ExecuteCommandNonQueryAsync(string connectString, SqliteCommand dbCommand)
        {
            using SqliteConnection conn = new SqliteConnection(connectString);

            return await ExecuteCommandNonQueryAsync(conn, dbCommand).ConfigureAwait(false);
        }


        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="sqliteTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static async Task<int> ExecuteCommandNonQueryAsync(SqliteTransaction sqliteTransaction, SqliteCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return await ExecuteCommandNonQueryAsync(sqliteTransaction.Connection!, dbCommand).ConfigureAwait(false);
        }


        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        
        private static async Task<int> ExecuteCommandNonQueryAsync(SqliteConnection conn, SqliteCommand command)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = conn;

                return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw DatabaseExceptions.ExecuterError(command.CommandText, ex);
            }
        }

        #endregion Comand NonQuery
    }
}
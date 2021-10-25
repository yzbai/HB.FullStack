using HB.FullStack.Database;

using MySqlConnector;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HB.Infrastructure.MySQL
{
    /// <summary>
    /// 动态SQL和SP执行
    /// 具体执行步骤都要有异常捕捉，直接抛出给上一层
    /// </summary>
    internal static class MySQLExecuter
    {
        #region Command Reader

        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static async Task<IDataReader> ExecuteCommandReaderAsync(MySqlTransaction mySqlTransaction, MySqlCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;

            return await ExecuteCommandReaderAsync(
                mySqlTransaction.Connection ?? throw DatabaseExceptions.TransactionConnectionIsNull(commandText:dbCommand.CommandText),
                false,
                dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static async Task<IDataReader> ExecuteCommandReaderAsync(string connectString, MySqlCommand dbCommand)
        {
#pragma warning disable CA2000 // 这里无法用Using，因为reader要用
            MySqlConnection conn = new MySqlConnection(connectString);
#pragma warning restore CA2000 // Dispose objects before losing scope
            return await ExecuteCommandReaderAsync(conn, true, dbCommand).ConfigureAwait(false);
        }


        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="isOwnedConnection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static async Task<IDataReader> ExecuteCommandReaderAsync(MySqlConnection connection, bool isOwnedConnection, MySqlCommand command)
        {
            MySqlDataReader? reader = null;

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
                    await reader.DisposeAsync().ConfigureAwait(false);
                }

                if (isOwnedConnection)
                {
                    await connection.DisposeAsync().ConfigureAwait(false);
                }

                throw DatabaseExceptions.ExecuterError(commandText:command.CommandText, innerException: ex);
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
        /// <exception cref="DatabaseException"></exception>
        public static async Task<object?> ExecuteCommandScalarAsync(string connectString, MySqlCommand dbCommand)
        {
            using MySqlConnection conn = new MySqlConnection(connectString);
            return await ExecuteCommandScalarAsync(conn, dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static async Task<object?> ExecuteCommandScalarAsync(MySqlTransaction mySqlTransaction, MySqlCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;
            return await ExecuteCommandScalarAsync(
                mySqlTransaction.Connection ?? throw DatabaseExceptions.TransactionConnectionIsNull(dbCommand.CommandText),
                dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static async Task<object?> ExecuteCommandScalarAsync(MySqlConnection connection, MySqlCommand command)
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
                throw DatabaseExceptions.ExecuterError(commandText: command.CommandText, innerException: ex);
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
        /// <exception cref="DatabaseException"></exception>
        public static async Task<int> ExecuteCommandNonQueryAsync(string connectString, MySqlCommand dbCommand)
        {
            using MySqlConnection conn = new MySqlConnection(connectString);

            return await ExecuteCommandNonQueryAsync(conn, dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static async Task<int> ExecuteCommandNonQueryAsync(MySqlTransaction mySqlTransaction, MySqlCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;
            return await ExecuteCommandNonQueryAsync(
                mySqlTransaction.Connection ?? throw DatabaseExceptions.TransactionConnectionIsNull(dbCommand.CommandText),
                dbCommand).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static async Task<int> ExecuteCommandNonQueryAsync(MySqlConnection conn, MySqlCommand command)
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
                throw DatabaseExceptions.ExecuterError(commandText: command.CommandText, innerException: ex);
            }
        }

        #endregion Comand NonQuery
    }
}
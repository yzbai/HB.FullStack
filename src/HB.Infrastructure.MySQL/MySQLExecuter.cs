using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Database.Config;
using MySqlConnector;

namespace HB.Infrastructure.MySQL
{
    /// <summary>
    /// 动态SQL和SP执行
    /// 具体执行步骤都要有异常捕捉，直接抛出给上一层
    /// </summary>
    internal static class MySQLExecuter
    {
        #region Command Reader

        public static async Task<IDataReader> ExecuteCommandReaderAsync(MySqlTransaction mySqlTransaction, MySqlCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;

            return await ExecuteCommandReaderAsync(
                mySqlTransaction.Connection ?? throw DbExceptions.TransactionConnectionIsNull(commandText: dbCommand.CommandText),
                false,
                dbCommand).ConfigureAwait(false);
        }

        public static async Task<IDataReader> ExecuteCommandReaderAsync(ConnectionString connectString, MySqlCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString.ToString());
            return await ExecuteCommandReaderAsync(conn, true, dbCommand).ConfigureAwait(false);
        }

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

                throw ConvertToDbException(command, ex);
            }
        }

        #endregion Command Reader

        #region Command Scalar

        public static async Task<object?> ExecuteCommandScalarAsync(ConnectionString connectString, MySqlCommand dbCommand)
        {
            using MySqlConnection conn = new MySqlConnection(connectString.ToString());
            return await ExecuteCommandScalarAsync(conn, dbCommand).ConfigureAwait(false);
        }

        public static async Task<object?> ExecuteCommandScalarAsync(MySqlTransaction mySqlTransaction, MySqlCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;
            return await ExecuteCommandScalarAsync(
                mySqlTransaction.Connection ?? throw DbExceptions.TransactionConnectionIsNull(dbCommand.CommandText),
                dbCommand).ConfigureAwait(false);
        }

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
                throw ConvertToDbException(command, ex);
            }
        }

        #endregion Command Scalar

        #region Comand NonQuery

        public static async Task<int> ExecuteCommandNonQueryAsync(ConnectionString connectString, MySqlCommand dbCommand)
        {
            using MySqlConnection conn = new MySqlConnection(connectString.ToString());

            return await ExecuteCommandNonQueryAsync(conn, dbCommand).ConfigureAwait(false);
        }

        public static async Task<int> ExecuteCommandNonQueryAsync(MySqlTransaction mySqlTransaction, MySqlCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;
            return await ExecuteCommandNonQueryAsync(
                mySqlTransaction.Connection ?? throw DbExceptions.TransactionConnectionIsNull(dbCommand.CommandText),
                dbCommand).ConfigureAwait(false);
        }

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
                throw ConvertToDbException(command, ex);
            }
        }

        #endregion Comand NonQuery

        private static Exception ConvertToDbException(MySqlCommand command, Exception ex)
        {
            if (ex is MySqlException mEx)
            {

                return mEx.ErrorCode switch
                {
                    MySqlErrorCode.DuplicateKeyEntry => DbExceptions.DuplicateKeyError(command.CommandText, ex),
                    _ => DbExceptions.MySQLExecuterError(command.CommandText, mEx.ErrorCode.ToString(), mEx.SqlState, ex)
                };
            }

            return DbExceptions.MySQLUnKownExecuterError(command.CommandText, ex);
        }
    }
}
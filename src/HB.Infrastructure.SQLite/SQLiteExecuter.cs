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
        
        public static Task<IDataReader> ExecuteCommandReaderAsync(SqliteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandReaderAsync(sqliteTransaction.Connection, false, (SqliteCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static Task<IDataReader> ExecuteCommandReaderAsync(string connectString, IDbCommand dbCommand)
        {
            SqliteConnection conn = new SqliteConnection(connectString);
            return ExecuteCommandReaderAsync(conn, true, (SqliteCommand)dbCommand);
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
                    reader = (SqliteDataReader)await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
                }
                else
                {
                    reader = (SqliteDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
                }

                return reader;
            }
            catch (Exception ex)
            {
                if (isOwnedConnection)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }

                if (reader != null)
                {
                    await reader.CloseAsync().ConfigureAwait(false);
                }

                if (ex is SqliteException sqliteException)
                {
                    throw new DatabaseEngineException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", sqliteException);
                }
                else
                {
                    throw new DatabaseEngineException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
                }
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
        
        public static Task<object> ExecuteCommandScalarAsync(string connectString, IDbCommand dbCommand)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            SqliteConnection conn = new SqliteConnection(connectString);
#pragma warning restore CA2000 // Dispose objects before losing scope
            return ExecuteCommandScalarAsync(conn, true, (SqliteCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="sqliteTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static Task<object> ExecuteCommandScalarAsync(SqliteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandScalarAsync(sqliteTransaction.Connection, false, (SqliteCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="isOwnedConnection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        
        private static async Task<object> ExecuteCommandScalarAsync(SqliteConnection connection, bool isOwnedConnection, SqliteCommand command)
        {
            object rtObj;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().ConfigureAwait(false);
                }

                command.Connection = connection;

                rtObj = await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            catch (SqliteException sqliteException)
            {
                throw new DatabaseEngineException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", sqliteException);
            }
            catch (Exception ex)
            {
                throw new DatabaseEngineException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
            }
            finally
            {
                if (isOwnedConnection)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }

            return rtObj;
        }

        #endregion Command Scalar

        #region Comand NonQuery

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static Task<int> ExecuteCommandNonQueryAsync(string connectString, IDbCommand dbCommand)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            SqliteConnection conn = new SqliteConnection(connectString);
#pragma warning restore CA2000 // Dispose objects before losing scope

            return ExecuteCommandNonQueryAsync(conn, true, (SqliteCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="sqliteTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        
        public static Task<int> ExecuteCommandNonQueryAsync(SqliteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandNonQueryAsync(sqliteTransaction.Connection, false, (SqliteCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="isOwnedConnection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        
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
            catch (SqliteException sqliteException)
            {
                throw new DatabaseEngineException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", sqliteException);
            }
            catch (Exception ex)
            {
                throw new DatabaseEngineException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
            }
            finally
            {
                if (isOwnedConnection)
                {
                    await conn.CloseAsync().ConfigureAwait(false);
                }
            }

            return rtInt;
        }

        #endregion Comand NonQuery
    }
}
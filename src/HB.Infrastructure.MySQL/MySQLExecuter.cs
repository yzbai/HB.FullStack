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
        public static Task<IDataReader> ExecuteCommandReaderAsync(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;

            return ExecuteCommandReaderAsync(
                mySqlTransaction.Connection ?? throw new DatabaseException(ErrorCode.DatabaseTransactionConnectionIsNull, null, $"CommandText:{dbCommand.CommandText}"),
                false,
                (MySqlCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandReaderAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<IDataReader> ExecuteCommandReaderAsync(string connectString, IDbCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteCommandReaderAsync(conn, true, (MySqlCommand)dbCommand);
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
            IDataReader? reader = null;

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
                if (isOwnedConnection)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }

                reader?.Close();

                if (ex is MySqlException mySqlException)
                {
                    throw new DatabaseException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", mySqlException);
                }
                else
                {
                    throw new DatabaseException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
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
        /// <exception cref="DatabaseException"></exception>
        public static Task<object> ExecuteCommandScalarAsync(string connectString, IDbCommand dbCommand)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            MySqlConnection conn = new MySqlConnection(connectString);
#pragma warning restore CA2000 // Dispose objects before losing scope
            return ExecuteCommandScalarAsync(conn, true, (MySqlCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<object> ExecuteCommandScalarAsync(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;
            return ExecuteCommandScalarAsync(
                mySqlTransaction.Connection ?? throw new DatabaseException(ErrorCode.DatabaseTransactionConnectionIsNull, null, $"CommandText:{dbCommand.CommandText}"),
                false,
                (MySqlCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandScalarAsync
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="isOwnedConnection"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static async Task<object> ExecuteCommandScalarAsync(MySqlConnection connection, bool isOwnedConnection, MySqlCommand command)
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
            catch (MySqlException mysqlException)
            {
                throw new DatabaseException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", mysqlException);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
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
        /// <exception cref="DatabaseException"></exception>
        public static Task<int> ExecuteCommandNonQueryAsync(string connectString, IDbCommand dbCommand)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            MySqlConnection conn = new MySqlConnection(connectString);
#pragma warning restore CA2000 // Dispose objects before losing scope

            return ExecuteCommandNonQueryAsync(conn, true, (MySqlCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<int> ExecuteCommandNonQueryAsync(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = mySqlTransaction;
            return ExecuteCommandNonQueryAsync(
                mySqlTransaction.Connection ?? throw new DatabaseException(ErrorCode.DatabaseTransactionConnectionIsNull, null, $"CommandText:{dbCommand.CommandText}"),
                false,
                (MySqlCommand)dbCommand);
        }

        /// <summary>
        /// ExecuteCommandNonQueryAsync
        /// </summary>
        private static async Task<int> ExecuteCommandNonQueryAsync(MySqlConnection conn, bool isOwnedConnection, MySqlCommand command)
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
            catch (MySqlException mysqlException)
            {
                throw new DatabaseException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", mysqlException);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
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

        #region SP NonQuery

        #region Privates

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        private static async Task PrepareCommandAsync(MySqlCommand command, MySqlConnection connection, MySqlTransaction? transaction,
            CommandType commandType, string commandText, IEnumerable<IDataParameter> commandParameters)
        {
            //if the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            //associate the connection with the command
            command.Connection = connection;

            //if we were provided a transaction, assign it.
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            //set the command type
            command.CommandType = commandType;

            //attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }

            //set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            return;
        }

        private static void AttachParameters(MySqlCommand command, IEnumerable<IDataParameter> commandParameters)
        {
            foreach (IDataParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        #endregion Privates

        /// <summary>
        /// ExecuteSPNonQueryAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<int> ExecuteSPNonQueryAsync(string connectString, string spName, IList<IDataParameter> parameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteSPNonQueryAsync(conn, null, true, spName, parameters);
        }

        /// <summary>
        /// ExecuteSPNonQueryAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<int> ExecuteSPNonQueryAsync(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> parameters)
        {
            return ExecuteSPNonQueryAsync(mySqlTransaction.Connection ?? throw new DatabaseException(ErrorCode.DatabaseTransactionConnectionIsNull, null, $"SpName:{spName}"), mySqlTransaction, false, spName, parameters);
        }

        /// <summary>
        /// ExecuteSPNonQueryAsync
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="isOwnedConnection"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static async Task<int> ExecuteSPNonQueryAsync(MySqlConnection conn, MySqlTransaction? trans, bool isOwnedConnection, string spName, IList<IDataParameter> parameters)
        {
            int rtInt = -1;
            MySqlCommand command = new MySqlCommand();

            await PrepareCommandAsync(command, conn, trans, CommandType.StoredProcedure, spName, parameters).ConfigureAwait(false);

            try
            {
                rtInt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (MySqlException mysqlException)
            {
                throw new DatabaseException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", mysqlException);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
            }
            finally
            {
                if (isOwnedConnection)
                {
                    await conn.CloseAsync().ConfigureAwait(false);
                }
            }

            command.Parameters.Clear();
            command.Dispose();

            return rtInt;
        }

        #endregion SP NonQuery

        #region SP Scalar

        /// <summary>
        /// ExecuteSPScalarAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<object> ExecuteSPScalarAsync(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> parameters)
        {
            return ExecuteSPScalarAsync(mySqlTransaction.Connection ?? throw new DatabaseException(ErrorCode.DatabaseTransactionConnectionIsNull, null, $"SpName:{spName}"), mySqlTransaction, false, spName, parameters);
        }

        /// <summary>
        /// ExecuteSPScalarAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<object> ExecuteSPScalarAsync(string connectString, string spName, IList<IDataParameter> parameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteSPScalarAsync(conn, null, true, spName, parameters);
        }

        /// <summary>
        /// ExecuteSPScalarAsync
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="isOwnedConnection"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private static async Task<object> ExecuteSPScalarAsync(MySqlConnection conn, MySqlTransaction? trans, bool isOwnedConnection, string spName, IList<IDataParameter> parameters)
        {
            object rtObj;
            MySqlCommand command = new MySqlCommand();

            await PrepareCommandAsync(command, conn, trans, CommandType.StoredProcedure, spName, parameters).ConfigureAwait(false);

            try
            {
                rtObj = await command.ExecuteScalarAsync().ConfigureAwait(false);
            }
            catch (MySqlException mysqlException)
            {
                throw new DatabaseException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", mysqlException);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
            }
            finally
            {
                if (isOwnedConnection)
                {
                    await conn.CloseAsync().ConfigureAwait(false);
                }
            }
            command.Parameters.Clear();
            command.Dispose();

            return rtObj;
        }

        #endregion SP Scalar

        #region SP Reader

        /// <summary>
        /// ExecuteSPReaderAsync
        /// </summary>
        /// <param name="connectString"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static async Task<Tuple<IDbCommand, IDataReader>> ExecuteSPReaderAsync(string connectString, string spName, IList<IDataParameter> dbParameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            await conn.OpenAsync().ConfigureAwait(false);

            return await ExecuteSPReaderAsync(conn, null, true, spName, dbParameters).ConfigureAwait(false);
        }

        /// <summary>
        /// ExecuteSPReaderAsync
        /// </summary>
        /// <param name="mySqlTransaction"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static Task<Tuple<IDbCommand, IDataReader>> ExecuteSPReaderAsync(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> dbParameters)
        {
            return ExecuteSPReaderAsync(mySqlTransaction.Connection ?? throw new DatabaseException(ErrorCode.DatabaseTransactionConnectionIsNull, null, $"SpName:{spName}"), mySqlTransaction, false, spName, dbParameters);
        }

        /// <summary>
        /// ExecuteSPReaderAsync
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="mySqlTransaction"></param>
        /// <param name="isOwedConnection"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private static async Task<Tuple<IDbCommand, IDataReader>> ExecuteSPReaderAsync(MySqlConnection connection, MySqlTransaction? mySqlTransaction, bool isOwedConnection, string spName, IList<IDataParameter> dbParameters)
        {
            MySqlCommand command = new MySqlCommand();

            await PrepareCommandAsync(command, connection, mySqlTransaction, CommandType.StoredProcedure, spName, dbParameters).ConfigureAwait(false);
            IDataReader? reader = null;

            try
            {
                if (isOwedConnection)
                {
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
                }
                else
                {
                    reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                if (isOwedConnection)
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }

                reader?.Close();

                if (ex is MySqlException mySqlException)
                {
                    throw new DatabaseException(ErrorCode.DatabaseExecuterError, null, $"CommandText:{command.CommandText}", mySqlException);
                }
                else
                {
                    throw new DatabaseException(ErrorCode.DatabaseError, null, $"CommandText:{command.CommandText}", ex);
                }
            }

            command.Parameters.Clear();

            return new Tuple<IDbCommand, IDataReader>(command, reader);
        }

        #endregion SP Reader
    }
}
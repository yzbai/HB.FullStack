using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace HB.Infrastructure.MySQL
{
    public partial class MySQLExecuter
    {
        #region Reader

        public static Task<IDataReader> ExecuteSqlReaderAsync(string connectionString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            MySqlCommand command = new MySqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString;

            return ExecuteCommandReaderAsync(conn, true, command);
        }

        public static Task<IDataReader> ExecuteSqlReaderAsync(MySqlTransaction mySqlTransaction, string sqlString)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString;

            return ExecuteCommandReaderAsync(mySqlTransaction.Connection, false, command);
        }

        public static Task<IDataReader> ExecuteSqlReaderAsync(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            return ExecuteCommandReaderAsync(mySqlTransaction.Connection, false, (MySqlCommand)dbCommand);
        }

        public static Task<IDataReader> ExecuteCommandReaderAsync(string connectString, IDbCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteCommandReaderAsync(conn, true, (MySqlCommand)dbCommand);
        }

        private static async Task<IDataReader> ExecuteCommandReaderAsync(MySqlConnection connection, bool isOwnedConnection, MySqlCommand command)
        {
            MySqlDataReader reader = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                command.Connection = connection;

                if (isOwnedConnection)
                {
                    reader = (MySqlDataReader)await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                }
                else
                {
                    reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                }

                return reader;
            }
            catch (Exception ex)
            {
                if (isOwnedConnection)
                {
                    connection.Close();
                }

                if (reader != null)
                {
                    reader.Close();
                }

                throw ex;
            }
        }

        #endregion

        #region Scalar

        public static Task<object> ExecuteCommandScalarAsync(string connectString, IDbCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteCommandScalarAsync(conn, true, (MySqlCommand)dbCommand);
        }

        public static Task<object> ExecuteCommandScalarAsync(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            return ExecuteCommandScalarAsync(mySqlTransaction.Connection, false, (MySqlCommand)dbCommand);
        }

        public static Task<object> ExecuteSqlScalarAsync(string connectionString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            MySqlCommand command = new MySqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString;

            return ExecuteCommandScalarAsync(conn, true, command);
        }

        public static Task<object> ExecuteSqlScalarAsync(MySqlTransaction mySqlTransaction, string sqlString)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString;

            return ExecuteCommandScalarAsync(mySqlTransaction.Connection, false, command);
        }

        private static async Task<object> ExecuteCommandScalarAsync(MySqlConnection connection, bool isOwnedConnection, MySqlCommand command)
        {
            object rtObj = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                command.Connection = connection;

                rtObj = await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw ex;
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

        #region NonQuery

        public static Task<int> ExecuteCommandNonQueryAsync(string connectString, IDbCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString);

            return ExecuteCommandNonQueryAsync(conn, true, (MySqlCommand)dbCommand);
        }

        public static Task<int> ExecuteCommandNonQueryAsync(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            return ExecuteCommandNonQueryAsync(mySqlTransaction.Connection, false, (MySqlCommand)dbCommand);
        }

        public static Task<int> ExecuteSqlNonQueryAsync(string connectionString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            MySqlCommand command = new MySqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString;

            return ExecuteCommandNonQueryAsync(conn, true, command);
        }

        public static Task<int> ExecuteSqlNonQueryAsync(MySqlTransaction mySqlTransaction, string sqlString)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlString;

            return ExecuteCommandNonQueryAsync(mySqlTransaction.Connection, false, command);
        }

        private static async Task<int> ExecuteCommandNonQueryAsync(MySqlConnection conn, bool isOwnedConnection, MySqlCommand command)
        {
            int rtInt = -1;

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                command.Connection = conn;

                rtInt = await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw ex;
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

        #region SP

        #region NonQuery

        public static Task<int> ExecuteSPNonQueryAsync(string connectString, string spName, IList<IDataParameter> parameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteSPNonQueryAsync(conn, null, true, spName, parameters);
        }

        public static Task<int> ExecuteSPNonQueryAsync(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> parameters)
        {
            return ExecuteSPNonQueryAsync(mySqlTransaction.Connection, mySqlTransaction, false, spName, parameters);
        }

        private static async Task<int> ExecuteSPNonQueryAsync(MySqlConnection conn, MySqlTransaction trans, bool isOwnedConnection, string spName, IList<IDataParameter> parameters)
        {
            int rtInt = -1;
            MySqlCommand command = new MySqlCommand();

            PrepareCommand(command, conn, trans, CommandType.StoredProcedure, spName, parameters);

            try
            {
                rtInt = await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (isOwnedConnection)
                {
                    conn.Close();
                }
            }

            command.Parameters.Clear();

            return rtInt;
        }

        #endregion

        #region scalar

        public static Task<object> ExecuteSPScalarAsync(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> parameters)
        {
            return ExecuteSPScalarAsync(mySqlTransaction.Connection, mySqlTransaction, false, spName, parameters);
        }

        public static Task<object> ExecuteSPScalarAsync(string connectString, string spName, IList<IDataParameter> parameters)
        {
            MySqlConnection conn = new MySqlConnection();
            return ExecuteSPScalarAsync(conn, null, true, spName, parameters);
        }

        private static async Task<object> ExecuteSPScalarAsync(MySqlConnection conn, MySqlTransaction trans, bool isOwnedConnection, string spName, IList<IDataParameter> parameters)
        {
            object rtObj = null;
            MySqlCommand command = new MySqlCommand();

            PrepareCommand(command, conn, trans, CommandType.StoredProcedure, spName, parameters);

            try
            {
                rtObj = await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (isOwnedConnection)
                {
                    conn.Close();
                }
            }
            command.Parameters.Clear();

            return rtObj;
        }

        #endregion

        #region reader

        public static Task<IDataReader> ExecuteSPReaderAsync(string connectString, string spName, IList<IDataParameter> dbParameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            conn.Open();

            return ExecuteSPReaderAsync(conn, null, true, spName, dbParameters);
        }

        public static Task<IDataReader> ExecuteSPReaderAsync(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> dbParameters)
        {
            return ExecuteSPReaderAsync(mySqlTransaction.Connection, mySqlTransaction, false, spName, dbParameters);
        }

        private static async Task<IDataReader> ExecuteSPReaderAsync(MySqlConnection connection, MySqlTransaction mySqlTransaction, bool isOwedConnection, string spName, IList<IDataParameter> dbParameters)
        {
            MySqlCommand command = new MySqlCommand();

            PrepareCommand(command, connection, mySqlTransaction, CommandType.StoredProcedure, spName, dbParameters);
            MySqlDataReader reader = null;

            try
            {
                if (isOwedConnection)
                {
                    reader = (MySqlDataReader)await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                }
                else
                {
                    reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                }
            }
            catch (Exception ex)
            {
                if (isOwedConnection)
                {
                    connection.Close();
                }

                if (reader != null)
                {
                    reader.Close();
                }

                throw ex;
            }

            command.Parameters.Clear();

            return reader;
        }

        #endregion

        #endregion

        #region DataTable

        public static Task<DataTable> ExecuteSqlDataTableAsync(string connectString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteSqlDataTableAsync(conn, sqlString, true);
        }

        public static Task<DataTable> ExecuteSqlDataTableAsync(MySqlTransaction mySqlTransaction, string sqlString)
        {
            if (mySqlTransaction == null)
            {
                throw new ArgumentNullException("ExecuteSqlReader方法不接收NULL参数");
            }

            return ExecuteSqlDataTableAsync(mySqlTransaction.Connection, sqlString, false);
        }

        private static Task<DataTable> ExecuteSqlDataTableAsync(MySqlConnection connection, string sqlString, bool isOwndConnection)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
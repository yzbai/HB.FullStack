using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data;

namespace HB.Infrastructure.MySQL
{
    /// <summary>
    /// 动态SQL和SP执行
    /// 具体执行步骤都要有异常捕捉，直接抛出给上一层
    /// </summary>
    public static partial class MySQLExecuter
    {
        #region private utility methods & constructors

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

        private static void PrepareCommand(MySqlCommand command, MySqlConnection connection, MySqlTransaction transaction, CommandType commandType, string commandText, IEnumerable<IDataParameter> commandParameters)
        {
            //if the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            //associate the connection with the command
            command.Connection = connection;

            //set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

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

            return;
        }

        #endregion

        #region Reader

        public static IDataReader ExecuteSqlReader(string connectionString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            MySqlCommand command = new MySqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandReader(conn, true, command);
        }

        public static IDataReader ExecuteSqlReader(MySqlTransaction mySqlTransaction, string sqlString)
        {
            MySqlCommand command = new MySqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandReader(mySqlTransaction.Connection, false, command);
        }

        public static IDataReader ExecuteSqlReader(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            return ExecuteCommandReader(mySqlTransaction.Connection, false, (MySqlCommand)dbCommand);
        }

        public static IDataReader ExecuteCommandReader(string connectString, IDbCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteCommandReader(conn, true, (MySqlCommand)dbCommand);
        }

        private static IDataReader ExecuteCommandReader(MySqlConnection connection, bool isOwnedConnection, MySqlCommand command)
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
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                else
                {
                    reader = command.ExecuteReader();
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

        #region Scalar

        public static object ExecuteCommandScalar(string connectString, IDbCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteCommandScalar(conn, true, (MySqlCommand)dbCommand);
        }

        public static object ExecuteCommandScalar(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            return ExecuteCommandScalar(mySqlTransaction.Connection, false, (MySqlCommand)dbCommand);
        }

        public static object ExecuteSqlScalar(string connectionString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            MySqlCommand command = new MySqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandScalar(conn, true, command);
        }

        public static object ExecuteSqlScalar(MySqlTransaction mySqlTransaction, string sqlString)
        {
            MySqlCommand command = new MySqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandScalar(mySqlTransaction.Connection, false, command);
        }

        private static object ExecuteCommandScalar(MySqlConnection connection, bool isOwnedConnection, MySqlCommand command)
        {
            object rtObj = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                command.Connection = connection;

                rtObj = command.ExecuteScalar();
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

        #region NonQuery

        public static int ExecuteCommandNonQuery(string connectString, IDbCommand dbCommand)
        {
            MySqlConnection conn = new MySqlConnection(connectString);

            return ExecuteCommandNonQuery(conn, true, (MySqlCommand)dbCommand);
        }

        public static int ExecuteCommandNonQuery(MySqlTransaction mySqlTransaction, IDbCommand dbCommand)
        {
            return ExecuteCommandNonQuery(mySqlTransaction.Connection, false, (MySqlCommand)dbCommand);
        }

        public static int ExecuteSqlNonQuery(string connectionString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            MySqlCommand command = new MySqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandNonQuery(conn, true, command);
        }

        public static int ExecuteSqlNonQuery(MySqlTransaction mySqlTransaction, string sqlString)
        {
            MySqlCommand command = new MySqlCommand
            {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandNonQuery(mySqlTransaction.Connection, false, command);
        }

        private static int ExecuteCommandNonQuery(MySqlConnection conn, bool isOwnedConnection, MySqlCommand command)
        {
            int rtInt = -1;

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    //TODO: 要用Polly来确保吗?
                    conn.Open();
                }

                command.Connection = conn;

                rtInt = command.ExecuteNonQuery();
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

        #region SP

        #region ExecuteSPNonQuery

        public static int ExecuteSPNonQuery(string connectString, string spName, IList<IDataParameter> parameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteSPNonQuery(conn, null, true, spName, parameters);
        }

        public static int ExecuteSPNonQuery(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> parameters)
        {
            return ExecuteSPNonQuery(mySqlTransaction.Connection, mySqlTransaction, false, spName, parameters);
        }

        private static int ExecuteSPNonQuery(MySqlConnection conn, MySqlTransaction trans, bool isOwnedConnection, string spName, IList<IDataParameter> parameters)
        {
            int rtInt = -1;
            MySqlCommand command = new MySqlCommand();

            PrepareCommand(command, conn, trans, CommandType.StoredProcedure, spName, parameters);

            try
            {
                rtInt = command.ExecuteNonQuery();
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

            command.Parameters.Clear();

            return rtInt;
        }

        #endregion

        #region ExecuteSPScalar

        public static object ExecuteSPScalar(string connectString, string spName, IList<IDataParameter> parameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteSPScalar(conn, null, true, spName, parameters);
        }

        public static object ExecuteSPScalar(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> parameters)
        {
            return ExecuteSPScalar(mySqlTransaction.Connection, mySqlTransaction, false, spName, parameters);
        }

        private static object ExecuteSPScalar(MySqlConnection conn, MySqlTransaction trans, bool isOwnedConnection, string spName, IList<IDataParameter> parameters)
        {
            object rtObj = null;
            MySqlCommand command = new MySqlCommand();

            PrepareCommand(command, conn, trans, CommandType.StoredProcedure, spName, parameters);

            try
            {
                rtObj = command.ExecuteScalar();
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
            command.Parameters.Clear();

            return rtObj;
        }

        #endregion

        #region ExecuteSPReader

        public static IDataReader ExecuteSPReader(string connectString, string spName, IList<IDataParameter> dbParameters)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            conn.Open();

            return ExecuteSPReader(conn, null, true, spName, dbParameters);
        }

        public static IDataReader ExecuteSPReader(MySqlTransaction mySqlTransaction, string spName, IList<IDataParameter> dbParameters)
        {
            return ExecuteSPReader(mySqlTransaction.Connection, mySqlTransaction, false, spName, dbParameters);
        }

        private static IDataReader ExecuteSPReader(MySqlConnection connection, MySqlTransaction mySqlTransaction, bool isOwedConnection, string spName, IList<IDataParameter> dbParameters)
        {
            MySqlCommand command = new MySqlCommand();

            PrepareCommand(command, connection, mySqlTransaction, CommandType.StoredProcedure, spName, dbParameters);
            MySqlDataReader reader = null;

            try
            {
                if (isOwedConnection)
                {
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                else
                {
                    reader = command.ExecuteReader();
                }
            }
            catch
            {
                if (isOwedConnection)
                {
                    connection.Close();
                }

                if (reader != null)
                {
                    reader.Close();
                }

                throw;
            }

            command.Parameters.Clear();

            return reader;
        }

        #endregion
        
        #endregion

        #region ExecuteSqlDataTable

        public static DataTable ExecuteSqlDataTable(string connectString, string sqlString)
        {
            MySqlConnection conn = new MySqlConnection(connectString);
            return ExecuteSqlDataTable(conn, sqlString, true);
        }

        public static DataTable ExecuteSqlDataTable(MySqlTransaction mySqlTransaction, string sqlString)
        {
            if (mySqlTransaction == null)
            {
                throw new ArgumentNullException(nameof(mySqlTransaction), "ExecuteSqlReader方法不接收NULL参数");
            }

            return ExecuteSqlDataTable(mySqlTransaction.Connection, sqlString, false);
        }

        private static DataTable ExecuteSqlDataTable(MySqlConnection connection, string sqlString, bool isOwndConnection)
        {

            throw new NotImplementedException();

            //DataTable table = new DataTable();
            
            //try
            //{
            //    if (connection.State != ConnectionState.Open)
            //    {
            //        connection.Open();
            //    }

            //    using (MySqlCommand command = connection.CreateCommand())
            //    {
            //        command.CommandText = sqlString;
            //        command.CommandType = CommandType.Text;

            //        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
            //        {
            //            adapter.Fill(table);
            //        }
            //    }

            //    return table;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    if (isOwndConnection)
            //    {
            //        connection.Close();
            //    }
            //}
        }

        #endregion
    }
}

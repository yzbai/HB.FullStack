using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace HB.Infrastructure.SQLite
{
    /// <summary>
    /// 动态SQL和SP执行
    /// 具体执行步骤都要有异常捕捉，直接抛出给上一层
    /// </summary>
    internal static partial class SQLiteExecuter
    {
        private static void AttachParameters(SQLiteCommand command, IEnumerable<IDataParameter> commandParameters)
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

        #region Comand Reader

        public static IDataReader ExecuteCommandReader(SQLiteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandReader(sqliteTransaction.Connection, false, (SQLiteCommand)dbCommand);
        }

        public static IDataReader ExecuteCommandReader(string connectString, IDbCommand dbCommand)
        {
            SQLiteConnection conn = new SQLiteConnection(connectString);
            return ExecuteCommandReader(conn, true, (SQLiteCommand)dbCommand);
        }

        private static IDataReader ExecuteCommandReader(SQLiteConnection connection, bool isOwnedConnection, SQLiteCommand command)
        {
            SQLiteDataReader reader = null;

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
                //TODO: 检查，整个解决方案，中所有的throw都要加log
                throw;
            }
        }

        #endregion

        #region Command Scalar

        public static object ExecuteCommandScalar(string connectString, IDbCommand dbCommand)
        {
            SQLiteConnection conn = new SQLiteConnection(connectString);
            return ExecuteCommandScalar(conn, true, (SQLiteCommand)dbCommand);
        }

        public static object ExecuteCommandScalar(SQLiteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandScalar(sqliteTransaction.Connection, false, (SQLiteCommand)dbCommand);
        }

        private static object ExecuteCommandScalar(SQLiteConnection connection, bool isOwnedConnection, SQLiteCommand command)
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

        #region Command NonQuery

        public static int ExecuteCommandNonQuery(string connectString, IDbCommand dbCommand)
        {
            SQLiteConnection conn = new SQLiteConnection(connectString);

            return ExecuteCommandNonQuery(conn, true, (SQLiteCommand)dbCommand);
        }

        public static int ExecuteCommandNonQuery(SQLiteTransaction sqliteTransaction, IDbCommand dbCommand)
        {
            dbCommand.Transaction = sqliteTransaction;
            return ExecuteCommandNonQuery(sqliteTransaction.Connection, false, (SQLiteCommand)dbCommand);
        }

        private static int ExecuteCommandNonQuery(SQLiteConnection conn, bool isOwnedConnection, SQLiteCommand command)
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

        #region SQL

        public static int ExecuteSqlNonQuery(string connectionString, string sqlString)
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);

            SQLiteCommand command = new SQLiteCommand {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandNonQuery(conn, true, command);
        }

        public static int ExecuteSqlNonQuery(SQLiteTransaction sqliteTransaction, string sqlString)
        {
            SQLiteCommand command = new SQLiteCommand {
                CommandType = CommandType.Text,
                CommandText = sqlString,
                Transaction = sqliteTransaction
            };

            return ExecuteCommandNonQuery(sqliteTransaction.Connection, false, command);
        }

        public static IDataReader ExecuteSqlReader(string connectionString, string sqlString)
        {
            //TODO: do we need a connection manager, that retry and makesure connection is avalible?
            SQLiteConnection conn = new SQLiteConnection(connectionString);

            SQLiteCommand command = new SQLiteCommand {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandReader(conn, true, command);
        }

        public static IDataReader ExecuteSqlReader(SQLiteTransaction sqliteTransaction, string sqlString)
        {
            SQLiteCommand command = new SQLiteCommand {
                CommandType = CommandType.Text,
                CommandText = sqlString,
                Transaction = sqliteTransaction
            };

            return ExecuteCommandReader(sqliteTransaction.Connection, false, command);
        }
        public static object ExecuteSqlScalar(string connectionString, string sqlString)
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);

            SQLiteCommand command = new SQLiteCommand {
                CommandType = CommandType.Text,
                CommandText = sqlString
            };

            return ExecuteCommandScalar(conn, true, command);
        }

        public static object ExecuteSqlScalar(SQLiteTransaction sqliteTransaction, string sqlString)
        {
            SQLiteCommand command = new SQLiteCommand {
                CommandType = CommandType.Text,
                CommandText = sqlString,
                Transaction = sqliteTransaction
            };

            return ExecuteCommandScalar(sqliteTransaction.Connection, false, command);
        }

        #endregion
    }
}

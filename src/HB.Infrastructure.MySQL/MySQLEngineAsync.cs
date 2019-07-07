using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Database.Engine;
using MySql.Data.MySqlClient;

namespace HB.Infrastructure.MySQL
{
    internal partial class MySQLEngine : IDatabaseEngineAsync
    {
        #region SP 能力

        public Task<IDataReader> ExecuteSPReaderAsync(IDbTransaction Transaction, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPReaderAsync(GetConnectionString(dbName, useMaster), spName, dbParameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPReaderAsync((MySqlTransaction)Transaction, spName, dbParameters);
            }
        }

        public Task<object> ExecuteSPScalarAsync(IDbTransaction Transaction, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPScalarAsync(GetConnectionString(dbName, useMaster), spName, parameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPScalarAsync((MySqlTransaction)Transaction, spName, parameters);
            }
        }

        public Task<int> ExecuteSPNonQueryAsync(IDbTransaction Transaction, string dbName, string spName, IList<IDataParameter> parameters)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPNonQueryAsync(GetConnectionString(dbName, true), spName, parameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPNonQueryAsync((MySqlTransaction)Transaction, spName, parameters);
            }
        }

        #endregion

        #region Command 能力

        public Task<int> ExecuteCommandNonQueryAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandNonQueryAsync(GetConnectionString(dbName, true), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandNonQueryAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        public Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandReaderAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandReaderAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        public Task<object> ExecuteCommandScalarAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandScalarAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandScalarAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        #endregion

        #region 事务

        public async Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            MySqlConnection conn = new MySqlConnection(GetConnectionString(dbName, true));
            await conn.OpenAsync();

            return await conn.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
        }

        public Task CommitAsync(IDbTransaction transaction)
        {
            MySqlTransaction mySqlTransaction = transaction as MySqlTransaction;

            return mySqlTransaction.CommitAsync();
        }

        public Task RollbackAsync(IDbTransaction transaction)
        {
            MySqlTransaction mySqlTransaction = transaction as MySqlTransaction;

            return mySqlTransaction.RollbackAsync();
        }



        #endregion

        public Task<bool> IsTableExistAsync(string tableName)
        {
            throw new NotImplementedException();
        }
    }
}

//#region SQL 能力

//public Task<IDataReader> ExecuteSqlReaderAsync(IDbTransaction Transaction, string dbName, string SQL, bool useMaster = false)
//{
//    if (Transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlReaderAsync(GetConnectionString(dbName, useMaster), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlReaderAsync((MySqlTransaction)Transaction, SQL);
//    }
//}

//public Task<object> ExecuteSqlScalarAsync(IDbTransaction Transaction, string dbName, string SQL, bool useMaster = false)
//{
//    if (Transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlScalarAsync(GetConnectionString(dbName, useMaster), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlScalarAsync((MySqlTransaction)Transaction, SQL);
//    }
//}

//public Task<int> ExecuteSqlNonQueryAsync(IDbTransaction Transaction, string dbName, string SQL)
//{
//    if (Transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlNonQueryAsync(GetConnectionString(dbName, true), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlNonQueryAsync((MySqlTransaction)Transaction, SQL);
//    }
//}

//public Task<DataTable> ExecuteSqlDataTableAsync(IDbTransaction transaction, string dbName, string SQL)
//{
//    if (transaction == null)
//    {
//        return MySQLExecuter.ExecuteSqlDataTableAsync(GetConnectionString(dbName, true), SQL);
//    }
//    else
//    {
//        return MySQLExecuter.ExecuteSqlDataTableAsync((MySqlTransaction)transaction, SQL);
//    }
//}

//#endregion

using HB.Framework.Database.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.SQLite
{
    internal partial class SQLiteEngine : IDatabaseEngineAsync
    {
        #region 事务

        public async Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            SQLiteConnection conn = new SQLiteConnection(GetConnectionString(dbName, true));
            await conn.OpenAsync();

            return conn.BeginTransaction(isolationLevel);
        }

        public Task CommitAsync(IDbTransaction transaction)
        {
            SQLiteTransaction sqliteTransaction = transaction as SQLiteTransaction;

            sqliteTransaction.Commit();

            return Task.FromResult(0);
        }

        public Task RollbackAsync(IDbTransaction transaction)
        {
            SQLiteTransaction sqliteTransaction = transaction as SQLiteTransaction;

            sqliteTransaction.Rollback();

            return Task.FromResult(0);
        }

        #endregion

        #region Command 能力

        public Task<int> ExecuteCommandNonQueryAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand)
        {
            if (Transaction == null)
            {
                return SQLiteExecuter.ExecuteCommandNonQueryAsync(GetConnectionString(dbName, true), dbCommand);
            }
            else
            {
                return SQLiteExecuter.ExecuteCommandNonQueryAsync((SQLiteTransaction)Transaction, dbCommand);
            }
        }

        public Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return SQLiteExecuter.ExecuteCommandReaderAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return SQLiteExecuter.ExecuteCommandReaderAsync((SQLiteTransaction)Transaction, dbCommand);
            }
        }

        public Task<object> ExecuteCommandScalarAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return SQLiteExecuter.ExecuteCommandScalarAsync(GetConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return SQLiteExecuter.ExecuteCommandScalarAsync((SQLiteTransaction)Transaction, dbCommand);
            }
        }

        #endregion

        #region SP
        public Task<int> ExecuteSPNonQueryAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<IDataReader> ExecuteSPReaderAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster)
        {
            throw new NotImplementedException();
        }

        public Task<object> ExecuteSPScalarAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}

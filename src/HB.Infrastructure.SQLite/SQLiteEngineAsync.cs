using HB.Framework.Database.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.SQLite
{
    internal partial class SQLiteEngine : IDatabaseEngineAsync
    {
        public Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteCommandNonQueryAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand)
        {
            throw new NotImplementedException();
        }

        public Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster)
        {
            throw new NotImplementedException();
        }

        public Task<object> ExecuteCommandScalarAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster)
        {
            throw new NotImplementedException();
        }

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

        public Task RollbackAsync(IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}

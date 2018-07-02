using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Database.Engine;
using MySql.Data.MySqlClient;

namespace HB.Infrastructure.MySQL
{
    public partial class MySQLEngine : IDatabaseEngineAsync
    {

        #region SP 能力

        public Task<IDataReader> ExecuteSPReaderAsync(IDbTransaction Transaction, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSPReaderAsync(getConnectionString(dbName, useMaster), spName, dbParameters);
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
                return MySQLExecuter.ExecuteSPScalarAsync(getConnectionString(dbName, useMaster), spName, parameters);
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
                return MySQLExecuter.ExecuteSPNonQueryAsync(getConnectionString(dbName, true), spName, parameters);
            }
            else
            {
                return MySQLExecuter.ExecuteSPNonQueryAsync((MySqlTransaction)Transaction, spName, parameters);
            }
        }

        #endregion

        #region SQL 能力

        public Task<IDataReader> ExecuteSqlReaderAsync(IDbTransaction Transaction, string dbName, string SQL, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSqlReaderAsync(getConnectionString(dbName, useMaster), SQL);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlReaderAsync((MySqlTransaction)Transaction, SQL);
            }
        }

        public Task<object> ExecuteSqlScalarAsync(IDbTransaction Transaction, string dbName, string SQL, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSqlScalarAsync(getConnectionString(dbName, useMaster), SQL);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlScalarAsync((MySqlTransaction)Transaction, SQL);
            }
        }

        public Task<int> ExecuteSqlNonQueryAsync(IDbTransaction Transaction, string dbName, string SQL)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteSqlNonQueryAsync(getConnectionString(dbName, true), SQL);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlNonQueryAsync((MySqlTransaction)Transaction, SQL);
            }
        }

        public Task<DataTable> ExecuteSqlDataTableAsync(IDbTransaction transaction, string dbName, string SQL)
        {
            if (transaction == null)
            {
                return MySQLExecuter.ExecuteSqlDataTableAsync(getConnectionString(dbName, true), SQL);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlDataTableAsync((MySqlTransaction)transaction, SQL);
            }
        }

        #endregion

        #region Command 能力

        public Task<int> ExecuteCommandNonQueryAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandNonQueryAsync(getConnectionString(dbName, true), dbCommand);
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
                return MySQLExecuter.ExecuteCommandReaderAsync(getConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteSqlReaderAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        public Task<object> ExecuteCommandScalarAsync(IDbTransaction Transaction, string dbName, IDbCommand dbCommand, bool useMaster = false)
        {
            if (Transaction == null)
            {
                return MySQLExecuter.ExecuteCommandScalarAsync(getConnectionString(dbName, useMaster), dbCommand);
            }
            else
            {
                return MySQLExecuter.ExecuteCommandScalarAsync((MySqlTransaction)Transaction, dbCommand);
            }
        }

        #endregion
    }
}

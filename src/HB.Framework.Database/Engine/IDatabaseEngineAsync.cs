using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

//
namespace HB.Framework.Database.Engine
{
    /// <summary>
    /// 数据库接口,是对数据库能力的表达. 
    /// 多线程复用..
    /// </summary>
    public interface IDatabaseEngineAsync
    {
        #region SP执行功能

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        Task<IDataReader> ExecuteSPReaderAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster = false);

        Task<object> ExecuteSPScalarAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster = false);

        Task<int> ExecuteSPNonQueryAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters);

        #endregion

        #region SQL执行功能

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        Task<IDataReader> ExecuteSqlReaderAsync(IDbTransaction trans, string dbName, string SQL, bool useMaster = false);

        Task<object> ExecuteSqlScalarAsync(IDbTransaction trans, string dbName, string SQL, bool useMaster = false);

        Task<int> ExecuteSqlNonQueryAsync(IDbTransaction trans, string dbName, string SQL);

        Task<DataTable> ExecuteSqlDataTableAsync(IDbTransaction trans, string dbName, string SQL);

        #endregion

        #region Command执行功能

        Task<int> ExecuteCommandNonQueryAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand);

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster = false);

        Task<object> ExecuteCommandScalarAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster = false);

        #endregion
    }
}

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
        Task<IDataReader> ExecuteSPReaderAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster );

        Task<object> ExecuteSPScalarAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster );

        Task<int> ExecuteSPNonQueryAsync(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters);

        #endregion

        #region Command执行功能

        Task<int> ExecuteCommandNonQueryAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand);

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster );

        Task<object> ExecuteCommandScalarAsync(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster );

        #endregion

        #region 事务功能

        /// <summary>
        /// 创建 事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task CommitAsync(IDbTransaction transaction);

        Task RollbackAsync(IDbTransaction transaction);

        #endregion
    }
}

//#region SQL执行功能

///// <summary>
///// 使用后必须Dispose，必须使用using
///// </summary>
//Task<IDataReader> ExecuteSqlReaderAsync(IDbTransaction trans, string dbName, string SQL, bool useMaster );

//Task<object> ExecuteSqlScalarAsync(IDbTransaction trans, string dbName, string SQL, bool useMaster );

//Task<int> ExecuteSqlNonQueryAsync(IDbTransaction trans, string dbName, string SQL);

//Task<DataTable> ExecuteSqlDataTableAsync(IDbTransaction trans, string dbName, string SQL);

//#endregion
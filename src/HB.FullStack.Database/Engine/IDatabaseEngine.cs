#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

//
namespace HB.FullStack.Database.Engine
{
    /// <summary>
    /// 数据库接口,是对数据库能力的表达.
    /// 多线程复用..
    /// </summary>
    public interface IDatabaseEngine
    {
        #region 管理功能

        DatabaseCommonSettings DatabaseSettings { get; }

        DatabaseEngineType EngineType { get; }

        string FirstDefaultDatabaseName { get; }

        IEnumerable<string> GetDatabaseNames();

        #endregion 管理功能

        #region 创建功能

        //IDataParameter CreateParameter(string name, object value, DbType dbType);

        //IDataParameter CreateParameter(string name, object value);

        //IDbCommand CreateTextCommand(string commandText, IDataParameter[]? parameters = null);

        IDbCommand CreateTextCommand(string commandText, IList<KeyValuePair<string, object>>? parameterPairs = null);

        #endregion 创建功能

        #region SP执行功能

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        Task<Tuple<IDbCommand, IDataReader>> ExecuteSPReaderAsync(IDbTransaction? trans, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster);

        /// <exception cref="DatabaseException"></exception>
        Task<object> ExecuteSPScalarAsync(IDbTransaction? trans, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster);

        /// <exception cref="DatabaseException"></exception>
        Task<int> ExecuteSPNonQueryAsync(IDbTransaction? trans, string dbName, string spName, IList<IDataParameter> parameters);

        #endregion SP执行功能

        #region Command执行功能

        /// <exception cref="DatabaseException"></exception>
        Task<int> ExecuteCommandNonQueryAsync(IDbTransaction? trans, string dbName, IDbCommand dbCommand);

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        /// <exception cref="DatabaseException"></exception>
        Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction? trans, string dbName, IDbCommand dbCommand, bool useMaster);

        /// <exception cref="DatabaseException"></exception>
        Task<object> ExecuteCommandScalarAsync(IDbTransaction? trans, string dbName, IDbCommand dbCommand, bool useMaster);

        #endregion Command执行功能

        #region 事务功能

        /// <summary>
        /// 创建 事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel? isolationLevel = null);

        Task CommitAsync(IDbTransaction transaction);

        Task RollbackAsync(IDbTransaction transaction);

        #endregion 事务功能
    }
}
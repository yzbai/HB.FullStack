

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

        //DbSetting DatabaseSettings { get; }

        EngineType EngineType { get; }

        //string FirstDefaultDatabaseName { get; }

        //IEnumerable<string> DatabaseNames { get; }

        #endregion 管理功能


        #region Command执行功能

        
        Task<int> ExecuteCommandNonQueryAsync(string connectionString, EngineCommand engineCommand);

        Task<int> ExecuteCommandNonQueryAsync(IDbTransaction trans, EngineCommand engineCommand);

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        
        Task<IDataReader> ExecuteCommandReaderAsync(string connectionString, EngineCommand engineCommand);
        
        Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction trans, EngineCommand engineCommand);

        
        Task<object?> ExecuteCommandScalarAsync(string connectionString, EngineCommand engineCommand);

        Task<object?> ExecuteCommandScalarAsync(IDbTransaction trans, EngineCommand engineCommand);

        #endregion Command执行功能

        #region 事务功能

        Task<IDbTransaction> BeginTransactionAsync(string connectionString, IsolationLevel? isolationLevel = null);

        Task CommitAsync(IDbTransaction transaction);

        Task RollbackAsync(IDbTransaction transaction);

        #endregion 事务功能
    }
}
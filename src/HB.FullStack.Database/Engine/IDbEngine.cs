using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using HB.FullStack.Database.Config;

//
namespace HB.FullStack.Database.Engine
{
    /// <summary>
    /// 数据库接口,是对数据库能力的表达.
    /// 多线程复用..
    /// </summary>
    public interface IDbEngine
    {
        DbEngineType EngineType { get; }

        #region Command执行功能

        Task<int> ExecuteCommandNonQueryAsync(ConnectionString connectionString, DbEngineCommand engineCommand);

        Task<int> ExecuteCommandNonQueryAsync(IDbTransaction trans, DbEngineCommand engineCommand);

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>

        Task<IDataReader> ExecuteCommandReaderAsync(ConnectionString connectionString, DbEngineCommand engineCommand);

        Task<IDataReader> ExecuteCommandReaderAsync(IDbTransaction trans, DbEngineCommand engineCommand);


        Task<object?> ExecuteCommandScalarAsync(ConnectionString connectionString, DbEngineCommand engineCommand);

        Task<object?> ExecuteCommandScalarAsync(IDbTransaction trans, DbEngineCommand engineCommand);

        #endregion

        #region 事务功能

        Task<IDbTransaction> BeginTransactionAsync(ConnectionString connectionString, IsolationLevel? isolationLevel = null);

        Task CommitAsync(IDbTransaction transaction);

        Task RollbackAsync(IDbTransaction transaction);

        #endregion


    }
}
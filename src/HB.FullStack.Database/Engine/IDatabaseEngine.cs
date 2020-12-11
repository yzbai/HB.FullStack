#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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

        #endregion

        #region 创建功能
        /// <summary>
        /// 创建 参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        //IDataParameter CreateParameter(string name, object value, DbType dbType);
        /// <summary>
        /// 创建 参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IDataParameter CreateParameter(string name, object value);
        /// <summary>
        /// 创建 空白命令
        /// </summary>
        /// <returns></returns>
        IDbCommand CreateTextCommand(string commandText, IDataParameter[]? parameters = null);


        #endregion

        #region 方言表达

        /// <summary>
        /// 用于参数化的字符（@）,用于参数化查询
        /// </summary>
        string ParameterizedChar { get; }
        /// <summary>
        /// 用于引号化的字符(')，用于字符串
        /// </summary>
        string QuotedChar { get; }
        /// <summary>
        /// 用于专有化的字符（`）
        /// </summary>
        string ReservedChar { get; }


        /// <summary>
        /// 将名称引号化
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetQuotedStatement(string name);
        /// <summary>
        /// 将名称参数化
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetParameterizedStatement(string name);
        /// <summary>
        /// 将名称专有化
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetReservedStatement(string name);
        /// <summary>
        /// 获取类型对应的数据库类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        DbType GetDbType(Type type);
        /// <summary>
        /// 获取类型对应的数据库类型的表达，用于编写SQL语句
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetDbTypeStatement(Type type);
        /// <summary>
        /// 获取值对应的数据库值的表达，用于编写SQL语句
        /// 做安全过滤
        /// </summary>
        /// <param name="value">类的值</param>
        /// <returns>数据库类的值的表达</returns>
        [return: NotNullIfNotNull("value")]
        string? GetDbValueStatement(object? value, bool needQuoted);
        /// <summary>
        /// 类型对应的数据库类型的值是否需要引号化
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsValueNeedQuoted(Type type);

        #endregion

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

        #endregion

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

        #endregion

        #region 事务功能

        /// <summary>
        /// 创建 事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        Task<IDbTransaction> BeginTransactionAsync(string dbName, IsolationLevel? isolationLevel = null);

        Task CommitAsync(IDbTransaction transaction);

        Task RollbackAsync(IDbTransaction transaction);

        #endregion
    }
}
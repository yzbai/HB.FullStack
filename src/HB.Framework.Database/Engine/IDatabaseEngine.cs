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
    public interface IDatabaseEngine : IDatabaseEngineAsync
    {
        #region SP执行功能

        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="spName"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        IDataReader ExecuteSPReader(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> dbParameters, bool useMaster);

        object ExecuteSPScalar(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters, bool useMaster);

        int ExecuteSPNonQuery(IDbTransaction trans, string dbName, string spName, IList<IDataParameter> parameters);

        #endregion

        #region Command执行功能

        int ExecuteCommandNonQuery(IDbTransaction trans, string dbName, IDbCommand dbCommand);
        
        /// <summary>
        /// 使用后必须Dispose，必须使用using
        /// </summary>
        IDataReader ExecuteCommandReader(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster);

        object ExecuteCommandScalar(IDbTransaction trans, string dbName, IDbCommand dbCommand, bool useMaster);

        #endregion

        #region 创建功能
        /// <summary>
        /// 创建 参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        IDataParameter CreateParameter(string name, object value, DbType dbType);
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
        IDbCommand CreateEmptyCommand();

        /// <summary>
        /// 创建 事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        IDbTransaction CreateTransaction(string dbName, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

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
        string GetDbValueStatement(object value, bool needQuoted);
        /// <summary>
        /// 类型对应的数据库类型的值是否需要引号化
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsValueNeedQuoted(Type type);
        

        #endregion
    }
}
///// <summary>
///// 创建 空白DataTable
///// </summary>
///// <param name="tableName"></param>
///// <returns></returns>
////DataTable CreateEmptyDataTable(string dbName, string tableName);
//#region SQL执行功能 - Unsafe

///// <summary>
///// 使用后必须Dispose，必须使用using. 在MySql中，IDataReader.Close工作不正常。解决之前不要用
///// </summary>
//IDataReader ExecuteSqlReader(IDbTransaction trans, string dbName, string SQL, bool useMaster);

//object ExecuteSqlScalar(IDbTransaction trans, string dbName, string SQL, bool useMaster);

//int ExecuteSqlNonQuery(IDbTransaction trans, string dbName, string SQL);

//DataTable ExecuteSqlDataTable(IDbTransaction trans, string dbName, string SQL);

//#endregion
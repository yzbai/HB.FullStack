using System;
using System.Data;
using HB.Framework.Database.Entity;

namespace HB.Framework.Database.Transaction
{
    /// <summary>
    /// 对逻辑层基础设施搭建
    /// 多线程复用
    /// 不支持跨库事务，如遇到跨库需求，请考虑是否应该把数据库设计到一个库中。
    /// </summary>
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private IDatabase _database { get; set; }
        
        public DatabaseTransaction(IDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public DatabaseTransactionContext BeginTransaction<T>(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : DatabaseEntity
        {
            return new DatabaseTransactionContext() {
                Transaction = _database.CreateTransaction<T>(isolationLevel),
                Status = DatabaseTransactionStatus.InTransaction
            };
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit(DatabaseTransactionContext context)
        {
            if (context == null || context.Transaction == null)
            {
                throw new ArgumentException("can not be null", nameof(context));
            }

            if (context.Status != DatabaseTransactionStatus.InTransaction)
            {
                return;
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;
                context.Transaction.Commit();
                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Dispose();
                }

                context.Status = DatabaseTransactionStatus.Commited;
            }
            catch
            {
                context.Status = DatabaseTransactionStatus.Failed;
                throw;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback(DatabaseTransactionContext context)
        {
            if(context == null || context.Transaction == null)
            {
                throw new ArgumentException("can not be null", nameof(context));
            }

            if (context.Status != DatabaseTransactionStatus.InTransaction)
            {
                return;
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;
                context.Transaction.Rollback();
                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Dispose();
                }

                context.Status = DatabaseTransactionStatus.Rollbacked;
            }
            catch 
            {
                context.Status = DatabaseTransactionStatus.Failed;
                throw;
            }
        }
    }
}

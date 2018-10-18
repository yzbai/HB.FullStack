using System;
using System.Data;
using HB.Framework.Database.Entity;

namespace HB.Framework.Database
{
    /// <summary>
    /// 对逻辑层基础设施搭建
    /// 多线程复用
    /// 不支持跨库事务，如遇到跨库需求，请考虑是否应该把数据库设计到一个库中。
    /// </summary>
    public class BizWithDbTransaction
    {
        private IDatabase _database { get; set; }
        
        public BizWithDbTransaction(IDatabase database)
        {
            _database = database;
        }

        #region  数据库事务

        /// <summary>
        /// 开始事务
        /// </summary>
        protected DbTransactionContext BeginTransaction<T>(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : DatabaseEntity
        {
            return new DbTransactionContext() {
                Transaction = _database.CreateTransaction<T>(isolationLevel),
                Status = DbTransactionStatus.InTransaction
            };
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        protected void Commit(DbTransactionContext context)
        {
            if (context == null || context.Transaction == null)
            {
                throw new ArgumentException("can not be null", nameof(context));
            }

            if (context.Status != DbTransactionStatus.InTransaction)
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

                context.Status = DbTransactionStatus.Commited;
            }
            catch
            {
                context.Status = DbTransactionStatus.Failed;
                throw;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        protected void Rollback(DbTransactionContext context)
        {
            if(context == null || context.Transaction == null)
            {
                throw new ArgumentException("can not be null", nameof(context));
            }

            if (context.Status != DbTransactionStatus.InTransaction)
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

                context.Status = DbTransactionStatus.Rollbacked;
            }
            catch 
            {
                context.Status = DbTransactionStatus.Failed;
                throw;
            }
        }

        #endregion
    }
}

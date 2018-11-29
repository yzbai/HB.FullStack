using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.Framework.Database.Transaction
{
    public interface IDatabaseTransaction
    {
        /// <summary>
        /// 开始事务
        /// </summary>
        DatabaseTransactionContext BeginTransaction<T>(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : DatabaseEntity;

        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit(DatabaseTransactionContext context);

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback(DatabaseTransactionContext context);
    }
}

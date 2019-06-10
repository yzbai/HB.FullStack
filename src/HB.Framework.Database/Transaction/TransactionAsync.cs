using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Database.Transaction
{
    internal partial class Transaction : ITransaction
    {
        public async Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel isolationLevel) where T : DatabaseEntity
        {
            DatabaseEntityDef entityDef = entityDefFactory.GetDef<T>();

            IDbTransaction dbTransaction = await databaseEngine.BeginTransactionAsync(entityDef.DatabaseName, isolationLevel).ConfigureAwait(false);

            return new TransactionContext() {
                Transaction = dbTransaction,
                Status = TransactionStatus.InTransaction
            };
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public async Task CommitAsync(TransactionContext context)
        {
            if (context == null || context.Transaction == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Status != TransactionStatus.InTransaction)
            {
                throw new DatabaseException("use a already finished transactioncontenxt");
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;

                await databaseEngine.CommitAsync(context.Transaction).ConfigureAwait(false);
                //context.Transaction.Commit();

                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Dispose();
                }

                context.Status = TransactionStatus.Commited;
            }
            catch
            {
                context.Status = TransactionStatus.Failed;
                throw;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public async Task RollbackAsync(TransactionContext context)
        {
            if (context == null || context.Transaction == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Status != TransactionStatus.InTransaction)
            {
                throw new DatabaseException("use a already finished transactioncontenxt");
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;

                await databaseEngine.RollbackAsync(context.Transaction).ConfigureAwait(false);
                //context.Transaction.Rollback();

                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Dispose();
                }

                context.Status = TransactionStatus.Rollbacked;
            }
            catch
            {
                context.Status = TransactionStatus.Failed;
                throw;
            }
        }

    }
}

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Properties;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Database
{
    internal class DefaultTransaction : ITransaction
    {
        private readonly IDatabaseEngine _databaseEngine;
        private readonly IDatabaseEntityDefFactory _entityDefFactory;
        public DefaultTransaction(IDatabaseEngine datbaseEngine, IDatabaseEntityDefFactory databaseEntityDefFactory)
        {
            _databaseEngine = datbaseEngine;
            _entityDefFactory = databaseEntityDefFactory;
        }

        #region 事务

        public async Task<TransactionContext> BeginTransactionAsync(string databaseName, IsolationLevel? isolationLevel = null)
        {
            IDbTransaction dbTransaction = await _databaseEngine.BeginTransactionAsync(databaseName, isolationLevel).ConfigureAwait(false);

            return new TransactionContext(dbTransaction, TransactionStatus.InTransaction, this);
        }

        public Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : Entity
        {
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return BeginTransactionAsync(entityDef.DatabaseName!, isolationLevel);
        }

        public async Task CommitAsync(TransactionContext context)
        {
            //if (context == null || context.Transaction == null)
            //{
            //    throw new ArgumentNullException(nameof(context));
            //}

            if (context.Status == TransactionStatus.Commited)
            {
                return;
            }

            if (context.Status != TransactionStatus.InTransaction)
            {
                throw new DatabaseException(ErrorCode.DatabaseTransactionError, Resources.TransactionAlreadyFinishedMessage);
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;

                await _databaseEngine.CommitAsync(context.Transaction).ConfigureAwait(false);
                //context.Transaction.Commit();

                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

                context.Status = TransactionStatus.Commited;
            }
            catch
            {
                context.Status = TransactionStatus.Failed;
                throw;
            }
        }

        public async Task RollbackAsync(TransactionContext context)
        {
            //if (context == null || context.Transaction == null)
            //{
            //    throw new ArgumentNullException(nameof(context));
            //}

            if (context.Status == TransactionStatus.Rollbacked || context.Status == TransactionStatus.Commited)
            {
                return;
            }

            if (context.Status != TransactionStatus.InTransaction)
            {
                throw new DatabaseException(ErrorCode.DatabaseTransactionError, Resources.TransactionAlreadyFinishedMessage);
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;

                await _databaseEngine.RollbackAsync(context.Transaction).ConfigureAwait(false);
                //context.Transaction.Rollback();

                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }

                context.Status = TransactionStatus.Rollbacked;
            }
            catch
            {
                context.Status = TransactionStatus.Failed;
                throw;
            }
        }

        #endregion
    }
}

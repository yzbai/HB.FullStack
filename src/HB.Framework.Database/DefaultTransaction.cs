using HB.Framework.Common.Entities;
using HB.Framework.Database.Engine;
using HB.Framework.Database.Entities;
using HB.Framework.Database.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Database
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

        public async Task<TransactionContext> BeginTransactionAsync(string databaseName, IsolationLevel isolationLevel)
        {
            IDbTransaction dbTransaction = await _databaseEngine.BeginTransactionAsync(databaseName, isolationLevel).ConfigureAwait(false);

            return new TransactionContext(dbTransaction, TransactionStatus.InTransaction);
        }

        public Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel isolationLevel) where T : Entity
        {
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.IsTableModel)
            {
                throw new DatabaseException(ErrorCode.DatabaseNotATableModel, entityDef.EntityFullName);
            }

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

        public async Task RollbackAsync(TransactionContext context)
        {
            //if (context == null || context.Transaction == null)
            //{
            //    throw new ArgumentNullException(nameof(context));
            //}

            if (context.Status == TransactionStatus.Rollbacked)
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

        #endregion
    }
}

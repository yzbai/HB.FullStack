using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    public class DefaultTransaction : ITransaction
    {
        private readonly IDatabaseEngine _databaseEngine;
        private readonly IEntityDefFactory _entityDefFactory;

        public DefaultTransaction(IDatabaseEngine datbaseEngine, IEntityDefFactory entityDefFactory)
        {
            _databaseEngine = datbaseEngine;
            _entityDefFactory = entityDefFactory;
        }

        #region 事务

        public async Task<TransactionContext> BeginTransactionAsync(string databaseName, IsolationLevel? isolationLevel = null)
        {
            IDbTransaction dbTransaction = await _databaseEngine.BeginTransactionAsync(databaseName, isolationLevel).ConfigureAwait(false);

            return new TransactionContext(dbTransaction, TransactionStatus.InTransaction, this);
        }

        public Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : DatabaseEntity
        {
            EntityDef entityDef = _entityDefFactory.GetDef<T>()!;

            return BeginTransactionAsync(entityDef.DatabaseName!, isolationLevel);
        }

        public async Task CommitAsync(TransactionContext context, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = 0)
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
                throw DatabaseExceptions.TransactionError("AlreadyFinished", callerMemberName, callerLineNumber);
            }

            try
            {
                IDbConnection? conn = context.Transaction.Connection;

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

        public async Task RollbackAsync(TransactionContext context, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = 0)
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
                throw DatabaseExceptions.TransactionError("AlreadyFinished", callerMemberName, callerLineNumber);
            }

            try
            {
                IDbConnection? conn = context.Transaction.Connection;

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

        #endregion 事务
    }
}
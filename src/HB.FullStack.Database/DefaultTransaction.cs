using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database
{
    public class DefaultTransaction : ITransaction
    {
        private readonly IDbModelDefFactory _modelDefFactory;
        private readonly IDbSettingManager _dbManager;

        public DefaultTransaction(IDbModelDefFactory modelDefFactory, IDbSettingManager dbManager)
        {
            _modelDefFactory = modelDefFactory;
            _dbManager = dbManager;
        }

        #region 事务

        public async Task<TransactionContext> BeginTransactionAsync(string dbSchema, IsolationLevel? isolationLevel = null)
        {
            IDatabaseEngine engine = _dbManager.GetDatabaseEngine(dbSchema);
            var connectionString = _dbManager.GetConnectionString(dbSchema, true);

            IDbTransaction dbTransaction = await engine.BeginTransactionAsync(connectionString, isolationLevel).ConfigureAwait(false);

            return new TransactionContext(dbTransaction, TransactionStatus.InTransaction, this, engine);
        }

        public async Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : DbModel
        {
            DbModelDef? modelDef = _modelDefFactory.GetDef<T>();

            ThrowIf.Null(modelDef, $"{typeof(T).FullName} 没有 DbModelDef");

            IDatabaseEngine engine = _dbManager.GetDatabaseEngine(modelDef.EngineType);
            var connectionString = _dbManager.GetConnectionString(modelDef.DbSchema, true);


            IDbTransaction dbTransaction = await engine.BeginTransactionAsync(connectionString, isolationLevel).ConfigureAwait(false);

            return new TransactionContext(dbTransaction, TransactionStatus.InTransaction, this, engine);
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

                await context.DatabaseEngine.CommitAsync(context.Transaction).ConfigureAwait(false);
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

                await context.DatabaseEngine.RollbackAsync(context.Transaction).ConfigureAwait(false);
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
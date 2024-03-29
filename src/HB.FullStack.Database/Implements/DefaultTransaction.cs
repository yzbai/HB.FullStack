﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.Implements
{
    internal class DefaultTransaction : ITransaction
    {
        private readonly IDbModelDefFactory _modelDefFactory;
        private readonly IDbSchemaManager _dbManager;

        public DefaultTransaction(IDbModelDefFactory modelDefFactory, IDbSchemaManager dbManager)
        {
            _modelDefFactory = modelDefFactory;
            _dbManager = dbManager;
        }

        #region 事务

        public async Task<TransactionContext> BeginTransactionAsync(string dbSchemaName, IsolationLevel? isolationLevel = null)
        {
            ConnectionString connectionString = _dbManager.GetRequiredConnectionString(dbSchemaName, true);
            IDbEngine engine = _dbManager.GetDatabaseEngine(dbSchemaName);

            IDbTransaction dbTransaction = await engine.BeginTransactionAsync(connectionString, isolationLevel).ConfigureAwait(false);

            return new TransactionContext(dbTransaction, TransactionStatus.InTransaction, this, engine);
        }

        public async Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : DbModel
        {
            DbModelDef? modelDef = _modelDefFactory.GetDef<T>();

            ThrowIf.Null(modelDef, $"{typeof(T).FullName} 没有 DbModelDef");

            ConnectionString connectionString = _dbManager.GetRequiredConnectionString(modelDef.DbSchemaName, true);
            IDbEngine engine = _dbManager.GetDatabaseEngine(modelDef.EngineType);


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
                throw DbExceptions.TransactionError("AlreadyFinished", callerMemberName, callerLineNumber);
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
                throw DbExceptions.TransactionError("AlreadyFinished", callerMemberName, callerLineNumber);
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
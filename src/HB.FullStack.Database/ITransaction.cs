using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;


using HB.FullStack.Database.Def;

namespace HB.FullStack.Database
{
    public interface ITransaction
    {
        Task<TransactionContext> BeginTransactionAsync(string databaseName, IsolationLevel? isolationLevel = null);

        Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : DatabaseEntity;

        /// <exception cref="System.DatabaseException">Ignore.</exception>
        Task RollbackAsync(TransactionContext context);

        /// <exception cref="DatabaseException"></exception>
        Task CommitAsync(TransactionContext context);
    }

#if NETSTANDARD2_0
    public static class NetStandard2_0_Database_Extensions
    {
        public static ValueTask DisposeAsync(this DbConnection connection)
        {
            connection.Dispose();
            return default;
        }

        public static Task CommitAsync(this DbTransaction dbTransaction, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                dbTransaction.Commit();
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        public static Task RollbackAsync(this DbTransaction dbTransaction, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            try
            {
                dbTransaction.Rollback();
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }
    }
#endif
}
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database
{
    public interface ITransaction
    {
        Task<TransactionContext> BeginTransactionAsync(string databaseName, IsolationLevel? isolationLevel = null);

        Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : DbModel;

        Task RollbackAsync(TransactionContext context, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = 0);

        Task CommitAsync(TransactionContext context, [CallerMemberName] string? callerMemberName = null, [CallerLineNumber] int callerLineNumber = 0);
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
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return Task.FromException(e);
            }
        }

        public static Task RollbackAsync(this DbTransaction dbTransaction)
        {
            try
            {
                dbTransaction.Rollback();
                return Task.CompletedTask;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return Task.FromException(e);
            }
        }
    }
#endif
}
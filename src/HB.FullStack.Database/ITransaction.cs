using System;
using System.Data;
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
}
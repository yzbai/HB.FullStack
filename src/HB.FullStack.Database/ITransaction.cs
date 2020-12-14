using System.Data;
using System.Threading.Tasks;

using HB.FullStack.Common.Entities;

namespace HB.FullStack.Database
{
    public interface ITransaction
    {
        Task<TransactionContext> BeginTransactionAsync(string databaseName, IsolationLevel? isolationLevel = null);

        Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel? isolationLevel = null) where T : Entity;

        Task RollbackAsync(TransactionContext context);

        Task CommitAsync(TransactionContext context);
    }
}
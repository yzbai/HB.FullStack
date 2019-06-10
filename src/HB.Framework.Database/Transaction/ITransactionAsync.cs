using System.Data;
using System.Threading.Tasks;
using HB.Framework.Database.Entity;

namespace HB.Framework.Database.Transaction
{
    public interface ITransactionAsync
    {
        Task<TransactionContext> BeginTransactionAsync<T>(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : DatabaseEntity;
        Task CommitAsync(TransactionContext context);
        Task RollbackAsync(TransactionContext context);
    }
}
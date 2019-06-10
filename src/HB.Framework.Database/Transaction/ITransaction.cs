using System.Data;
using HB.Framework.Database.Entity;

namespace HB.Framework.Database.Transaction
{
    public interface ITransaction : ITransactionAsync
    {
        TransactionContext BeginTransaction<T>(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : DatabaseEntity;
        void Commit(TransactionContext context);
        void Rollback(TransactionContext context);
    }
}
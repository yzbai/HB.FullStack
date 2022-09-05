

using System.Data;
using System.Threading.Tasks;

namespace HB.FullStack.Database
{
    public class TransactionContext
    {
        private readonly ITransaction _transactionManager;

        public IDbTransaction Transaction { get; private set; }

        public TransactionStatus Status { get; set; }

        public TransactionContext(IDbTransaction transaction, TransactionStatus status, ITransaction transactionManager)
        {
            _transactionManager = transactionManager;
            Transaction = transaction;
            Status = status;
        }
        
        public Task CommitAsync()
        {
            return _transactionManager.CommitAsync(this);
        }

        public Task RollbackAsync()
        {
            return _transactionManager.RollbackAsync(this);
        }
    }
}
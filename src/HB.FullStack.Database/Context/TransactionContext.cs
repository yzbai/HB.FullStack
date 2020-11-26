#nullable enable

using System.Data;

namespace HB.FullStack.Database
{
    public class TransactionContext
    {
        public IDbTransaction Transaction { get; private set; }

        public TransactionStatus Status { get; set; }

        public TransactionContext(IDbTransaction transaction, TransactionStatus status)
        {
            Transaction = transaction;
            Status = status;
        }
    }
}

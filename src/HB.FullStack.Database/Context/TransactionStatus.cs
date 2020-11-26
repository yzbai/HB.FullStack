#nullable enable

namespace HB.FullStack.Database
{
    public enum TransactionStatus
    {
        InTransaction,
        Rollbacked,
        Commited,
        Failed
    }
}

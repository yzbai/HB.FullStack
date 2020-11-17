#nullable enable

namespace HB.Framework.Database
{
    public enum TransactionStatus
    {
        InTransaction,
        Rollbacked,
        Commited,
        Failed
    }
}

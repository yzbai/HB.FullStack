namespace HB.Framework.Database.Transaction
{
    public enum DatabaseTransactionStatus
    {
        InTransaction,
        Rollbacked,
        Commited,
        Failed
    }
}

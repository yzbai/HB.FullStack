namespace HB.FullStack.Database.DbModels
{
    public enum DbConflictCheckMethod
    {
        None = 0,
        Both = 1,
        OldNewValueCompareOnly = 2,
        TimestampOnly = 3
    }
}
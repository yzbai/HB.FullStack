namespace HB.Framework.DistributedLock
{
    public enum DistributedLockStatus
    {
        Waiting,
        Acquired,
        Expired,
        Failed,
        Disposed,
        Cancelled
    }
}

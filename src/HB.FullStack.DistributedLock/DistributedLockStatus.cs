namespace HB.FullStack.DistributedLock
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

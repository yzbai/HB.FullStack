namespace HB.FullStack.Lock.Distributed
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

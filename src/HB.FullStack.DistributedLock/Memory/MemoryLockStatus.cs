namespace HB.FullStack.Lock.Memory
{
    public enum MemoryLockStatus
    {
        Failed,
        Acquired,
        ResourceTypeSemaphoreExpired,
        Expired,
        Waiting,
        Disposed
    }
}

using System;

namespace HB.FullStack.Lock.Distributed
{
    public interface IDistributedLock : IDisposable, IAsyncDisposable
    {
        DistributedLockStatus Status { get; }

        bool IsAcquired { get; }

        int ExtendCount { get; }

    }
}

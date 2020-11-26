using System;

namespace HB.FullStack.DistributedLock
{
    public interface IDistributedLock : IDisposable, IAsyncDisposable
    {
        DistributedLockStatus Status { get; }

        bool IsAcquired { get; }

        int ExtendCount { get; }

    }
}

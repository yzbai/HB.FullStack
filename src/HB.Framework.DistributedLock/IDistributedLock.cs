using System;

namespace HB.Framework.DistributedLock
{
    public interface IDistributedLock : IDisposable, IAsyncDisposable
    {
        DistributedLockStatus Status { get; }

        bool IsAcquired { get; }

        int ExtendCount { get; }

    }
}

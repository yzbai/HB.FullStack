using System;
using System.Collections.Generic;

namespace HB.FullStack.Lock.Memory
{
    public interface IMemoryLock : IDisposable
    {
        bool IsAcquired { get; }
        string ResourceType { get; set; }
        MemoryLockStatus Status { get; set; }
        int ExtendCount { get; set; }
    }
}
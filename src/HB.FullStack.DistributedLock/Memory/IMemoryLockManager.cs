using System;
using System.Collections.Generic;
using System.Threading;

namespace HB.FullStack.Lock.Memory
{
    public interface IMemoryLockManager
    {
        IMemoryLock Lock(string resourceType, IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan? waitTime = null, CancellationToken cancellationToken = default);

        IMemoryLock Lock(string resourceType, string resource, TimeSpan expiryTime, TimeSpan? waitTime = null, CancellationToken cancellationToken = default)
        {
            return Lock(resourceType, new string[] { resource }, expiryTime, waitTime, cancellationToken);
        }
    }
}
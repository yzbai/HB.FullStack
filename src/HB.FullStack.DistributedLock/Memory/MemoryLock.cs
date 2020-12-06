using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HB.FullStack.Lock.Memory
{
    public class MemoryLock : IMemoryLock
    {
        private const string _prefix = "_ml_";

        public MemoryLock(MemoryLockManager lockManager, string resourceType, IEnumerable<string> resources, TimeSpan expiryTime)
        {
            LockManager = lockManager;
            ResourceType = resourceType;
            ExpiryTime = expiryTime;
            Status = MemoryLockStatus.Waiting;

            ResourceKeys = resources.Select(r => _prefix + resourceType + r).ToList();

            ResourceValues = new List<string>(ResourceKeys.Count());

            for (int i = 0; i < ResourceKeys.Count(); ++i)
            {
                ResourceValues.Add(SecurityUtil.CreateUniqueToken());
            }
        }

        public MemoryLockManager LockManager { get; set; } = null!;

        public string ResourceType { get; set; } = null!;

        public IList<string> ResourceKeys { get; set; } = null!;

        public IList<string> ResourceValues { get; set; }

        public TimeSpan ExpiryTime { get; set; }

        public MemoryLockStatus Status { get; set; }

        public bool IsAcquired => Status == MemoryLockStatus.Acquired;

        internal Timer? KeepAliveTimer { get; set; }

        public int ExtendCount { get; set; }

        public void Dispose()
        {
            LockManager.Unlock(this);
        }

        public void StopKeepAliveTimer()
        {
            LockManager.StopKeepAliveTimer(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HB.FullStack.Lock.Memory
{
    public class MemoryLock : IMemoryLock
    {
        private const string PREFIX = "_ml_";

        public MemoryLock(MemoryLockManager lockManager, string resourceType, IEnumerable<string> resources, TimeSpan expiryTime)
        {
            LockManager = lockManager;
            ResourceType = resourceType;
            ExpiryTime = expiryTime;
            Status = MemoryLockStatus.Waiting;

            ResourceKeys = resources.Select(r => PREFIX + resourceType + r).ToList();

            ResourceValues = new List<string>(ResourceKeys.Count);

            for (int i = 0; i < ResourceKeys.Count; ++i)
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

        //public void Dispose()
        //{
        //    LockManager.Unlock(this);
        //}

        public void StopKeepAliveTimer()
        {
            MemoryLockManager.StopKeepAliveTimer(this);
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                    LockManager.Unlock(this);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MemoryLock()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

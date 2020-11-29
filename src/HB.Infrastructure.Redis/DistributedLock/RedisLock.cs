using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Lock;
using HB.FullStack.Lock.Distributed;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.DistributedLock
{
    public class RedisLock : IDistributedLock
    {
        private const string _prefix = "HBRL_";
        internal IEnumerable<string> Resources { get; set; }

        internal IEnumerable<string> ResourceValues { get; set; }
        internal TimeSpan ExpiryTime { get; set; }
        internal TimeSpan WaitTime { get; set; }
        internal TimeSpan RetryTime { get; set; }
        internal CancellationToken? CancellationToken { get; set; }

        internal SingleRedisDistributedLockOptions Options { get; set; }

        internal Timer? KeepAliveTimer { get; set; }

        internal RedisLock(SingleRedisDistributedLockOptions options, IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime, CancellationToken? cancellationToken)
        {
            Options = options;
            ExpiryTime = expiryTime;
            WaitTime = waitTime;
            RetryTime = retryTime;
            CancellationToken = cancellationToken;

            List<string> keyResources = new List<string>();

            foreach (string item in resources)
            {
                keyResources.Add(_prefix + Options.ApplicationName + item);
            }

            Resources = keyResources;

            List<string> resourceValues = new List<string>();

            for (int i = 0; i < Resources.Count(); ++i)
            {
                resourceValues.Add(SecurityUtil.CreateUniqueToken());
            }

            ResourceValues = resourceValues;
        }

        public DistributedLockStatus Status { get; set; }

        public bool IsAcquired => Status == DistributedLockStatus.Acquired;

        public int ExtendCount { get; set; }


        #region Disposable Pattern

        private bool _disposedValue;

        private object _lockObj = new object();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (KeepAliveTimer != null)
                    {
                        lock (_lockObj)
                        {
                            if (KeepAliveTimer != null)
                            {
                                KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                                KeepAliveTimer.Dispose();
                                KeepAliveTimer = null;
                            }
                        }
                    }

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null

                SingleRedisDistributedLockManager.ReleaseResourceAsync(this).Fire();

                Resources = null!;
                ResourceValues = null!;
                _disposedValue = true;

                Status = DistributedLockStatus.Disposed;
            }
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    StopKeepAliveTimer();

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null

                await SingleRedisDistributedLockManager.ReleaseResourceAsync(this).ConfigureAwait(false);

                Resources = null!;
                ResourceValues = null!;
                _disposedValue = true;

                Status = DistributedLockStatus.Disposed;
            }
        }

        public void StopKeepAliveTimer()
        {
            if (KeepAliveTimer != null)
            {
                lock (_lockObj)
                {
                    if (KeepAliveTimer != null)
                    {
                        KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        KeepAliveTimer.Dispose();
                        KeepAliveTimer = null;
                    }
                }
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RedisLock()
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

        public ValueTask DisposeAsync()
        {
            return DisposeAsync(true);
        }
        #endregion
    }
}

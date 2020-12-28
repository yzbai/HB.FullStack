using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Lock;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.DistributedLock
{
    public class RedisLock : IDistributedLock
    {
        private const string _prefix = "HBRL_";

        private readonly ILogger _logger;

        internal IEnumerable<string> Resources { get; set; }

        internal IEnumerable<string> ResourceValues { get; set; }
        internal TimeSpan ExpiryTime { get; set; }
        internal TimeSpan WaitTime { get; set; }
        internal TimeSpan RetryTime { get; set; }
        internal CancellationToken? CancellationToken { get; set; }

        internal SingleRedisDistributedLockOptions Options { get; set; }

        internal Timer? KeepAliveTimer { get; set; }

        internal object StopKeepAliveTimerLockObj { get; private set; } = new object();

        internal RedisLock(SingleRedisDistributedLockOptions options, ILogger logger, IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime, CancellationToken? cancellationToken)
        {
            Options = options;
            _logger = logger;
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



        protected virtual void Dispose(bool disposing)
        {
            _logger.LogDebug($"锁开始Dispose，Resources:{Resources.ToJoinedString(",")}");

            if (!_disposedValue)
            {
                if (disposing)
                {
                    SingleRedisDistributedLockManager.ReleaseResourceAsync(this, _logger).Fire();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null



                //Resources = null!;
                ResourceValues = null!;
                _disposedValue = true;

                Status = DistributedLockStatus.Disposed;
            }
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            _logger.LogDebug($"锁开始Dispose，Resources:{Resources.ToJoinedString(",")}");

            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    await SingleRedisDistributedLockManager.ReleaseResourceAsync(this, _logger).ConfigureAwait(false);

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null


                //Resources = null!;
                ResourceValues = null!;
                _disposedValue = true;

                Status = DistributedLockStatus.Disposed;
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Lock.Memory
{
    public class MemoryLockManager : IMemoryLockManager
    {
        private readonly MemoryLockOptions _options;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// resourceType:Semaphore
        /// 对资源类型上锁，同一时间只能操作一种资源类型
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _resourceTypeSemaphoreDict = new ConcurrentDictionary<string, SemaphoreSlim>();

        public MemoryLockManager(IOptions<MemoryLockOptions> options, IMemoryCache memoryCache)
        {
            _options = options.Value;
            _memoryCache = memoryCache;
        }

        public IMemoryLock Lock(string resourceType, IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan? waitTime = null, CancellationToken cancellationToken = default)
        {
            if (waitTime == null)
            {
                waitTime = _options.DefaultWaitTime;
            }

            MemoryLock memoryLock = new MemoryLock(this, resourceType, resources, expiryTime);

            SemaphoreSlim semaphore = _resourceTypeSemaphoreDict.GetOrAdd(resourceType, new SemaphoreSlim(1, 1));

            Stopwatch stopwatch = Stopwatch.StartNew();

            if (semaphore.Wait(waitTime.Value, cancellationToken))
            {
                try
                {
                    if (waitTime != TimeSpan.Zero)
                    {
                        //可以等待
                        while (!memoryLock.IsAcquired && stopwatch.Elapsed <= waitTime)
                        {
                            memoryLock.Status = TryAcquireResources(memoryLock);

                            if (!memoryLock.IsAcquired)
                            {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                                Task.Delay(_options.DefaultRetryIntervalMilliseconds, cancellationToken).Wait(cancellationToken);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                            }
                        }

                        if (!memoryLock.IsAcquired)
                        {
                            memoryLock.Status = MemoryLockStatus.Expired;
                        }
                    }
                    else
                    {
                        //不等待
                        memoryLock.Status = TryAcquireResources(memoryLock);
                    }
                }
                finally
                {
                    semaphore.Release();
                }

                if (memoryLock.IsAcquired)
                {
                    StartAutoExtendTimer(memoryLock);
                }
            }
            else
            {
                memoryLock.Status = MemoryLockStatus.ResourceTypeSemaphoreExpired;
            }

            stopwatch.Stop();

            return memoryLock;
        }

        private void StartAutoExtendTimer(MemoryLock memoryLock)
        {
            long interval = (long)memoryLock.ExpiryTime.TotalMilliseconds / 2;

            memoryLock.KeepAliveTimer = new Timer(
                state => { ExtendLockLifetime(memoryLock); },
                null,
                interval,
                interval);
        }

        private void ExtendLockLifetime(MemoryLock memoryLock)
        {
            memoryLock.ExtendCount++;

            long now = TimeUtil.UtcNowUnixTimeMilliseconds;

            for (int i = 0; i < memoryLock.ResourceKeys.Count; ++i)
            {
                if (_memoryCache.TryGetValue(memoryLock.ResourceKeys[i], out MemoryLockResourceExpiryInfo storedInfo) && storedInfo.ResourceValue == memoryLock.ResourceValues[i])
                {
                    _memoryCache.Set(
                        memoryLock.ResourceKeys[i],
                        new MemoryLockResourceExpiryInfo(now, (long)memoryLock.ExpiryTime.TotalMilliseconds, memoryLock.ResourceValues[i]),
                        memoryLock.ExpiryTime);
                }
            }
        }

        private MemoryLockStatus TryAcquireResources(MemoryLock memoryLock)
        {
            //这里可以不加锁，因为已经上了资源锁

            long now = TimeUtil.UtcNowUnixTimeMilliseconds;

            for (int i = 0; i < memoryLock.ResourceKeys.Count; ++i)
            {
                string resourceKey = memoryLock.ResourceKeys[i];

                //不存在 or 存在但已经过期，因为MemoryCache并不能即使的清除过期，所以也得检查
                if (!_memoryCache.TryGetValue(resourceKey, out MemoryLockResourceExpiryInfo storedInfo) || now - storedInfo.Timestamp >= storedInfo.ExpiryMilliseconds)
                {
                    _memoryCache.Set(
                        resourceKey,
                        new MemoryLockResourceExpiryInfo(now, (long)memoryLock.ExpiryTime.TotalMilliseconds, memoryLock.ResourceValues[i]),
                        memoryLock.ExpiryTime);

                    continue;
                }
                else
                {
                    //存在且不过期
                    return MemoryLockStatus.Failed;
                }
            }

            return MemoryLockStatus.Acquired;
        }

        internal void Unlock(MemoryLock memoryLock)
        {
            //这里不加锁也没关系，因为不影响上面Acquire时的三个判断
            //比如：判断“存在但过期”，这里删掉了。不影响

            if (memoryLock.KeepAliveTimer != null)
            {
                memoryLock.KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                memoryLock.KeepAliveTimer.Dispose();
                memoryLock.KeepAliveTimer = null;
            }

            for (int i = 0; i < memoryLock.ResourceKeys.Count; ++i)
            {
                if (_memoryCache.TryGetValue(memoryLock.ResourceKeys[i], out MemoryLockResourceExpiryInfo storedInfo) && storedInfo.ResourceValue == memoryLock.ResourceValues[i])
                {
                    _memoryCache.Remove(memoryLock.ResourceKeys[i]);
                }
            }

            memoryLock.Status = MemoryLockStatus.Disposed;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.DistributedLock
{
    public interface IDistributedLockManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="expiryTime">对资源的最大占用时间，应该大于TimeSpan.Zero, null表示使用默认</param>
        /// <param name="waitTime">如果资源被占用，你愿意等多久，TimeSpan.Zero表明不愿意等。null表示使用默认等待时间</param>
        /// <param name="retryInterval">等待时不断尝试获取资源 的 等待间隔，应该大于TimeSpan.Zero, null 表示使用默认时间</param>
        /// <returns></returns>
        Task<IDistributedLock> LockAsync(IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan? waitTime = null, TimeSpan? retryInterval = null, CancellationToken? cancellationToken = null);
    }

    public interface IDistributedLock : IDisposable, IAsyncDisposable
    {
        DistributedLockStatus Status { get; }

        bool IsAcquired { get; }

        int ExtendCount { get; }

    }

    public enum DistributedLockStatus
    {
        Waiting,
        Acquired,
        Expired,
        Failed,
        Disposed,
        Cancelled
    }
}

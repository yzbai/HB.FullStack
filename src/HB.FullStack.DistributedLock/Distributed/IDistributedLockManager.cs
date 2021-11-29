using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Lock.Distributed
{
    public interface IDistributedLockManager
    {
        /// <summary>
        /// 尝试独占资源，如果不行则等待
        /// </summary>
        /// <param name="resources">资源</param>
        /// <param name="expiryTime">对资源的最大占用时间，应该大于TimeSpan.Zero, null表示使用默认</param>
        /// <param name="waitTime">如果资源被占用，你愿意等多久，TimeSpan.Zero表明不愿意等。null表示使用默认等待时间</param>
        /// <param name="retryInterval">等待时不断尝试获取资源 的 等待间隔，应该大于TimeSpan.Zero, null 表示使用默认时间</param>
        /// <param name="notUnlockWhenDispose">释放锁时，是否解除索，还是等他自己慢慢过期</param>
        /// <returns></returns>
        Task<IDistributedLock> LockAsync(IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan? waitTime = null, TimeSpan? retryInterval = null, bool notUnlockWhenDispose = false, CancellationToken? cancellationToken = null);

        /// <summary>
        /// 尝试独占资源，如果不行则等待
        /// </summary>
        /// <param name="resource">资源</param>
        /// <param name="expiryTime">对资源的最大占用时间，应该大于TimeSpan.Zero, null表示使用默认</param>
        /// <param name="waitTime">如果资源被占用，你愿意等多久，TimeSpan.Zero表明不愿意等。null表示使用默认等待时间</param>
        /// <param name="retryInterval">等待时不断尝试获取资源 的 等待间隔，应该大于TimeSpan.Zero, null 表示使用默认时间</param>
        /// <param name="notUnlockWhenDispose">释放锁时，是否解除索，还是等他自己慢慢过期</param>
        /// <returns></returns>
        Task<IDistributedLock> LockAsync(string resource, TimeSpan expiryTime, TimeSpan? waitTime = null, TimeSpan? retryInterval = null, bool notUnlockWhenDispose = false, CancellationToken? cancellationToken = null)
        {
            return LockAsync(new string[] { resource }, expiryTime, waitTime, retryInterval, notUnlockWhenDispose, cancellationToken);
        }

        /// <summary>
        /// 尝试独占资源，如果不行，立刻返回，不等待
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="expiryTime"></param>
        /// <param name="notUnlockWhenDispose"></param>
        /// <param name="notUnlockWhenDispose">释放锁时，是否解除索，还是等他自己慢慢过期</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IDistributedLock> NoWaitLockAsync(string resource, TimeSpan expiryTime, bool notUnlockWhenDispose = false, CancellationToken? cancellationToken = null)
        {
            return LockAsync(new string[] { resource }, expiryTime, TimeSpan.Zero, null, notUnlockWhenDispose, cancellationToken);
        }
    }
}

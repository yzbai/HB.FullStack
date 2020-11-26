using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.FullStack.Common.Entities;
using HB.FullStack.DistributedLock;

namespace HB.FullStack.DistributedLock
{
    public static class DistributedLockExtensions
    {
        public static Task<IDistributedLock> LockEntityAsync<TEntity>(this IDistributedLockManager distributedLockManager, TEntity entity, TimeSpan expiryTime, TimeSpan? waitTime = null, TimeSpan? retryInterval = null, CancellationToken? cancellationToken = null) where TEntity : Entity, new()
        {
            return distributedLockManager.LockAsync(new string[] { entity.Guid }, expiryTime, waitTime, retryInterval, cancellationToken);
        }

        public static Task<IDistributedLock> LockEntitiesAsync<TEntity>(this IDistributedLockManager distributedLockManager, IEnumerable<TEntity> entities, TimeSpan expiryTime, TimeSpan? waitTime = null, TimeSpan? retryInterval = null, CancellationToken? cancellationToken = null) where TEntity : Entity, new()
        {
            return distributedLockManager.LockAsync(entities.Select(e => e.Guid), expiryTime, waitTime, retryInterval, cancellationToken);
        }
    }
}

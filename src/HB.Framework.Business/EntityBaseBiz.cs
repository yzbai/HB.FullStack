using HB.Framework.Cache;
using HB.Framework.Common;
using HB.Framework.Common.Entities;
using HB.Framework.Database;
using HB.Framework.DistributedLock;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Business
{
    /// <summary>
    /// 这里体现缓存的策略：
    /// 所有的关于TEntity的update\delete都要经过这里，保证缓存的Invalidation正确
    /// Service里面不要出现_database.Update / _database.Delete,全部由Biz来调用
    /// Cache Strategy : Cache Aside
    /// Invalidation Strategy: delete from cache when database update/delete, add to cache when database add
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class EntityBaseBiz<TEntity> where TEntity : Entity, new()
    {
        public static readonly TimeSpan OccupiedTime = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan PatienceTime = TimeSpan.FromSeconds(5);

        protected readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();
        protected readonly ILogger _logger;
        protected readonly ICache _cache;
        protected readonly IDistributedLockManager _lockManager;
        protected readonly IDatabase _database;

        public EntityBaseBiz(ILogger logger, IDatabaseReader databaseReader, ICache cache, IDistributedLockManager lockManager)
        {
            _logger = logger;
            _cache = cache;
            _lockManager = lockManager;

            //Dirty trick
            _database = (IDatabase)databaseReader;
        }
        https://mp.weixin.qq.com/s?__biz=MjM5ODYxMDA5OQ==&mid=2651961341&idx=1&sn=e27916b8e96bd771c72c055f1f53e5be&chksm=bd2d02218a5a8b37ecffd78d20b65501645ac07c7ba2eb65b7e501a3eb9de023febe63bfdb36&scene=21#wechat_redirect

            invalidate缓存时，应该在数据库update完成后






        #region Events

        public event AsyncEventHandler<TEntity> EntityUpdating
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity> EntityUpdated
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity?> EntityUpdateFailed
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity> EntityAdding
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity> EntityAdded
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity> EntityDeleting
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity> EntityDeleted
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        protected virtual Task OnEntityUpdatingAsync(TEntity entity)
        {
            //Cache
            if (ICache.IsEnabled<TEntity>())
            {
                _cache.RemoveEntityAsync<TEntity>(entity).Fire();
            }

            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityUpdating), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityUpdatedAsync(TEntity entity)
        {
            //Cache 不主动添加，等待读取时添加,数据库由事务控制
            //if (_cache.IsEnabled<TEntity>())
            //_cache.SetEntityAsync<TEntity>(entity).Fire();

            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityUpdated), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityUpdateFailedAsync(TEntity? entity)
        {
            ////Cache Update之前就已经删除了
            //if (entity != null && ICache.IsEnabled<TEntity>())
            //{
            //    _cache.RemoveEntityAsync<TEntity>(entity).Fire();
            //}

            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityUpdateFailed), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityAddingAsync(TEntity entity)
        {
            //events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityAdding), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityAddedAsync(TEntity entity)
        {
            //Cache 不主动添加，等待读取时添加,数据库由事务控制
            //if (_cache.IsEnabled<TEntity>())
            //{
            //    _cache.SetEntityAsync<TEntity>(entity).Fire();
            //}

            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityAdded), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityDeletingAsync(TEntity entity)
        {
            //Cache
            if (ICache.IsEnabled<TEntity>())
            {
                _cache.RemoveEntityAsync(entity).Fire();
            }

            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityDeleting), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityDeletedAsync(TEntity entity)
        {
            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityDeleted), entity, EventArgs.Empty);
        }

        #endregion

        #region Cache Strategy

        protected async Task<TEntity?> TryCacheAsideAsync(string dimensionKeyName, string dimensionKeyValue, Func<Task<TEntity?>> retrieve)
        {
            if (!ICache.IsEnabled<TEntity>())
            {
                return await retrieve().ConfigureAwait(false);
            }

            (TEntity? cached, bool exists) = await _cache.GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue).ConfigureAwait(false);

            if (exists)
            {
                return cached;
            }

            //常规做法是先获取锁（参看历史）。但如果仅从当前dimension来锁的话，有可能被别人从其他dimension操作同一个entity，所以这里改变常规做法，先做database retrieve
            TEntity? entity = await retrieve().ConfigureAwait(true);

            if (entity != null)
            {
                UpdateCacheAsync(entity).Fire();
            }

            return entity;
        }

        protected async Task<IEnumerable<TEntity>> TryCacheAsideAsync(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, Func<Task<IEnumerable<TEntity>>> retrieve)
        {
            if (!ICache.IsBatchEnabled<TEntity>())
            {
                return await retrieve().ConfigureAwait(false);
            }

            (IEnumerable<TEntity>? cachedEntities, bool allExists) = await _cache.GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

            if (allExists)
            {
                return cachedEntities!;
            }

            IEnumerable<TEntity> entities = await retrieve().ConfigureAwait(false);

            if (entities.IsNotNullOrEmpty())
            {
                UpdateCacheAsync(entities).Fire();
            }

            return await retrieve().ConfigureAwait(false);
        }

        private async Task UpdateCacheAsync(TEntity entity)
        {
            using IDistributedLock distributedLock = await _lockManager.LockEntityAsync(entity, OccupiedTime, PatienceTime).ConfigureAwait(false);

            if (!distributedLock.IsAcquired)
            {
                _logger.LogWarning($"锁未能占用. Entity:{nameof(TEntity)}, Guid:{entity.Guid}, Lock Status:{distributedLock.Status}");
                return;
            }

            //Double Check
            (TEntity? cached2, bool exists2) = await _cache.GetEntityAsync<TEntity>(entity).ConfigureAwait(false);

            if (exists2 && cached2!.Version >= entity.Version)
            {
                return;
            }

            _logger.LogInformation($"Cache Missed. Entity:{nameof(TEntity)}, Guid:{entity.Guid}");

            await _cache.SetEntityAsync(entity).ConfigureAwait(false);
        }

        private async Task UpdateCacheAsync(IEnumerable<TEntity> entities)
        {
            using IDistributedLock distributedLock = await _lockManager.LockEntitiesAsync(entities, OccupiedTime, PatienceTime).ConfigureAwait(false);

            if (!distributedLock.IsAcquired)
            {
                _logger.LogWarning($"锁未能占用. Entity:{nameof(TEntity)}, Guids:{entities.Select(e => e.Guid).ToJoinedString(",")}, Lock Status:{distributedLock.Status}");
                return;
            }

            //Double Check
            (IEnumerable<TEntity>? cachedEntities, bool allExists) = await _cache.GetEntitiesAsync<TEntity>(entities).ConfigureAwait(false);

            //版本检查
            if (allExis ts)
            {
                return;
            }

            _logger.LogInformation($"Cache Missed. Entity:{nameof(TEntity)}, Guids:{entities.Select(e => e.Guid).ToJoinedString(",")}");

            await _cache.SetEntitiesAsync(entities).ConfigureAwait(false);
        }

        #endregion

        #region Database Wrapper

        public async Task UpdateAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityUpdatingAsync(entity).ConfigureAwait(false);

            try
            {
                await _database.UpdateAsync(entity, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnEntityUpdateFailedAsync(entity).ConfigureAwait(false);
                throw;
            }

            await OnEntityUpdatedAsync(entity).ConfigureAwait(false);
        }

        public async Task AddAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityAddingAsync(entity).ConfigureAwait(false);

            await _database.AddAsync(entity, lastUser, transContext).ConfigureAwait(false);

            await OnEntityAddedAsync(entity).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityDeletingAsync(entity).ConfigureAwait(false);

            await _database.DeleteAsync(entity, lastUser, transContext).ConfigureAwait(false);

            await OnEntityDeletedAsync(entity).ConfigureAwait(false);
        }

        public async Task AddOrUpdateAsync(TEntity item, string lastUser, TransactionContext? transContext)
        {
            await Task.WhenAll(OnEntityUpdatingAsync(item), OnEntityAddingAsync(item)).ConfigureAwait(false);

            await _database.AddOrUpdateAsync(item, lastUser, transContext);


        }

        public async Task<IEnumerable<long>> BatchAddAsync(IEnumerable items, Func<T, Task> entityAdding, Func<T, Task> entityAdded, string lastUser, TransactionContext transContext)
        { }

        public async Task BatchUpdateAsync(IEnumerable items, Func<T, Task> entityUpdating, Func<T, Task> entityUpdated, string lastUser, TransactionContext transContext)
        { }

        public async Task BatchDeleteAsync(IEnumerable items, Func<T, Task> entityDeleting, Func<T, Task> entityDeleted, string lastUser, TransactionContext transContext)
        { }

        public async Task<IEnumerable<Tuple<long, int>>> BatchAddOrUpdateAsync(IEnumerable items, Func<T, Task> entityAddOrUpdating, Func<T, Task> entityAddOrUpdated, string lastUser, TransactionContext transContext)
        { }

        #endregion
    }
}

﻿using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Common.Entities;
using HB.FullStack.Database;
using HB.FullStack.DistributedLock;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Business
{
    /// <summary>
    /// 参考https://blog.csdn.net/z50l2o08e2u4aftor9a/article/details/81008933
    /// Update时先操作数据库，再操作缓存。只在读取时，更新缓存
    /// 这里体现缓存的策略：
    /// 所有的关于TEntity的update\delete都要经过这里，保证缓存的Invalidation正确
    /// Service里面不要出现_database.Update / _database.Delete,全部由Biz来调用
    /// Cache Strategy : Cache Aside
    /// Invalidation Strategy: delete from cache when database update/delete, add to cache when database add
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class BaseEntityBiz<TEntity> where TEntity : Entity, new()
    {
        public static readonly TimeSpan OccupiedTime = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan PatienceTime = TimeSpan.FromSeconds(2);

        protected readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();
        protected readonly ILogger _logger;
        protected readonly ICache _cache;
        private readonly IDatabase _database;
        private readonly IMemoryLockManager _memoryLockManager;

        public BaseEntityBiz(ILogger logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
        {
            _logger = logger;
            _cache = cache;
            _memoryLockManager = memoryLockManager;

            //Dirty trick
            _database = (IDatabase)databaseReader;
        }

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

        public event AsyncEventHandler<TEntity> EntityAddFailed
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

        public event AsyncEventHandler<TEntity> EntityDeleteFailed
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        protected virtual Task OnEntityUpdatingAsync(TEntity entity)
        {
            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityUpdating), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityUpdatedAsync(TEntity entity)
        {
            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityUpdated), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityUpdateFailedAsync(TEntity? entity)
        {
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
            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityAdded), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityAddFailedAsync(TEntity entity)
        {
            return _asyncEventManager.RaiseEventAsync(nameof(EntityAddFailed), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityDeletingAsync(TEntity entity)
        {
            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityDeleting), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityDeletedAsync(TEntity entity)
        {
            //Events
            return _asyncEventManager.RaiseEventAsync(nameof(EntityDeleted), entity, EventArgs.Empty);
        }

        protected virtual Task OnEntityDeleteFailedAsync(TEntity entity)
        {
            return _asyncEventManager.RaiseEventAsync(nameof(EntityAddFailed), entity, EventArgs.Empty);
        }

        #endregion

        #region Entity

        protected async Task<TEntity?> TryCacheAsideAsync(string dimensionKeyName, string dimensionKeyValue, Func<IDatabaseReader, Task<TEntity?>> dbRetrieve)
        {
            if (!ICache.IsEntityEnabled<TEntity>())
            {
                return await dbRetrieve(_database).ConfigureAwait(false);
            }

            (TEntity? cached, bool exists) = await _cache.GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue).ConfigureAwait(false);

            if (exists)
            {
                return cached;
            }

            //常规做法是先获取锁（参看历史）。
            //但如果仅从当前dimension来锁的话，有可能被别人从其他dimension操作同一个entity，
            //所以这里改变常规做法，先做database retrieve

            //以上是针对无version版本cache的。现在不用担心从其他dimension操作同一个entity了，cache会自带version来判断。
            //而且如果刚开始很多请求直接打到数据库上，数据库撑不住，还是得加锁。
            //但可以考虑加单机本版的锁就可，这个锁主要为了降低数据库压力，不再是为了数据一致性（带version的cache自己解决）。
            //所以可以使用单机版本的锁即可。一个主机同时放一个db请求，还是没问题的。

            using var @lock = _memoryLockManager.Lock(typeof(TEntity).Name, dimensionKeyName + dimensionKeyValue, OccupiedTime, PatienceTime);

            if (@lock.IsAcquired)
            {
                //double check
                (cached, exists) = await _cache.GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue).ConfigureAwait(false);

                if (exists)
                {
                    return cached;
                }

                TEntity? entity = await dbRetrieve(_database).ConfigureAwait(true);

                if (entity != null)
                {
                    UpdateCache(new TEntity[] { entity });

                    _logger.LogInformation($"缓存 Missed. Entity:{typeof(TEntity).Name}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValue:{dimensionKeyValue}");
                }
                else
                {
                    _logger.LogInformation($"查询到空值. Entity:{typeof(TEntity).Name}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValue:{dimensionKeyValue}");
                }

                return entity;
            }
            else
            {
                _logger.LogCritical($"锁未能占用. Entity:{typeof(TEntity).Name}, dimensionKeyName:{dimensionKeyName},dimensionKeyValue:{dimensionKeyValue}, Lock Status:{@lock.Status}");

                return await dbRetrieve(_database).ConfigureAwait(false);
            }
        }

        protected async Task<IEnumerable<TEntity>> TryCacheAsideAsync(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TEntity>>> dbRetrieve)
        {
            if (!ICache.IsEntityBatchEnabled<TEntity>())
            {
                return await dbRetrieve(_database).ConfigureAwait(false);
            }

            (IEnumerable<TEntity>? cachedEntities, bool allExists) = await _cache.GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

            if (allExists)
            {
                return cachedEntities!;
            }

            using var @lock = _memoryLockManager.Lock(typeof(TEntity).Name, dimensionKeyValues.Select(d => dimensionKeyName + d), OccupiedTime, PatienceTime);

            if (@lock.IsAcquired)
            {
                //Double check
                (cachedEntities, allExists) = await _cache.GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

                if (allExists)
                {
                    return cachedEntities!;
                }

                IEnumerable<TEntity> entities = await dbRetrieve(_database).ConfigureAwait(false);

                if (entities.IsNotNullOrEmpty())
                {
                    UpdateCache(entities);

                    _logger.LogInformation($"缓存 Missed. Entity:{typeof(TEntity).Name}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}");
                }
                else
                {
                    _logger.LogInformation($"查询到空值. Entity:{typeof(TEntity).Name}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}");
                }

                return entities;
            }
            else
            {
                _logger.LogError($"锁未能占用. Entity:{typeof(TEntity).Name}, dimensionKeyName:{dimensionKeyName},dimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}, Lock Status:{@lock.Status}");

                return await dbRetrieve(_database).ConfigureAwait(false);
            }

        }

        protected async Task UpdateAsync(TEntity entity, string lastUser, TransactionContext? transContext)
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

            //Cache
            InvalidateCache(new TEntity[] { entity });

            await OnEntityUpdatedAsync(entity).ConfigureAwait(false);
        }

        protected async Task AddAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityAddingAsync(entity).ConfigureAwait(false);

            try
            {
                await _database.AddAsync(entity, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnEntityAddFailedAsync(entity).ConfigureAwait(false);
                throw;
            }

            await OnEntityAddedAsync(entity).ConfigureAwait(false);
        }

        protected async Task DeleteAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityDeletingAsync(entity).ConfigureAwait(false);

            try
            {
                await _database.DeleteAsync(entity, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnEntityDeleteFailedAsync(entity).ConfigureAwait(false);
                throw;
            }

            //Cache
            InvalidateCache(new TEntity[] { entity });

            await OnEntityDeletedAsync(entity).ConfigureAwait(false);
        }

        protected async Task<IEnumerable<long>> BatchAddAsync(IEnumerable<TEntity> entities, string lastUser, TransactionContext transContext)
        {
            foreach (TEntity entity in entities)
            {
                await OnEntityAddingAsync(entity).ConfigureAwait(false);
            }

            IEnumerable<long> results;

            try
            {
                results = await _database.BatchAddAsync(entities, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                foreach (TEntity entity in entities)
                {
                    await OnEntityAddFailedAsync(entity).ConfigureAwait(false);
                }

                throw;
            }

            foreach (TEntity entity in entities)
            {
                await OnEntityAddedAsync(entity).ConfigureAwait(false);
            }

            return results;
        }

        protected async Task BatchUpdateAsync(IEnumerable<TEntity> entities, string lastUser, TransactionContext transContext)
        {
            foreach (TEntity entity in entities)
            {
                await OnEntityUpdatingAsync(entity).ConfigureAwait(false);
            }

            try
            {
                await _database.BatchUpdateAsync(entities, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                foreach (TEntity entity in entities)
                {
                    await OnEntityUpdateFailedAsync(entity).ConfigureAwait(false);
                }

                throw;
            }

            //Cache
            InvalidateCache(entities);

            foreach (TEntity entity in entities)
            {
                await OnEntityUpdatedAsync(entity).ConfigureAwait(false);
            }
        }

        protected async Task BatchDeleteAsync(IEnumerable<TEntity> entities, string lastUser, TransactionContext transContext)
        {
            foreach (TEntity entity in entities)
            {
                await OnEntityDeletingAsync(entity).ConfigureAwait(false);
            }

            try
            {
                await _database.BatchDeleteAsync(entities, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                foreach (TEntity entity in entities)
                {
                    await OnEntityDeleteFailedAsync(entity).ConfigureAwait(false);
                }

                throw;
            }

            //Cache
            InvalidateCache(entities);

            foreach (TEntity entity in entities)
            {
                await OnEntityDeletedAsync(entity).ConfigureAwait(false);
            }
        }

        private void UpdateCache(IEnumerable<TEntity> entities)
        {
            #region 普通缓存，加锁的做法
            //using IDistributedLock distributedLock = await _lockManager.LockEntitiesAsync(entities, OccupiedTime, PatienceTime).ConfigureAwait(false);

            //if (!distributedLock.IsAcquired)
            //{
            //    _logger.LogWarning($"锁未能占用. Entity:{nameof(TEntity)}, Guids:{entities.Select(e => e.Guid).ToJoinedString(",")}, Lock Status:{distributedLock.Status}");
            //    return;
            //}

            ////Double Check
            //(IEnumerable<TEntity>? cachedEntities, bool allExists) = await _cache.GetEntitiesAsync<TEntity>(entities).ConfigureAwait(false);

            ////版本检查
            //if (allExis ts)
            //{
            //    return;
            //}

            //_logger.LogInformation($"Cache Missed. Entity:{nameof(TEntity)}, Guids:{entities.Select(e => e.Guid).ToJoinedString(",")}");

            //await _cache.SetEntitiesAsync(entities).ConfigureAwait(false);
            #endregion

            #region 有版本控制的Cache. 就一句话，爽不爽

            _cache.SetEntitiesAsync(entities).Fire();

            #endregion

        }

        private void InvalidateCache(IEnumerable<TEntity> entities)
        {
            if (ICache.IsEntityBatchEnabled<TEntity>())
            {
                _cache.RemoveEntitiesAsync(entities).Fire();
            }
        }

        #endregion

        #region CacheItem

        protected async Task<TResult?> TryCacheAsideAsync<TResult>(CacheItem<TResult> cacheItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve)
            where TResult : class
        {
            //Cache First
            TResult? result = await cacheItem.GetFromAsync(_cache).ConfigureAwait(false);

            if (result != null)
            {
                return result;
            }

            using var @lock = _memoryLockManager.Lock(cacheItem.ResourceType, cacheItem.CacheKey, OccupiedTime, PatienceTime);

            if (@lock.IsAcquired)
            {
                //Double Check
                result = await cacheItem.GetFromAsync(_cache).ConfigureAwait(false);

                if (result != null)
                {
                    return result;
                }

                TResult dbRt = await dbRetrieve(_database).ConfigureAwait(false);
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (dbRt != null)
                {
                    UpdateCache(cacheItem.Value(dbRt).Timestamp(now));
                    _logger.LogInformation($"缓存 Missed. Entity:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}");
                }
                else
                {
                    _logger.LogInformation($"查询到空值. Entity:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}");
                }

                return dbRt;
            }
            else
            {
                _logger.LogCritical($"锁未能占用. Entity:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}, Lock Status:{@lock.Status}");

                return await dbRetrieve(_database).ConfigureAwait(false);
            }
        }

        private void UpdateCache<TResult>(CacheItem<TResult> cacheItem) where TResult : class
        {
            cacheItem.SetToAsync(_cache).Fire();
        }

        protected void InvalidateCache<TResult>(CacheItem<TResult> cacheItem) where TResult : class
        {
            cacheItem.RemoveFromAsync(_cache).Fire();
        }

        #endregion
    }
}

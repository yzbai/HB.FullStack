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
    /// 参考https://blog.csdn.net/z50l2o08e2u4aftor9a/article/details/81008933
    /// Update时先操作数据库，再操作缓存。只在读取时，更新缓存
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
        protected readonly IDatabase _database;

        public EntityBaseBiz(ILogger logger, IDatabaseReader databaseReader, ICache cache)
        {
            _logger = logger;
            _cache = cache;

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

        #region Cache Strategy

        protected async Task<TEntity?> TryCacheAsideAsync(string dimensionKeyName, string dimensionKeyValue, Func<IDatabaseReader, Task<TEntity?>> dbRetrieve)
        {
            if (!ICache.IsEnabled<TEntity>())
            {
                return await dbRetrieve(_database).ConfigureAwait(false);
            }

            (TEntity? cached, bool exists) = await _cache.GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue).ConfigureAwait(false);

            if (exists)
            {
                return cached;
            }

            //常规做法是先获取锁（参看历史）。但如果仅从当前dimension来锁的话，有可能被别人从其他dimension操作同一个entity，所以这里改变常规做法，先做database retrieve
            TEntity? entity = await dbRetrieve(_database).ConfigureAwait(true);

            if (entity != null)
            {
                UpdateCacheAsync(entity).Fire();

                _logger.LogInformation($"缓存 Missed. Entity:{nameof(TEntity)}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValue:{dimensionKeyValue}");
            }
            else
            {
                _logger.LogInformation($"查询到空值. Entity:{nameof(TEntity)}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValue:{dimensionKeyValue}");
            }

            return entity;
        }

        protected async Task<IEnumerable<TEntity>> TryCacheAsideAsync(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TEntity>>> dbRetrieve)
        {
            if (!ICache.IsBatchEnabled<TEntity>())
            {
                return await dbRetrieve(_database).ConfigureAwait(false);
            }

            (IEnumerable<TEntity>? cachedEntities, bool allExists) = await _cache.GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

            if (allExists)
            {
                return cachedEntities!;
            }

            IEnumerable<TEntity> entities = await dbRetrieve(_database).ConfigureAwait(false);

            if (entities.IsNotNullOrEmpty())
            {
                UpdateCacheAsync(entities).Fire();
                _logger.LogInformation($"缓存 Missed. Entity:{nameof(TEntity)}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}");
            }
            else
            {
                _logger.LogInformation($"查询到空值. Entity:{nameof(TEntity)}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}");
            }

            return entities;
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
            if (ICache.IsEnabled<TEntity>())
            {
                _cache.RemoveEntityAsync<TEntity>(entity).Fire();
            }

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
            if (ICache.IsEnabled<TEntity>())
            {
                _cache.RemoveEntityAsync(entity).Fire();
            }

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

            foreach (TEntity entity in entities)
            {
                await OnEntityDeletedAsync(entity).ConfigureAwait(false);
            }
        }

        private Task UpdateCacheAsync(TEntity entity)
        {
            #region 普通缓存，加锁的做法
            //using IDistributedLock distributedLock = await _lockManager.LockEntityAsync(entity, OccupiedTime, PatienceTime).ConfigureAwait(false);

            //if (!distributedLock.IsAcquired)
            //{
            //    _logger.LogWarning($"锁未能占用. Entity:{nameof(TEntity)}, Guid:{entity.Guid}, Lock Status:{distributedLock.Status}");
            //    return;
            //}

            ////Double Check
            //(TEntity? cached2, bool exists2) = await _cache.GetEntityAsync<TEntity>(entity).ConfigureAwait(false);

            //if (exists2 && cached2!.Version >= entity.Version)
            //{
            //    return;
            //}

            //_logger.LogInformation($"Cache Missed. Entity:{nameof(TEntity)}, Guid:{entity.Guid}");

            //await _cache.SetEntityAsync(entity).ConfigureAwait(false);

            #endregion

            #region 有版本控制的Cache. 就一句话，爽不爽

            return _cache.SetEntityAsync(entity);

            #endregion
        }

        private Task UpdateCacheAsync(IEnumerable<TEntity> entities)
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

            return _cache.SetEntitiesAsync(entities);

            #endregion

        }

        #endregion
    }
}

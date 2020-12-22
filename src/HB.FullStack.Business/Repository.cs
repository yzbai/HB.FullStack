using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Database;
using HB.FullStack.Database.Def;
using HB.FullStack.Lock.Memory;
using Microsoft.Extensions.Logging;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.FullStack.Repository
{
    /// <summary>
    /// 参考https://blog.csdn.net/z50l2o08e2u4aftor9a/article/details/81008933
    /// Update时先操作数据库，再操作缓存。只在读取时，更新缓存
    /// 这里体现缓存的策略：
    /// 所有的关于TEntity的update\delete都要经过这里，保证缓存的Invalidation正确
    /// Service里面不要出现_database.Update / _database.Delete,全部由Repo来调用
    /// Cache Strategy : Cache Aside
    /// Invalidation Strategy: delete from cache when database update/delete, add to cache when database add
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class Repository<TEntity> where TEntity : DatabaseEntity, new()
    {
        protected readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();
        protected readonly ILogger _logger;
        protected readonly ICache _cache;
        private readonly IDatabase _database;
        private readonly IMemoryLockManager _memoryLockManager;

        public Repository(ILogger logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
        {
            _logger = logger;
            _cache = cache;
            _memoryLockManager = memoryLockManager;

            //Dirty trick
            _database = (IDatabase)databaseReader;

            _logger.LogInformation($"{GetType().Name} 初始化完成");
        }

        #region Events

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityUpdating
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityUpdated
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity?, DatabaseWriteEventArgs> EntityUpdateFailed
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityAdding
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityAdded
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityAddFailed
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityDeleting
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityDeleted
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityDeleteFailed
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

        #region Database Write Wrapper

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
            EntityCacheStrategy.InvalidateCache(new TEntity[] { entity }, _cache);

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
            EntityCacheStrategy.InvalidateCache(new TEntity[] { entity }, _cache);

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
            EntityCacheStrategy.InvalidateCache(entities, _cache);

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
            EntityCacheStrategy.InvalidateCache(entities, _cache);

            foreach (TEntity entity in entities)
            {
                await OnEntityDeletedAsync(entity).ConfigureAwait(false);
            }
        }

        #endregion

        #region Cache Strategy

        protected async Task<TEntity?> TryCacheAsideAsync(string dimensionKeyName, object dimensionKeyValue, Func<IDatabaseReader, Task<TEntity?>> dbRetrieve)
        {
            var results = await EntityCacheStrategy.CacheAsideAsync<TEntity>(
                dimensionKeyName,
                new object[] { dimensionKeyValue },
                async dbReader =>
                {
                    TEntity? single = await dbRetrieve(dbReader).ConfigureAwait(false);

                    if (single == null)
                    {
                        return new TEntity[0];
                    }

                    return new TEntity[] { single };
                },
                _database,
                _cache,
                _memoryLockManager,
                _logger).ConfigureAwait(false);

            if (results.IsNullOrEmpty())
            {
                return null;
            }

            return results.ElementAt(0);
        }

        protected Task<IEnumerable<TEntity>> TryCacheAsideAsync(string dimensionKeyName, IEnumerable dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TEntity>>> dbRetrieve)
        {
            return EntityCacheStrategy.CacheAsideAsync(dimensionKeyName, dimensionKeyValues, dbRetrieve, _database, _cache, _memoryLockManager, _logger);
        }

        protected Task<TResult?> TryCacheAsideAsync<TResult>(CachedItem<TResult> cachedItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve) where TResult : class
        {
            return CachedItemCacheStrategy.CacheAsideAsync(cachedItem, dbRetrieve, _cache, _memoryLockManager, _database, _logger);
        }

        protected Task<IEnumerable<TResult>> TryCacheAsideAsync<TResult>(CachedItem<IEnumerable<TResult>> cachedItem, Func<IDatabaseReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : class
        {
            return CachedItemCacheStrategy.CacheAsideAsync<IEnumerable<TResult>>(cachedItem, dbRetrieve, _cache, _memoryLockManager, _database, _logger)!;
        }

        protected void InvalidateCache<TResult>(CachedItem<TResult> cachedItem) where TResult : class
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItem, _cache);
        }

        #endregion
    }
}

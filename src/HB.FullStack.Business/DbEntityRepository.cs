﻿using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Database;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace HB.FullStack.Repository
{
    /// <summary>
    /// 每一个Repo对应一个Entity
    /// 参考https://blog.csdn.net/z50l2o08e2u4aftor9a/article/details/81008933
    /// Update时先操作数据库，再操作缓存。只在读取时，更新缓存
    /// 这里体现缓存的策略：
    /// 所有的关于TEntity的update\delete都要经过这里，保证缓存的Invalidation正确
    /// Service里面不要出现_database.Update / _database.Delete,全部由Repo来调用
    /// Cache Strategy : Cache Aside
    /// Invalidation Strategy: delete from cache when database update/delete, add to cache when database add
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class DbEntityRepository<TEntity> where TEntity : DatabaseEntity, new()
    {
        protected WeakAsyncEventManager AsyncEventManager { get; } = new WeakAsyncEventManager();
        protected ILogger Logger { get; }
        protected ICache Cache { get; }
        private IDatabase Database { get; }

        protected IDatabaseReader DbReader => Database;

        private IMemoryLockManager MemoryLockManager { get; }

        protected DbEntityRepository(ILogger logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
        {
            Logger = logger;
            Cache = cache;
            MemoryLockManager = memoryLockManager;

            //Dirty trick
            Database = (IDatabase)databaseReader;

            Logger.LogInformation($"{GetType().Name} 初始化完成");

            //在Changed后Delete Cache，而不是Changing时Delete Cache
            //https://zongwb.medium.com/how-to-invalidate-or-update-cache-correctly-5dce2db9dde5
            RegisterEntityChangedEvents(InvalidateCacheItemsOnChanged);
        }

        public void RegisterEntityChangedEvents(AsyncEventHandler<TEntity, DatabaseWriteEventArgs> OnEntityChanged)
        {
            EntityAdded += OnEntityChanged;

            EntityUpdated += OnEntityChanged;

            EntityDeleted += OnEntityChanged;
        }

        /// <summary>
        /// 多个CachedItem的时候，使用Parrell来并行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected abstract Task InvalidateCacheItemsOnChanged(TEntity sender, DatabaseWriteEventArgs args);

        #region Events

#pragma warning disable CA1003 // Use generic event handler instances
        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityUpdating
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityUpdated
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity?, DatabaseWriteEventArgs> EntityUpdateFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityAdding
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityAdded
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityAddFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityDeleting
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityDeleted
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TEntity, DatabaseWriteEventArgs> EntityDeleteFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }
#pragma warning restore CA1003 // Use generic event handler instances

        protected virtual Task OnEntityUpdatingAsync(TEntity entity)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(EntityUpdating), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityUpdatedAsync(TEntity entity)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(EntityUpdated), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityUpdateFailedAsync(TEntity? entity)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(EntityUpdateFailed), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityAddingAsync(TEntity entity)
        {
            //events
            return AsyncEventManager.RaiseEventAsync(nameof(EntityAdding), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityAddedAsync(TEntity entity)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(EntityAdded), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityAddFailedAsync(TEntity entity)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(EntityAddFailed), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityDeletingAsync(TEntity entity)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(EntityDeleting), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityDeletedAsync(TEntity entity)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(EntityDeleted), entity, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnEntityDeleteFailedAsync(TEntity entity)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(EntityAddFailed), entity, new DatabaseWriteEventArgs());
        }

        #endregion

        #region Cache Strategy

        //TODO: 尝试提取IRetrieveStrategy

        
        
        protected async Task<TEntity?> TryCacheAsideAsync(string dimensionKeyName, object dimensionKeyValue, Func<IDatabaseReader, Task<TEntity?>> dbRetrieve)
        {
            IEnumerable<TEntity>? results = await EntityCacheStrategy.CacheAsideAsync<TEntity>(
                dimensionKeyName,
                new object[] { dimensionKeyValue },
                async dbReader =>
                {
                    TEntity? single = await dbRetrieve(dbReader).ConfigureAwait(false);

                    if (single == null)
                    {
                        return Array.Empty<TEntity>();
                    }

                    return new TEntity[] { single };
                },
                Database,
                Cache,
                MemoryLockManager,
                Logger).ConfigureAwait(false);

            if (results.IsNullOrEmpty())
            {
                Logger.LogDebug("Repo中没有找到 {EntityType}, dimensionKey :{dimensionKey}, dimensionKeyValue :{dimensionKeyValue}", typeof(TEntity).Name, dimensionKeyName, dimensionKeyValue);
                return null;
            }

            Logger.LogDebug("Repo中 找到 {EntityType}, dimensionKey :{dimensionKey}, dimensionKeyValue :{dimensionKeyValue}", typeof(TEntity).Name, dimensionKeyName, dimensionKeyValue);

            return results.ElementAt(0);
        }

        
        
        protected Task<IEnumerable<TEntity>> TryCacheAsideAsync(string dimensionKeyName, IEnumerable dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TEntity>>> dbRetrieve)
        {
            return EntityCacheStrategy.CacheAsideAsync(dimensionKeyName, dimensionKeyValues, dbRetrieve, Database, Cache, MemoryLockManager, Logger);
        }

        
        
        protected Task<TResult?> TryCacheAsideAsync<TResult>(CachedItem<TResult> cachedItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve) where TResult : class
        {
            return CachedItemCacheStrategy.CacheAsideAsync(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger);
        }

        
        
        protected Task<IEnumerable<TResult>> TryCacheAsideAsync<TResult>(CachedItem<IEnumerable<TResult>> cachedItem, Func<IDatabaseReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : class
        {
            return CachedItemCacheStrategy.CacheAsideAsync<IEnumerable<TResult>>(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger)!;
        }

        
        public void InvalidateCache(CachedItem cachedItem)
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItem, Cache);
        }

        public void InvalidateCacheByCacheType(CachedItem cachedItem)
        {
            CachedItemCacheStrategy.InvalidateCacheByCacheType(cachedItem, Cache);
        }

        public void InvalidateCaches(IEnumerable<CachedItem> cachedItems, UtcNowTicks utcNowTicks)
        {
            CachedItemCacheStrategy.InvalidateCaches(cachedItems, utcNowTicks, Cache);
        }

        #endregion

        #region Database Write Wrapper

        /// <summary>
        /// UpdateAsync
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        
        public async Task UpdateAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityUpdatingAsync(entity).ConfigureAwait(false);

            try
            {
                await Database.UpdateAsync(entity, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnEntityUpdateFailedAsync(entity).ConfigureAwait(false);
                throw;
            }

            //Cache
            EntityCacheStrategy.InvalidateCache(new TEntity[] { entity }, Cache);

            await OnEntityUpdatedAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// AddAsync
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        
        public async Task AddAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityAddingAsync(entity).ConfigureAwait(false);

            try
            {
                await Database.AddAsync(entity, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnEntityAddFailedAsync(entity).ConfigureAwait(false);
                throw;
            }

            await OnEntityAddedAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        
        public async Task DeleteAsync(TEntity entity, string lastUser, TransactionContext? transContext)
        {
            await OnEntityDeletingAsync(entity).ConfigureAwait(false);

            try
            {
                await Database.DeleteAsync(entity, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnEntityDeleteFailedAsync(entity).ConfigureAwait(false);
                throw;
            }

            //Cache
            EntityCacheStrategy.InvalidateCache(new TEntity[] { entity }, Cache);

            await OnEntityDeletedAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// AddAsync
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        
        public async Task<IEnumerable<object>> AddAsync(IEnumerable<TEntity> entities, string lastUser, TransactionContext? transContext)
        {
            foreach (TEntity entity in entities)
            {
                await OnEntityAddingAsync(entity).ConfigureAwait(false);
            }

            IEnumerable<object> results;

            try
            {
                results = await Database.BatchAddAsync(entities, lastUser, transContext).ConfigureAwait(false);
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

        /// <summary>
        /// UpdateAsync
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        
        public async Task UpdateAsync(IEnumerable<TEntity> entities, string lastUser, TransactionContext? transContext)
        {
            foreach (TEntity entity in entities)
            {
                await OnEntityUpdatingAsync(entity).ConfigureAwait(false);
            }

            try
            {
                await Database.BatchUpdateAsync(entities, lastUser, transContext).ConfigureAwait(false);
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
            EntityCacheStrategy.InvalidateCache(entities, Cache);

            foreach (TEntity entity in entities)
            {
                await OnEntityUpdatedAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="lastUser"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        
        public async Task DeleteAsync(IEnumerable<TEntity> entities, string lastUser, TransactionContext? transContext)
        {
            foreach (TEntity entity in entities)
            {
                await OnEntityDeletingAsync(entity).ConfigureAwait(false);
            }

            try
            {
                await Database.BatchDeleteAsync(entities, lastUser, transContext).ConfigureAwait(false);
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
            EntityCacheStrategy.InvalidateCache(entities, Cache);

            foreach (TEntity entity in entities)
            {
                await OnEntityDeletedAsync(entity).ConfigureAwait(false);
            }
        }

        //public Task<IEnumerable<TEntity>> GetByForeignKeyAsync(
        //    Expression<Func<TEntity, object>> foreignKeyExp, 
        //    object foreignKeyValue, 
        //    TransactionContext? transactionContext, 
        //    int? page, 
        //    int? perPage, 
        //    string? orderBy)
        //{
        //    return Database.RetrieveByForeignKeyAsync(foreignKeyExp, foreignKeyValue, transactionContext, page, perPage, orderBy);
        //}

        //public Task<IEnumerable<T>> GetAsync<T>(int? page, int? perPage, string? orderBy) where T : DatabaseEntity
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<T> GetById<T>(object id) where T : DatabaseEntity
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}

using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Database;
using HB.FullStack.Database.DatabaseModels;
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
    /// 每一个Repo对应一个Model
    /// 参考https://blog.csdn.net/z50l2o08e2u4aftor9a/article/details/81008933
    /// Update时先操作数据库，再操作缓存。只在读取时，更新缓存
    /// 这里体现缓存的策略：
    /// 所有的关于TModel的update\delete都要经过这里，保证缓存的Invalidation正确
    /// Service里面不要出现_database.Update / _database.Delete,全部由Repo来调用
    /// Cache Strategy : Cache Aside
    /// Invalidation Strategy: delete from cache when database update/delete, add to cache when database add
    /// Cache架构策略可以参考笔记
    /// </summary>
    /// <typeparam name="TDatabaseModel"></typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "<Pending>")]
    public abstract class ModelRepository<TDatabaseModel> where TDatabaseModel : ServerDatabaseModel, new()
    {
        protected WeakAsyncEventManager AsyncEventManager { get; } = new WeakAsyncEventManager();
        protected ILogger Logger { get; }
        protected IModelCache Cache { get; }
        private IDatabase Database { get; }

        protected IDatabaseReader DbReader => Database;

        private IMemoryLockManager MemoryLockManager { get; }

        protected ModelRepository(ILogger logger, IDatabaseReader databaseReader, IModelCache cache, IMemoryLockManager memoryLockManager)
        {
            Logger = logger;
            Cache = cache;
            MemoryLockManager = memoryLockManager;

            //Dirty trick
            Database = (IDatabase)databaseReader;

            Logger.LogInformation($"{GetType().Name} 初始化完成");

            //在Changed后Delete Cache，而不是Changing时Delete Cache
            //https://zongwb.medium.com/how-to-invalidate-or-update-cache-correctly-5dce2db9dde5
            RegisterModelChangedEvents(InvalidateCacheItemsOnChanged);
        }

        public void RegisterModelChangedEvents(AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> OnModelChanged)
        {
            ModelAdded += OnModelChanged;

            ModelUpdated += OnModelChanged;

            ModelDeleted += OnModelChanged;
        }

        /// <summary>
        /// 多个CachedItem的时候，使用Parrell来并行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected abstract Task InvalidateCacheItemsOnChanged(TDatabaseModel sender, DatabaseWriteEventArgs args);

        #region Events

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelUpdating
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelUpdated
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel?, DatabaseWriteEventArgs> ModelUpdateFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelAdding
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelAdded
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelAddFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelDeleting
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelDeleted
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<TDatabaseModel, DatabaseWriteEventArgs> ModelDeleteFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        protected virtual Task OnModelUpdatingAsync(TDatabaseModel model)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdating), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelUpdatedAsync(TDatabaseModel model)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdated), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelUpdateFailedAsync(TDatabaseModel? model)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdateFailed), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelAddingAsync(TDatabaseModel model)
        {
            //events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAdding), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelAddedAsync(TDatabaseModel model)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAdded), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelAddFailedAsync(TDatabaseModel model)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAddFailed), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelDeletingAsync(TDatabaseModel model)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelDeleting), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelDeletedAsync(TDatabaseModel model)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelDeleted), model, new DatabaseWriteEventArgs());
        }

        protected virtual Task OnModelDeleteFailedAsync(TDatabaseModel model)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAddFailed), model, new DatabaseWriteEventArgs());
        }

        #endregion

        #region Model Cache Strategy

        //TODO: 尝试提取IRetrieveStrategy

        protected async Task<TDatabaseModel?> CacheAsideAsync(string dimensionKeyName, object dimensionKeyValue, Func<IDatabaseReader, Task<TDatabaseModel?>> dbRetrieve)
        {
            IEnumerable<TDatabaseModel>? results = await ModelCacheStrategy.CacheAsideAsync<TDatabaseModel>(
                dimensionKeyName,
                new object[] { dimensionKeyValue },
                async dbReader =>
                {
                    TDatabaseModel? single = await dbRetrieve(dbReader).ConfigureAwait(false);

                    if (single == null)
                    {
                        return Array.Empty<TDatabaseModel>();
                    }

                    return new TDatabaseModel[] { single };
                },
                Database,
                Cache,
                MemoryLockManager,
                Logger).ConfigureAwait(false);

            if (results.IsNullOrEmpty())
            {
                Logger.LogDebug("Repo中没有找到 {ModelType}, dimensionKey :{dimensionKey}, dimensionKeyValue :{dimensionKeyValue}", typeof(TDatabaseModel).Name, dimensionKeyName, dimensionKeyValue);
                return null;
            }

            Logger.LogDebug("Repo中 找到 {ModelType}, dimensionKey :{dimensionKey}, dimensionKeyValue :{dimensionKeyValue}", typeof(TDatabaseModel).Name, dimensionKeyName, dimensionKeyValue);

            return results.ElementAt(0);
        }

        protected Task<IEnumerable<TDatabaseModel>> CacheAsideAsync(string dimensionKeyName, IEnumerable dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TDatabaseModel>>> dbRetrieve)
        {
            return ModelCacheStrategy.CacheAsideAsync(dimensionKeyName, dimensionKeyValues, dbRetrieve, Database, Cache, MemoryLockManager, Logger);
        }

        #endregion

        #region Timestamp Cache Strategy

        protected Task<TResult?> CacheAsideAsync<TResult>(CachedItem<TResult> cachedItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve) where TResult : class
        {
            return CachedItemCacheStrategy.CacheAsideAsync(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger);
        }

        protected Task<IEnumerable<TResult>> CacheAsideAsync<TResult>(CachedItem<IEnumerable<TResult>> cachedItem, Func<IDatabaseReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : class
        {
            return CachedItemCacheStrategy.CacheAsideAsync<IEnumerable<TResult>>(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger)!;
        }

        public void InvalidateCache(CachedItem cachedItem)
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItem, Cache);
        }

        public void InvalidateCache(IEnumerable<CachedItem> cachedItems, UtcNowTicks utcNowTicks)
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItems, utcNowTicks, Cache);
        }

        #endregion

        #region Collection Cache Strategy

        protected Task<TResult?> CacheAsideAsync<TResult>(CachedCollectionItem<TResult> cachedCollectionItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve) where TResult : class
        {
            return CachedCollectionItemCacheStrategy.CacheAsideAsync(cachedCollectionItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger);
        }

        protected Task<IEnumerable<TResult>> CacheAsideAsync<TResult>(CachedCollectionItem<IEnumerable<TResult>> cachedCollectionItem, Func<IDatabaseReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : class
        {
            return CachedCollectionItemCacheStrategy.CacheAsideAsync<IEnumerable<TResult>>(cachedCollectionItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger)!;
        }

        public void InvalidateCache(CachedCollectionItem cachedCollectionItem)
        {
            CachedCollectionItemCacheStrategy.InvalidateCache(cachedCollectionItem, Cache);
        }

        public void InvalidateCache(IEnumerable<CachedCollectionItem> cachedCollectionItems)
        {
            CachedCollectionItemCacheStrategy.InvalidateCache(cachedCollectionItems, Cache);
        }

        public void InvalidateCacheCollection<T>() where T : CachedCollectionItem
        {
            CachedCollectionItemCacheStrategy.InvalidateCacheCollection(CachedCollectionItem.GetCollectionKey<T>(), Cache);
        }

        #endregion

        #region Database Write Wrapper

        public async Task UpdateAsync(TDatabaseModel model, string lastUser, TransactionContext? transContext)
        {
            await OnModelUpdatingAsync(model).ConfigureAwait(false);

            try
            {
                await Database.UpdateAsync(model, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelUpdateFailedAsync(model).ConfigureAwait(false);
                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(new TDatabaseModel[] { model }, Cache);

            await OnModelUpdatedAsync(model).ConfigureAwait(false);
        }

        public async Task AddAsync(TDatabaseModel model, string lastUser, TransactionContext? transContext)
        {
            await OnModelAddingAsync(model).ConfigureAwait(false);

            try
            {
                await Database.AddAsync(model, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelAddFailedAsync(model).ConfigureAwait(false);
                throw;
            }

            await OnModelAddedAsync(model).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TDatabaseModel model, string lastUser, TransactionContext? transContext)
        {
            await OnModelDeletingAsync(model).ConfigureAwait(false);

            try
            {
                await Database.DeleteAsync(model, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelDeleteFailedAsync(model).ConfigureAwait(false);
                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(new TDatabaseModel[] { model }, Cache);

            await OnModelDeletedAsync(model).ConfigureAwait(false);
        }

        public async Task<IEnumerable<object>> AddAsync(IEnumerable<TDatabaseModel> models, string lastUser, TransactionContext? transContext)
        {
            foreach (TDatabaseModel model in models)
            {
                await OnModelAddingAsync(model).ConfigureAwait(false);
            }

            IEnumerable<object> results;

            try
            {
                results = await Database.BatchAddAsync(models, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                foreach (TDatabaseModel model in models)
                {
                    await OnModelAddFailedAsync(model).ConfigureAwait(false);
                }

                throw;
            }

            foreach (TDatabaseModel model in models)
            {
                await OnModelAddedAsync(model).ConfigureAwait(false);
            }

            return results;
        }

        public async Task UpdateAsync(IEnumerable<TDatabaseModel> models, string lastUser, TransactionContext? transContext)
        {
            foreach (TDatabaseModel model in models)
            {
                await OnModelUpdatingAsync(model).ConfigureAwait(false);
            }

            try
            {
                await Database.BatchUpdateAsync(models, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                foreach (TDatabaseModel model in models)
                {
                    await OnModelUpdateFailedAsync(model).ConfigureAwait(false);
                }

                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(models, Cache);

            foreach (TDatabaseModel model in models)
            {
                await OnModelUpdatedAsync(model).ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(IEnumerable<TDatabaseModel> models, string lastUser, TransactionContext? transContext)
        {
            foreach (TDatabaseModel model in models)
            {
                await OnModelDeletingAsync(model).ConfigureAwait(false);
            }

            try
            {
                await Database.BatchDeleteAsync(models, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                foreach (TDatabaseModel model in models)
                {
                    await OnModelDeleteFailedAsync(model).ConfigureAwait(false);
                }

                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(models, Cache);

            foreach (TDatabaseModel model in models)
            {
                await OnModelDeletedAsync(model).ConfigureAwait(false);
            }
        }

        //public Task<IEnumerable<TModel>> GetByForeignKeyAsync(
        //    Expression<Func<TModel, object>> foreignKeyExp,
        //    object foreignKeyValue,
        //    TransactionContext? transactionContext,
        //    int? page,
        //    int? perPage,
        //    string? orderBy)
        //{
        //    return Database.RetrieveByForeignKeyAsync(foreignKeyExp, foreignKeyValue, transactionContext, page, perPage, orderBy);
        //}

        //public Task<IEnumerable<T>> GetAsync<T>(int? page, int? perPage, string? orderBy) where T : DatabaseModel
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<T> GetById<T>(object id) where T : DatabaseModel
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
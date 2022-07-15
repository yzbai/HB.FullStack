using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Common.Cache.CachedCollectionItems;
using HB.FullStack.Common.Cache.CacheItems;
using HB.FullStack.Common.Cache.CacheModels;
using HB.FullStack.Database;
using HB.FullStack.Database.DBModels;
using HB.FullStack.Database.SQL;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository.CacheStrategies;

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
    public abstract class ModelRepository<TDatabaseModel> where TDatabaseModel : TimestampDBModel, new()
    {
        protected WeakAsyncEventManager AsyncEventManager { get; } = new WeakAsyncEventManager();
        protected ILogger Logger { get; }
        protected ICache Cache { get; }
        private IDatabase Database { get; }

        protected IDatabaseReader DbReader => Database;

        private IMemoryLockManager MemoryLockManager { get; }

        protected ModelRepository(ILogger logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
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

        public void RegisterModelChangedEvents(AsyncEventHandler<IEnumerable<DBModel>, DBChangedEventArgs> OnModelsChanged)
        {
            ModelsAdded += OnModelsChanged;

            ModelsUpdated += OnModelsChanged;

            ModelsDeleted += OnModelsChanged;
        }

        /// <summary>
        /// //NOTICE: 为什么再基类中要abstract而不是virtual，就是强迫程序员思考这里需要释放的Cache有没有
        /// </summary>
        protected abstract Task InvalidateCacheItemsOnChanged(IEnumerable<DBModel> sender, DBChangedEventArgs args);

        #region Events

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangingEventArgs> ModelUpdating
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangedEventArgs> ModelsUpdated
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangedEventArgs> ModelUpdateFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangingEventArgs> ModelAdding
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangedEventArgs> ModelsAdded
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangedEventArgs> ModelAddFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangingEventArgs> ModelDeleting
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangedEventArgs> ModelsDeleted
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IEnumerable<DBModel>, DBChangedEventArgs> ModelDeleteFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        protected virtual Task OnModelUpdatingAsync(IEnumerable<DBModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdating), models, new DBChangingEventArgs { ChangeType = DBChangeType.Update });
        }

        protected virtual Task OnModelUpdatedAsync(IEnumerable<DBModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelsUpdated), models, new DBChangedEventArgs { ChangeType = DBChangeType.Update });
        }

        protected virtual Task OnModelUpdateFailedAsync(IEnumerable<DBModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdateFailed), models, new DBChangedEventArgs { ChangeType = DBChangeType.Update });
        }

        protected virtual Task OnModelAddingAsync(IEnumerable<DBModel> models)
        {
            //events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAdding), models, new DBChangingEventArgs { ChangeType = DBChangeType.Add });
        }

        protected virtual Task OnModelAddedAsync(IEnumerable<DBModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelsAdded), models, new DBChangedEventArgs { ChangeType = DBChangeType.Add });
        }

        protected virtual Task OnModelAddFailedAsync(IEnumerable<DBModel> models)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAddFailed), models, new DBChangedEventArgs { ChangeType = DBChangeType.Add });
        }

        protected virtual Task OnModelDeletingAsync(IEnumerable<DBModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelDeleting), models, new DBChangingEventArgs { ChangeType = DBChangeType.Delete });
        }

        protected virtual Task OnModelDeletedAsync(IEnumerable<DBModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelsDeleted), models, new DBChangedEventArgs { ChangeType = DBChangeType.Delete });
        }

        protected virtual Task OnModelDeleteFailedAsync(IEnumerable<DBModel> models)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(ModelDeleteFailed), models, new DBChangedEventArgs { ChangeType = DBChangeType.Delete });
        }

        #endregion

        #region Database Write Wrapper

        public async Task UpdateAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : DBModel, new()
        {
            await OnModelUpdatingAsync(new T[] { model }).ConfigureAwait(false);

            try
            {
                await Database.UpdateAsync<T>(model, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelUpdateFailedAsync(new T[] { model }).ConfigureAwait(false);
                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(model, Cache);

            await OnModelUpdatedAsync(new T[] { model }).ConfigureAwait(false);
        }

        public async Task AddAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : DBModel, new()
        {
            await OnModelAddingAsync(new T[] { model }).ConfigureAwait(false);

            try
            {
                await Database.AddAsync(model, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelAddFailedAsync(new T[] { model }).ConfigureAwait(false);
                throw;
            }

            await OnModelAddedAsync(new T[] { model }).ConfigureAwait(false);
        }

        public async Task DeleteAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : DBModel, new()
        {
            await OnModelDeletingAsync(new T[] { model }).ConfigureAwait(false);

            try
            {
                await Database.DeleteAsync(model, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelDeleteFailedAsync(new T[] { model }).ConfigureAwait(false);
                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(model, Cache);

            await OnModelDeletedAsync(new T[] { model }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<object>> AddAsync<T>(IEnumerable<T> models, string lastUser, TransactionContext? transContext) where T : DBModel, new()
        {
            await OnModelAddingAsync(models).ConfigureAwait(false);

            IEnumerable<object> results;

            try
            {
                results = await Database.BatchAddAsync<T>(models, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelAddFailedAsync(models).ConfigureAwait(false);

                throw;
            }

            await OnModelAddedAsync(models).ConfigureAwait(false);

            return results;
        }

        public async Task UpdateAsync<T>(IEnumerable<T> models, string lastUser, TransactionContext? transContext) where T : DBModel, new()
        {
            await OnModelUpdatingAsync(models).ConfigureAwait(false);

            try
            {
                await Database.BatchUpdateAsync(models, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelUpdateFailedAsync(models).ConfigureAwait(false);
                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(models, Cache);

            await OnModelUpdatedAsync(models).ConfigureAwait(false);
        }

        public async Task DeleteAsync<T>(IEnumerable<T> models, string lastUser, TransactionContext? transContext) where T : DBModel, new()
        {
            await OnModelDeletingAsync(models).ConfigureAwait(false);

            try
            {
                await Database.BatchDeleteAsync(models, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelDeleteFailedAsync(models).ConfigureAwait(false);

                throw;
            }

            //Cache
            ModelCacheStrategy.InvalidateCache(models, Cache);

            await OnModelDeletedAsync(models).ConfigureAwait(false);
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

        //public Task<IEnumerable<T>> GetAsync<T>(int? page, int? perPage, string? orderBy) where T : DBModel
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<T> GetById<T>(object id) where T : DBModel
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region Model Cache Strategy

        protected async Task<TDatabaseModel?> GetUsingCacheAsideAsync(string keyName, object keyValue, Func<IDatabaseReader, Task<TDatabaseModel?>> dbRetrieve)
        {
            IEnumerable<TDatabaseModel>? results = await ModelCacheStrategy.GetUsingCacheAsideAsync<TDatabaseModel>(
                keyName,
                new object[] { keyValue },
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
                Logger.LogDebug("Repo中没有找到 {ModelType}, dimensionKey :{dimensionKey}, dimensionKeyValue :{dimensionKeyValue}", typeof(TDatabaseModel).Name, keyName, keyValue);
                return null;
            }

            Logger.LogDebug("Repo中 找到 {ModelType}, dimensionKey :{dimensionKey}, dimensionKeyValue :{dimensionKeyValue}", typeof(TDatabaseModel).Name, keyName, keyValue);

            return results.ElementAt(0);
        }

        protected Task<IEnumerable<TDatabaseModel>> GetUsingCacheAsideAsync(string keyName, IEnumerable keyValues, Func<IDatabaseReader, Task<IEnumerable<TDatabaseModel>>> dbRetrieve)
        {
            return ModelCacheStrategy.GetUsingCacheAsideAsync(keyName, keyValues, dbRetrieve, Database, Cache, MemoryLockManager, Logger);
        }

        #endregion

        #region Timestamp Cache Strategy

        protected Task<TResult?> GetUsingCacheAsideAsync<TResult>(CachedItem<TResult> cachedItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve) where TResult : TimestampDBModel
        {
            return CachedItemCacheStrategy.GetUsingCacheAsideAsync(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger);
        }

        protected Task<IEnumerable<TResult>> GetUsingCacheAsideAsync<TResult>(CachedItem<IEnumerable<TResult>> cachedItem, Func<IDatabaseReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : TimestampDBModel
        {
            return CachedItemCacheStrategy.GetUsingCacheAsideAsync<IEnumerable<TResult>>(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger)!;
        }

        public void InvalidateCache(CachedItem cachedItem)
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItem, Cache);
        }

        public void InvalidateCache(IEnumerable<CachedItem> cachedItems)
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItems, Cache);
        }

        #endregion

        #region Collection Cache Strategy

        protected Task<TResult?> GetUsingCacheAsideAsync<TResult>(CachedCollectionItem<TResult> cachedCollectionItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve) where TResult : class
        {
            return CachedCollectionItemCacheStrategy.GetUsingCacheAsideAsync(cachedCollectionItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger);
        }

        protected Task<IEnumerable<TResult>> GetUsingCacheAsideAsync<TResult>(CachedCollectionItem<IEnumerable<TResult>> cachedCollectionItem, Func<IDatabaseReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : class
        {
            return CachedCollectionItemCacheStrategy.GetUsingCacheAsideAsync<IEnumerable<TResult>>(cachedCollectionItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger)!;
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

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository.CacheStrategies;

using Microsoft.Extensions.Logging;

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
    public abstract class ModelRepository<TMainDBModel> where TMainDBModel : TimestampDbModel, new()
    {
        protected WeakAsyncEventManager AsyncEventManager { get; } = new WeakAsyncEventManager();

        protected ILogger Logger { get; }

        protected ICache Cache { get; }

        private IDatabase Database { get; }

        protected IDbReader DbReader => Database;

        private IMemoryLockManager MemoryLockManager { get; }

        protected ModelRepository(ILogger logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
        {
            Logger = logger;
            Cache = cache;
            MemoryLockManager = memoryLockManager;

            //Dirty trick
            Database = (IDatabase)databaseReader;

            //在Changed后Delete Cache，而不是Changing时Delete Cache
            //https://zongwb.medium.com/how-to-invalidate-or-update-cache-correctly-5dce2db9dde5
            RegisterModelChangedEvents(InvalidateCacheItemsOnChanged);
        }

        public void RegisterModelChangedEvents(Func<object, DBChangeEventArgs, Task> OnModelsChanged)
        {
            ModelAdded += OnModelsChanged;

            ModelUpdated += OnModelsChanged;

            ModelDeleted += OnModelsChanged;
        }

        /// <summary>
        /// //NOTICE: 为什么再基类中要abstract而不是virtual，就是强迫程序员思考这里需要释放的Cache有没有
        /// </summary>
        protected abstract Task InvalidateCacheItemsOnChanged(object sender, DBChangeEventArgs args);

        #region Events

        public event Func<object, DBChangeEventArgs, Task>? ModelUpdating
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelUpdated
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelUpdateFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelAdding
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelAdded
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelAddFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelDeleting
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelDeleted
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        public event Func<object, DBChangeEventArgs, Task>? ModelDeleteFailed
        {
            add => AsyncEventManager.Add(value);
            remove => AsyncEventManager.Remove(value);
        }

        protected virtual Task OnModelUpdatingPropertiesAsync(IEnumerable<PropertyChangePack> cpps)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdating), cpps, new DBChangeEventArgs { ChangeType = DBChangeType.UpdateProperties });
        }

        protected virtual Task OnModelUpdatedPropertiesAsync(IEnumerable<PropertyChangePack> cpps)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdated), cpps, new DBChangeEventArgs { ChangeType = DBChangeType.UpdateProperties });
        }

        protected virtual Task OnModelUpdatePropertiesFailedAsync(IEnumerable<PropertyChangePack> cpps)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdateFailed), cpps, new DBChangeEventArgs { ChangeType = DBChangeType.UpdateProperties });
        }

        protected virtual Task OnModelUpdatingAsync(IEnumerable<DbModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdating), models, new DBChangeEventArgs { ChangeType = DBChangeType.Update });
        }

        protected virtual Task OnModelUpdatedAsync(IEnumerable<DbModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdated), models, new DBChangeEventArgs { ChangeType = DBChangeType.Update });
        }

        protected virtual Task OnModelUpdateFailedAsync(IEnumerable<DbModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelUpdateFailed), models, new DBChangeEventArgs { ChangeType = DBChangeType.Update });
        }

        protected virtual Task OnModelAddingAsync(IEnumerable<DbModel> models)
        {
            //events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAdding), models, new DBChangeEventArgs { ChangeType = DBChangeType.Add });
        }

        protected virtual Task OnModelAddedAsync(IEnumerable<DbModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAdded), models, new DBChangeEventArgs { ChangeType = DBChangeType.Add });
        }

        protected virtual Task OnModelAddFailedAsync(IEnumerable<DbModel> models)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(ModelAddFailed), models, new DBChangeEventArgs { ChangeType = DBChangeType.Add });
        }

        protected virtual Task OnModelDeletingAsync(IEnumerable<DbModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelDeleting), models, new DBChangeEventArgs { ChangeType = DBChangeType.Delete });
        }

        protected virtual Task OnModelDeletedAsync(IEnumerable<DbModel> models)
        {
            //Events
            return AsyncEventManager.RaiseEventAsync(nameof(ModelDeleted), models, new DBChangeEventArgs { ChangeType = DBChangeType.Delete });
        }

        protected virtual Task OnModelDeleteFailedAsync(IEnumerable<DbModel> models)
        {
            return AsyncEventManager.RaiseEventAsync(nameof(ModelDeleteFailed), models, new DBChangeEventArgs { ChangeType = DBChangeType.Delete });
        }

        #endregion

        #region Add

        public async Task AddAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : DbModel, new()
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

        public async Task<IEnumerable<object>> AddAsync<T>(IEnumerable<T> models, string lastUser, TransactionContext? transContext) where T : DbModel, new()
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

        #endregion

        #region Update

        public async Task UpdateAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : DbModel, new()
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

        public async Task UpdateAsync<T>(IEnumerable<T> models, string lastUser, TransactionContext? transContext) where T : DbModel, new()
        {
            await OnModelUpdatingAsync(models).ConfigureAwait(false);

            try
            {
                await Database.BatchUpdateAsync<T>(models, lastUser, transContext).ConfigureAwait(false);
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

        public async Task UpdateProperties<T>(PropertyChangePack cp, string lastUser, TransactionContext? transactionContext) where T : DbModel, new()
        {
            //检查必要的AddtionalProperties
            //TODO: 是否需要创建一个Attribute，标记哪些是必须包含的？而不是默认指定ForeignKey

            DbModelDef modelDef = Database.ModelDefFactory.GetDef<T>()!;

            ThrowIfAddtionalPropertiesLack(new PropertyChangePack[] { cp }, modelDef);

            await OnModelUpdatingPropertiesAsync(new PropertyChangePack[] { cp }).ConfigureAwait(false);

            try
            {
                await Database.UpdatePropertiesAsync<T>(cp, lastUser, transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelUpdatePropertiesFailedAsync(new PropertyChangePack[] { cp }).ConfigureAwait(false);
                throw;
            }

            ModelCacheStrategy.InvalidateCache<T>(new PropertyChangePack[] { cp }, modelDef, Cache);

            await OnModelUpdatedPropertiesAsync(new PropertyChangePack[] { cp }).ConfigureAwait(false);
        }

        public async Task UpdateProperties<T>(IEnumerable<PropertyChangePack> cps, string lastUser, TransactionContext? transactionContext) where T : DbModel, new()
        {
            DbModelDef modelDef = Database.ModelDefFactory.GetDef<T>()!;

            ThrowIfAddtionalPropertiesLack(cps, modelDef);

            await OnModelUpdatingPropertiesAsync(cps).ConfigureAwait(false);

            try
            {
                await Database.BatchUpdatePropertiesAsync<T>(cps, lastUser, transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await OnModelUpdatePropertiesFailedAsync(cps).ConfigureAwait(false);
                throw;
            }

            ModelCacheStrategy.InvalidateCache<T>(cps, modelDef, Cache);

            await OnModelUpdatedPropertiesAsync(cps).ConfigureAwait(false);
        }

        private static void ThrowIfAddtionalPropertiesLack(IEnumerable<PropertyChangePack> cps, DbModelDef modelDef)
        {
            foreach (var cp in cps)
            {
                if (!cp.AddtionalProperties.ContainsAllKey(modelDef.ForeignKeyProperties.Select(p => p.Name).ToList()))
                {
                    throw RepositoryExceptions.AddtionalPropertyNeeded(modelDef.ModelFullName);
                }
            }
        }

        #endregion

        #region Delete

        public async Task DeleteAsync<T>(T model, string lastUser, TransactionContext? transContext) where T : DbModel, new()
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

        public async Task DeleteAsync<T>(IEnumerable<T> models, string lastUser, TransactionContext? transContext) where T : DbModel, new()
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

        #endregion

        #region Model Cache Strategy

        protected async Task<TMainDBModel?> GetUsingCacheAsideAsync(string keyName, object keyValue, Func<IDbReader, Task<TMainDBModel?>> dbRetrieve)
        {
            IEnumerable<TMainDBModel>? results = await ModelCacheStrategy.GetUsingCacheAsideAsync<TMainDBModel>(
                keyName,
                new object[] { keyValue },
                async dbReader =>
                {
                    TMainDBModel? single = await dbRetrieve(dbReader).ConfigureAwait(false);

                    if (single == null)
                    {
                        return Array.Empty<TMainDBModel>();
                    }

                    return new TMainDBModel[] { single };
                },
                Database,
                Cache,
                MemoryLockManager,
                Logger).ConfigureAwait(false);

            if (results.IsNullOrEmpty())
            {
                Logger.LogDebug("Repo中没有找到 {ModelType}, KeyName :{KeyName}, KeyValue :{KeyValue}", typeof(TMainDBModel).Name, keyName, keyValue);
                return null;
            }

            Logger.LogDebug("Repo中 找到 {@Context}", new { ModelName = typeof(TMainDBModel).Name, KeyName = keyName, KeyValue = keyValue });

            return results.ElementAt(0);
        }

        protected Task<IEnumerable<TMainDBModel>> GetUsingCacheAsideAsync(string keyName, IEnumerable keyValues, Func<IDbReader, Task<IEnumerable<TMainDBModel>>> dbRetrieve)
        {
            return ModelCacheStrategy.GetUsingCacheAsideAsync(keyName, keyValues, dbRetrieve, Database, Cache, MemoryLockManager, Logger);
        }

        #endregion

        #region Timestamp Cache Strategy

        protected Task<TResult?> GetUsingCacheAsideAsync<TResult>(CachedItem<TResult> cachedItem, Func<IDbReader, Task<TResult>> dbRetrieve) where TResult : TimestampDbModel
        {
            return CachedItemCacheStrategy.GetUsingCacheAsideAsync(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger);
        }

        protected Task<IEnumerable<TResult>> GetUsingCacheAsideAsync<TResult>(CachedItem<IEnumerable<TResult>> cachedItem, Func<IDbReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : TimestampDbModel
        {
            return CachedItemCacheStrategy.GetUsingCacheAsideAsync<IEnumerable<TResult>>(cachedItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger)!;
        }

        public void InvalidateCache(ICachedItem cachedItem)
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItem, Cache);
        }

        public void InvalidateCache(IEnumerable<ICachedItem> cachedItems)
        {
            CachedItemCacheStrategy.InvalidateCache(cachedItems, Cache);
        }

        #endregion

        #region Collection Cache Strategy

        protected Task<TResult?> GetUsingCacheAsideAsync<TResult>(CachedCollectionItem<TResult> cachedCollectionItem, Func<IDbReader, Task<TResult>> dbRetrieve) where TResult : class
        {
            return CachedCollectionItemCacheStrategy.GetUsingCacheAsideAsync(cachedCollectionItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger);
        }

        protected Task<IEnumerable<TResult>> GetUsingCacheAsideAsync<TResult>(CachedCollectionItem<IEnumerable<TResult>> cachedCollectionItem, Func<IDbReader, Task<IEnumerable<TResult>>> dbRetrieve) where TResult : class
        {
            return CachedCollectionItemCacheStrategy.GetUsingCacheAsideAsync<IEnumerable<TResult>>(cachedCollectionItem, dbRetrieve, Cache, MemoryLockManager, Database, Logger)!;
        }

        public void InvalidateCache(ICachedCollectionItem cachedCollectionItem)
        {
            CachedCollectionItemCacheStrategy.InvalidateCache(cachedCollectionItem, Cache);
        }

        public void InvalidateCache(IEnumerable<ICachedCollectionItem> cachedCollectionItems)
        {
            CachedCollectionItemCacheStrategy.InvalidateCache(cachedCollectionItems, Cache);
        }

        public void InvalidateCacheCollection<T>() where T : ICachedCollectionItem
        {
            CachedCollectionItemCacheStrategy.InvalidateCacheCollection(ICachedCollectionItem.GetCollectionKey(typeof(T)), Cache);
        }

        #endregion

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

        //public Task<IEnumerable<T>> GetAsync<T>(int? page, int? perPage, string? orderBy) where T : DbModel
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<T> GetById<T>(object id) where T : DbModel
        //{
        //    throw new NotImplementedException();
        //}
    }
}
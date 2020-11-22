using HB.Framework.Cache;
using HB.Framework.Common;
using HB.Framework.Common.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Business
{
    public abstract class EntityBaseBiz<TEntity> where TEntity : Entity, new()
    {
        protected readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();
        protected readonly ICache _cache;

        public EntityBaseBiz(ICache cache)
        {
            _cache = cache;
        }

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

        protected async virtual Task OnEntityUpdatingAsync(TEntity entity)
        {
            //Cache
            if (ICache.IsEnabled<TEntity>())
            {
                _cache.RemoveEntityAsync<TEntity>(entity).Fire();
            }

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityUpdating), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityUpdatedAsync(TEntity entity)
        {
            //Cache 不主动添加，等待读取时添加,数据库由事务控制
            //if (_cache.IsEnabled<TEntity>())
            //_cache.SetEntityAsync<TEntity>(entity).Fire();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityUpdated), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityUpdateFailedAsync(TEntity? entity)
        {
            ////Cache Update之前就已经删除了
            //if (entity != null && ICache.IsEnabled<TEntity>())
            //{
            //    _cache.RemoveEntityAsync<TEntity>(entity).Fire();
            //}

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityUpdateFailed), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityAddingAsync(TEntity entity)
        {
            //events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityAdding), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityAddedAsync(TEntity entity)
        {
            //Cache 不主动添加，等待读取时添加,数据库由事务控制
            //if (_cache.IsEnabled<TEntity>())
            //{
            //    _cache.SetEntityAsync<TEntity>(entity).Fire();
            //}

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityAdded), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected virtual async Task OnEntityDeletingAsync(TEntity entity)
        {
            //Cache
            if (ICache.IsEnabled<TEntity>())
            {
                _cache.RemoveEntityAsync(entity).Fire();
            }

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityDeleting), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected virtual async Task OnEntityDeletedAsync(TEntity entity)
        {
            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityDeleted), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        #region Cache Strategy

        //TODO: 这里必须用分布式锁，因为可能又多台Service服务器
        //TODO: 有可能锁不住：从不同的dimension为同一个entity加缓存？
        private static ConcurrentDictionary<string, SemaphoreSlim> guidSemaphoreDict = new ConcurrentDictionary<string, SemaphoreSlim>();

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

        protected async Task<IEnumerable<TEntity>> CacheAsideAsync(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, Func<Task<IEnumerable<TEntity>>> retrieve)
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

        private const int _semaphoreTimeoutMilliseconds = 2000;
        private const int _semaphoreBatchTimeoutMilliseconds = 400;

        private async Task UpdateCacheAsync(TEntity entity)
        {
            SemaphoreSlim curSlim = guidSemaphoreDict.GetOrAdd(entity.Guid, new SemaphoreSlim(1, 1));


            if (await curSlim.WaitAsync(_semaphoreTimeoutMilliseconds).ConfigureAwait(false))
            {
                try
                {
                    //Double Check
                    (TEntity? cached2, bool exists2) = await _cache.GetEntityAsync<TEntity>(entity).ConfigureAwait(false);

                    if (exists2 && cached2!.Version >= entity.Version)
                    {
                        return;
                    }

                    //TODO: Log Missed!

                    await _cache.SetEntityAsync(entity).ConfigureAwait(false);

                }
                finally
                {
                    curSlim.Release();
                }
            }
            else
            {
                //TODO: Log 冲突
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>")]
        private async Task UpdateCacheAsync(IEnumerable<TEntity> entities)
        {
            List<SemaphoreSlim> semaphores = new List<SemaphoreSlim>();

            foreach (TEntity entity in entities)
            {
                semaphores.Add(guidSemaphoreDict.GetOrAdd(entity.Guid, new SemaphoreSlim(1, 1)));
            }

            CancellationTokenSource cts = new CancellationTokenSource(10000);

            Dictionary<Task<bool>, SemaphoreSlim> dict = new Dictionary<Task<bool>, SemaphoreSlim>();

            foreach (SemaphoreSlim semaphore in semaphores)
            {
                dict.Add(semaphore.WaitAsync(_semaphoreBatchTimeoutMilliseconds, cts.Token), semaphore);
            }

            List<SemaphoreSlim> successed = new List<SemaphoreSlim>();

            while (dict.Any())
            {
                Task<bool> finished = await Task.WhenAny<bool>(dict.Keys).ConfigureAwait(false);

                bool waitSuccess = await finished.ConfigureAwait(false);

                if (waitSuccess)
                {
                    successed.Add(dict[finished]);
                }
                else
                {
                    cts.Cancel();
                }

                dict.Remove(finished);
            }

            if (cts.IsCancellationRequested)
            {
                //TODO: log this

                foreach (SemaphoreSlim sm in successed)
                {
                    sm.Release();
                }

                return;
            }

            //获取了所有的locker
            try
            {
                await _cache.SetEntitiesAsync(entities).ConfigureAwait(false);
            }
            finally
            {
                foreach (SemaphoreSlim semaphore in semaphores)
                {
                    semaphore.Release();
                }
            }
        }

        #endregion
    }
}

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
    public abstract class SingleEntityBaseBiz<TEntity> where TEntity : Entity, new()
    {
        private static readonly int _semaphoreSlimTimeout = 5000;

        protected readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();
        protected readonly IDistributedCache _cache;

        public SingleEntityBaseBiz(IDistributedCache cache)
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

        protected async virtual Task OnEntityUpdatingAsync(TEntity entity)
        {
            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityUpdating), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityUpdatedAsync(TEntity entity)
        {
            //Cache
            _cache.SetEntityAsync<TEntity>(entity).Fire();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityUpdated), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityUpdateFailedAsync(TEntity? entity)
        {
            //Cache
            if (entity != null)
            {
                _cache.RemoveEntityAsync<TEntity>(entity).Fire();
            }

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
            //Cache
            _cache.SetEntityAsync<TEntity>(entity).Fire();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityAdded), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        //这里必须用分布式锁，因为可能又多台Service服务器
        private static ConcurrentDictionary<string, SemaphoreSlim> concurrentDictionary = new ConcurrentDictionary<string, SemaphoreSlim>();

        protected async Task<IEnumerable<TEntity>> CacheAsideAsync(string cacheKeyName, IEnumerable<string> cacheKeyValues, Func<Task<IEnumerable<TEntity>>> retrieve)
        {
            //if (!_cache.IsMultipleEnabled<TEntity>())
            //{
            //    return await retrieve().ConfigureAwait(false);
            //}

            //TODO: 暂不支持multiple 等待重写IDistributeCache

            return await retrieve().ConfigureAwait(false);
        }

        protected async Task<TEntity?> TryCacheAsideAsync(string cacheKeyName, string cacheKeyValue, Func<Task<TEntity?>> retrieve)
        {
            if (!_cache.IsEnabled<TEntity>())
            {
                return await retrieve().ConfigureAwait(false);
            }

            (TEntity? cached, bool exists) = await _cache.GetEntityAsync<TEntity>(cacheKeyName: cacheKeyName, cacheKeyValue: cacheKeyValue).ConfigureAwait(false);

            if (exists)
            {
                return cached;
            }

            SemaphoreSlim curSlim = concurrentDictionary.GetOrAdd(cacheKeyName + cacheKeyValue, new SemaphoreSlim(1, 1));

            await curSlim.WaitAsync(_semaphoreSlimTimeout).ConfigureAwait(false);

            try
            {
                //Double Check
                (TEntity? cached2, bool exists2) = await _cache.GetEntityAsync<TEntity>(cacheKeyName: cacheKeyName, cacheKeyValue: cacheKeyValue).ConfigureAwait(false);

                if (exists2)
                {
                    return cached2;
                }

                //TODO: Log Missed!

                TEntity? entity = await retrieve().ConfigureAwait(true);

                _cache.SetEntityAsync<TEntity>(cacheKeyName, cacheKeyValue, entity).Fire();

                return entity;

            }
            finally
            {
                curSlim.Release();
            }
        }
    }
}

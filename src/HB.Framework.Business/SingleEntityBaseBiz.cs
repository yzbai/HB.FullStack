using HB.Framework.Common;
using HB.Framework.Common.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Business
{
    public abstract class SingleEntityBaseBiz<TEntity> where TEntity : Entity, new()
    {
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
            _cache.SetAsync(user.Guid, user, _distributedCacheEntryOptions).Fire();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityUpdated), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityUpdateFailedAsync<TUser>(TEntity? entity)
        {
            //Cache
            if (entity != null)
            {
                _cache.RemoveAsync(user.Guid).Fire();
            }

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityUpdateFailed), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityAddingAsync<TUser>(TEntity entity)
        {
            //events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityAdding), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        protected async virtual Task OnEntityAddedAsync(TEntity entity)
        {


            //Cache
            _cache.SetAsync(user.Guid, user, _distributedCacheEntryOptions).Fire();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(EntityAdded), entity, EventArgs.Empty).ConfigureAwait(false);
        }

        private static ConcurrentDictionary<string, object> concurrentDictionary = new ConcurrentDictionary<string, object>();
        protected async Task<TEntity?> CacheAsideAsync(Func<Task<TEntity?>> execute, TimeSpan? expiresIn, string key)
        {

            (TEntity? cached, bool exists) = await _cache.GetAsync<TEntity>().ConfigureAwait(false);

            if (exists)
            {
                return cached;
            }

            //TODO: Log Missed!

            TEntity? fromDb = await execute().ConfigureAwait(true);

            _cache.SetAsync<TUser>(userGuid, fromDb, _distributedCacheEntryOptions).Fire();

            return fromDb;


            var cached = cacheManager.Get(key);

            if (EqualityComparer<T>.Default.Equals(cached, default(T)))
            {
                object lockOn = concurrentDictionary.GetOrAdd(key, new object());

                lock (lockOn)
                {
                    cached = cacheManager.Get(key);

                    if (EqualityComparer<T>.Default.Equals(cached, default(T)))
                    {
                        var executed = execute();

                        if (expiresIn.HasValue)
                            cacheManager.Set(key, executed, expiresIn.Value);
                        else
                            cacheManager.Set(key, executed);

                        return executed;
                    }
                    else
                    {
                        return cached;
                    }
                }
            }
            else
            {
                return cached;
            }
        }
    }
}

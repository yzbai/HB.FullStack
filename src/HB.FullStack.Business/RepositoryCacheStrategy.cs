using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common.Entities;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Repository
{
    internal static class RepositoryCacheStrategy
    {
        public static readonly TimeSpan OccupiedTime = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan PatienceTime = TimeSpan.FromSeconds(2);

        public static async Task<IEnumerable<TEntity>> CacheAsideAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TEntity>>> dbRetrieve,
            IDatabase database, ICache cache, IMemoryLockManager memoryLockManager, ILogger logger) where TEntity : Entity, new()
        {
            if (!ICache.IsEntityEnabled<TEntity>())
            {
                return await dbRetrieve(database).ConfigureAwait(false);
            }

            (IEnumerable<TEntity>? cachedEntities, bool allExists) = await cache.GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

            if (allExists)
            {
                return cachedEntities!;
            }

            //常规做法是先获取锁（参看历史）。
            //但如果仅从当前dimension来锁的话，有可能被别人从其他dimension操作同一个entity，
            //所以这里改变常规做法，先做database retrieve

            //以上是针对无version版本cache的。现在不用担心从其他dimension操作同一个entity了，cache会自带version来判断。
            //而且如果刚开始很多请求直接打到数据库上，数据库撑不住，还是得加锁。
            //但可以考虑加单机本版的锁就可，这个锁主要为了降低数据库压力，不再是为了数据一致性（带version的cache自己解决）。
            //所以可以使用单机版本的锁即可。一个主机同时放一个db请求，还是没问题的。

            using var @lock = memoryLockManager.Lock(typeof(TEntity).Name, dimensionKeyValues.Select(d => dimensionKeyName + d), OccupiedTime, PatienceTime);

            if (@lock.IsAcquired)
            {
                //Double check
                (cachedEntities, allExists) = await cache.GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

                if (allExists)
                {
                    return cachedEntities!;
                }

                IEnumerable<TEntity> entities = await dbRetrieve(database).ConfigureAwait(false);

                if (entities.IsNotNullOrEmpty())
                {
                    UpdateCache(entities, cache);

                    logger.LogInformation($"缓存 Missed. Entity:{typeof(TEntity).Name}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}");
                }
                else
                {
                    logger.LogInformation($"查询到空值. Entity:{typeof(TEntity).Name}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}");
                }

                return entities;
            }
            else
            {
                logger.LogError($"锁未能占用. Entity:{typeof(TEntity).Name}, dimensionKeyName:{dimensionKeyName},dimensionKeyValues:{dimensionKeyValues.ToJoinedString(",")}, Lock Status:{@lock.Status}");

                return await dbRetrieve(database).ConfigureAwait(false);
            }

        }

        public static async Task<TResult?> CacheAsideAsync<TResult>(
            CachedItem<TResult> cacheItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve,
            ICache cache, IMemoryLockManager memoryLockManager, IDatabase database, ILogger logger)
            where TResult : class
        {
            //Cache First
            TResult? result = await cacheItem.GetFromAsync(cache).ConfigureAwait(false);

            if (result != null)
            {
                return result;
            }

            using var @lock = memoryLockManager.Lock(cacheItem.ResourceType, cacheItem.CacheKey, OccupiedTime, PatienceTime);

            if (@lock.IsAcquired)
            {
                //Double Check
                result = await cacheItem.GetFromAsync(cache).ConfigureAwait(false);

                if (result != null)
                {
                    return result;
                }

                TResult dbRt = await dbRetrieve(database).ConfigureAwait(false);
                UtcNowTicks now = TimeUtil.UtcNowTicks;


                // 如果TResult是集合类型，可能会存入空集合。而在EntityCache中是不会存入空集合的。
                //这样设计是合理的，因为EntityCache是按Entity角度，存入的Entity会复用，就像一个KVStore一样，而CachedItem纯粹是一个查询结果，不思考查询结果的内容。
                if (dbRt != null)
                {
                    UpdateCache(cacheItem.Value(dbRt).Timestamp(now), cache);
                    logger.LogInformation($"缓存 Missed. Entity:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}");
                }
                else
                {
                    logger.LogInformation($"查询到空值. Entity:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}");
                }

                return dbRt;
            }
            else
            {
                logger.LogCritical($"锁未能占用. Entity:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}, Lock Status:{@lock.Status}");

                return await dbRetrieve(database).ConfigureAwait(false);
            }
        }

        public static void InvalidateCache<TEntity>(IEnumerable<TEntity> entities, ICache cache) where TEntity : Entity, new()
        {
            if (ICache.IsEntityEnabled<TEntity>())
            {
                cache.RemoveEntitiesAsync(entities).Fire();
            }
        }

        public static void InvalidateCache<TResult>(CachedItem<TResult> cacheItem, ICache cache) where TResult : class
        {
            cacheItem.RemoveFromAsync(cache).Fire();
        }

        private static void UpdateCache<TResult>(CachedItem<TResult> cacheItem, ICache cache) where TResult : class
        {
            cacheItem.SetToAsync(cache).Fire();
        }

        private static void UpdateCache<TEntity>(IEnumerable<TEntity> entities, ICache cache) where TEntity : Entity, new()
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

            cache.SetEntitiesAsync(entities).Fire();

            #endregion

        }
    }
}

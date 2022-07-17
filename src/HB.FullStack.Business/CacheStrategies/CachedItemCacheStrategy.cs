using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Common.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.DBModels;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Repository.CacheStrategies
{
    public static class CachedItemCacheStrategy
    {
        public static async Task<TResult?> GetUsingCacheAsideAsync<TResult>(
            CachedItem<TResult> cacheItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve,
            ICache cache, IMemoryLockManager memoryLockManager, IDatabase database, ILogger logger)
            where TResult : class
        {
            //Cache First
            TResult? result = await cache.GetAsync<TResult>(cacheItem).ConfigureAwait(false);

            if (result != null)
            {
                return result;
            }

            using var @lock = memoryLockManager.Lock(cacheItem.CachedType, cacheItem.CacheKey, Consts.OccupiedTime, Consts.PatienceTime);

            if (@lock.IsAcquired)
            {
                //Double Check ：
                //如果大量请求，请求同一项资源。那么十分有用。
                //根据log来kan是否省略，如果比较少，其实可以让db承压，而且timestamp的检查让重复SetCache是没问题的
                result = await cache.GetAsync(cacheItem).ConfigureAwait(false);

                if (result != null)
                {
                    logger.LogInformation("//TODO: 请求同一项CachedItem，等待锁并获取锁后，发现Cache已存在。Model:{ModelName},CacheKey:{CacheKey}",
                        cacheItem.GetType().Name, cacheItem.CacheKey);
                    return result;
                }

                TResult dbRt = await dbRetrieve(database).ConfigureAwait(false);

                // 如果TResult是集合类型，可能会存入空集合。而在ModelCache中是不会存入空集合的。
                //这样设计是合理的，因为ModelCache是按Model角度，存入的Model会复用，就像一个KVStore一样，而CachedItem纯粹是一个查询结果，不思考查询结果的内容。
                if (dbRt != null)
                {
                    long timestamp = (dbRt as TimestampDBModel)?.Timestamp ?? TimeUtil.UtcNowTicks;
                    SetCache(cacheItem.SetValue(dbRt).SetTimestamp(timestamp), cache);
                    logger.LogInformation("缓存 Missed. Model:{ModelName}, CacheKey:{CacheKey}", cacheItem.GetType().Name, cacheItem.CacheKey);
                }
                else
                {
                    logger.LogInformation("查询到空值. Model:{ModelName}, CacheKey:{CacheKey}", cacheItem.GetType().Name, cacheItem.CacheKey);
                }

                return dbRt;
            }
            else
            {
                logger.LogCritical("锁未能占用. Model:{ModelName}, CacheKey:{CacheKey}, Lock Status:{LockStatus}",
                    cacheItem.GetType().Name, cacheItem.CacheKey, @lock.Status);

                return await dbRetrieve(database).ConfigureAwait(false);
            }
        }

        public static void InvalidateCache(ICachedItem cachedItem, ICache cache)
        {
            cache.RemoveAsync(cachedItem).SafeFireAndForget(OnException);
        }

        private static void SetCache<TResult>(CachedItem<TResult> cachedItem, ICache cache) where TResult : class
        {
            cache.SetAsync(cachedItem).SafeFireAndForget(OnException);
        }

        internal static void InvalidateCache(IEnumerable<ICachedItem> cachedItems, ICache cache)
        {
            cache.RemoveAsync(cachedItems).SafeFireAndForget(OnException);
        }

        private static void OnException(Exception ex)
        {
            //TODO: 是否要停用缓存？停机等等。
            GlobalSettings.Logger.LogCritical(ex, "CachedItemCacheStrategy 中缓存Update或者Invalidate出错，Cache中可能出现脏数据/旧数据.请重建缓存或其他操作");
        }

    }
}

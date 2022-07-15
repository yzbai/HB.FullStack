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
    public static class CachedCollectionItemCacheStrategy
    {
        public static async Task<TResult?> GetUsingCacheAsideAsync<TResult>(
            CachedCollectionItem<TResult> cacheCollectionItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve,
            ICache cache, IMemoryLockManager memoryLockManager, IDatabase database, ILogger logger)
            where TResult : class
        {

            //Cache First
            TResult? result = await cache.GetAsync(cacheCollectionItem).ConfigureAwait(false);

            if (result != null)
            {
                return result;
            }

            using var @lock = memoryLockManager.Lock(cacheCollectionItem.CollectionKey, cacheCollectionItem.ItemKey, Consts.OccupiedTime, Consts.PatienceTime);

            if (@lock.IsAcquired)
            {
                //Double Check
                result = await cache.GetAsync(cacheCollectionItem).ConfigureAwait(false);

                if (result != null)
                {
                    logger.LogInformation($"//TODO: 请求同一项CachedCollectionItem，等待锁并获取锁后，发现Cache已存在。Model:{cacheCollectionItem.GetType().Name},CacheKey:{cacheCollectionItem.CollectionKey}");
                    return result;
                }

                TResult dbRt = await dbRetrieve(database).ConfigureAwait(false);

                // 如果TResult是集合类型，可能会存入空集合。而在ModelCache中是不会存入空集合的。
                //这样设计是合理的，因为ModelCache是按Model角度，存入的Model会复用，就像一个KVStore一样，而CachedItem纯粹是一个查询结果，不思考查询结果的内容。
                if (dbRt != null)
                {
                    long timestamp = (dbRt as TimestampDBModel)?.Timestamp ?? TimeUtil.UtcNowTicks;
                    SetCache(cacheCollectionItem.SetValue(dbRt).SetTimestamp(timestamp), cache);
                    logger.LogInformation($"缓存 Missed. Model:{cacheCollectionItem.GetType().Name}, CacheCollectionKey:{cacheCollectionItem.CollectionKey}, CacheItemKey:{cacheCollectionItem.ItemKey}");
                }
                else
                {
                    logger.LogInformation($"查询到空值. Model:{cacheCollectionItem.GetType().Name}, CacheCollectionKey:{cacheCollectionItem.CollectionKey}, CacheItemKey:{cacheCollectionItem.ItemKey}");
                }

                return dbRt;
            }
            else
            {
                logger.LogCritical($"锁未能占用. Model:{cacheCollectionItem.GetType().Name}, CacheCollectionKey:{cacheCollectionItem.CollectionKey}, CacheItemKey:{cacheCollectionItem.ItemKey}, Lock Status:{@lock.Status}");

                return await dbRetrieve(database).ConfigureAwait(false);
            }
        }

        public static void InvalidateCache(CachedCollectionItem cachedCollectionItem, ICache cache)
        {
            cache.RemoveAsync(cachedCollectionItem).SafeFireAndForget(OnException);
        }

        public static void InvalidateCache(IEnumerable<CachedCollectionItem> cachedCollectionItems, ICache cache)
        {
            cache.RemoveAsync(cachedCollectionItems).SafeFireAndForget(OnException);
        }

        public static void InvalidateCacheCollection(string collectionKey, ICache cache)
        {
            cache.RemoveCollectionAsync(collectionKey).SafeFireAndForget(OnException);
        }

        private static void SetCache<TResult>(CachedCollectionItem<TResult> cachedItem, ICache cache) where TResult : class
        {
            cache.SetAsync(cachedItem).SafeFireAndForget(OnException);
        }

        private static void OnException(Exception ex)
        {
            //TODO: 是否要停用缓存？停机等等。
            GlobalSettings.Logger.LogCritical(ex, "CachedCollectionItemCacheStrategy中缓存Update或者Invalidate出错，Cache中可能出现脏数据/旧数据.请重建缓存或其他操作");
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Repository
{
    public static class CachedItemCacheStrategy
    {          
        public static async Task<TResult?> CacheAsideAsync<TResult>(
            CachedItem<TResult> cacheItem, Func<IDatabaseReader, Task<TResult>> dbRetrieve,
            ICache cache, IMemoryLockManager memoryLockManager, IDatabase database, ILogger logger)
            where TResult : class
        {
            //Cache First
            TResult? result = await cache.GetAsync(cacheItem).ConfigureAwait(false);

            if (result != null)
            {
                return result;
            }

            using var @lock = memoryLockManager.Lock(cacheItem.CachedType, cacheItem.CacheKey, Consts.OccupiedTime, Consts.PatienceTime);

            if (@lock.IsAcquired)
            {
                //Double Check
                result = await cache.GetAsync(cacheItem).ConfigureAwait(false);

                if (result != null)
                {
                    return result;
                }

                TResult dbRt = await dbRetrieve(database).ConfigureAwait(false);
                UtcNowTicks now = TimeUtil.UtcNowTicks;


                // 如果TResult是集合类型，可能会存入空集合。而在ModelCache中是不会存入空集合的。
                //这样设计是合理的，因为ModelCache是按Model角度，存入的Model会复用，就像一个KVStore一样，而CachedItem纯粹是一个查询结果，不思考查询结果的内容。
                if (dbRt != null)
                {
                    UpdateCache(cacheItem.Value(dbRt).Timestamp(now), cache);
                    logger.LogInformation($"缓存 Missed. Model:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}");
                }
                else
                {
                    logger.LogInformation($"查询到空值. Model:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}");
                }

                return dbRt;
            }
            else
            {
                logger.LogCritical($"锁未能占用. Model:{cacheItem.GetType().Name}, CacheKey:{cacheItem.CacheKey}, Lock Status:{@lock.Status}");

                return await dbRetrieve(database).ConfigureAwait(false);
            }
        }
        
        public static void InvalidateCache(CachedItem cachedItem, ICache cache)
        {
            cache.RemoveAsync(cachedItem).SafeFireAndForget(OnException);
        }


        private static void UpdateCache<TResult>(CachedItem<TResult> cachedItem, ICache cache) where TResult : class
        {
            cache.SetAsync(cachedItem).SafeFireAndForget(OnException);
        }

        internal static void InvalidateCache(IEnumerable<CachedItem> cachedItems,UtcNowTicks utcNowTicks, ICache cache)
        {
            cache.RemoveAsync(cachedItems, utcNowTicks).SafeFireAndForget(OnException);
        }

        private static void OnException(Exception ex)
        {
            //TODO: 是否要停用缓存？停机等等。
            GlobalSettings.Logger.LogCritical(ex, "CachedItemCacheStrategy 中缓存Update或者Invalidate出错，Cache中可能出现脏数据/旧数据.请重建缓存或其他操作");
        }

    }
}

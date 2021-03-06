﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Repository
{
    public static class CachedItemCacheStrategy
    {
        /// <summary>
        /// CacheAsideAsync
        /// </summary>
        /// <param name="cacheItem"></param>
        /// <param name="dbRetrieve"></param>
        /// <param name="cache"></param>
        /// <param name="memoryLockManager"></param>
        /// <param name="database"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="RepositoryException"></exception>
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

            using var @lock = memoryLockManager.Lock(cacheItem.ResourceType, cacheItem.CacheKey, Consts.OccupiedTime, Consts.PatienceTime);

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

        /// <summary>
        /// InvalidateCache
        /// </summary>
        /// <param name="cacheItem"></param>
        /// <param name="cache"></param>
        /// <exception cref="CacheException">Ignore.</exception>
        public static void InvalidateCache<TResult>(CachedItem<TResult> cacheItem, ICache cache) where TResult : class
        {
            cacheItem.RemoveFromAsync(cache).Fire();
        }

        /// <summary>
        /// UpdateCache
        /// </summary>
        /// <param name="cacheItem"></param>
        /// <param name="cache"></param>
        /// <exception cref="CacheException"></exception>
        private static void UpdateCache<TResult>(CachedItem<TResult> cacheItem, ICache cache) where TResult : class
        {
            cacheItem.SetToAsync(cache).Fire();
        }
    }
}

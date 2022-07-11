using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Common.Cache.CacheModels;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Repository
{
    internal static class ModelCacheStrategy
    {
        public static async Task<IEnumerable<TModel>> CacheAsideAsync<TModel>(string dimensionKeyName, IEnumerable dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TModel>>> dbRetrieve,
            IDatabase database, Cache.ICache cache, IMemoryLockManager memoryLockManager, ILogger logger) where TModel : Common.Cache.CacheModels.ICacheModel, new()
        {

            if (!cache.IsModelEnabled<TModel>())
            {
                return await dbRetrieve(database).ConfigureAwait(false);
            }

            try
            {
                (IEnumerable<TModel>? cachedModels, bool allExists) = await cache.GetModelsAsync<TModel>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

                if (allExists)
                {
                    return cachedModels!;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                logger.LogCacheGetError(dimensionKeyName, dimensionKeyValues, ex);
            }

            //常规做法是先获取锁（参看历史）。
            //但如果仅从当前dimension来锁的话，有可能被别人从其他dimension操作同一个model，
            //所以这里改变常规做法，先做database retrieve

            //以上是针对无version版本cache的。现在不用担心从其他dimension操作同一个model了，cache会自带version来判断。
            //而且如果刚开始很多请求直接打到数据库上，数据库撑不住，还是得加锁。
            //但可以考虑加单机本版的锁就可，这个锁主要为了降低数据库压力，不再是为了数据一致性（带version的cache自己解决）。
            //所以可以使用单机版本的锁即可。一个主机同时放一个db请求，还是没问题的。

            List<string> resources = new List<string>();
            foreach (object dimensionKeyValue in dimensionKeyValues)
            {
                resources.Add(dimensionKeyName + dimensionKeyValue.ToString());
            }

            using var @lock = memoryLockManager.Lock(typeof(TModel).Name, resources, Consts.OccupiedTime, Consts.PatienceTime);

            if (@lock.IsAcquired)
            {

                try
                {
                    //Double check
                    (IEnumerable<TModel>? cachedModels, bool allExists) = await cache.GetModelsAsync<TModel>(dimensionKeyName, dimensionKeyValues).ConfigureAwait(false);

                    if (allExists)
                    {
                        return cachedModels!;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    logger.LogCacheGetError(dimensionKeyName, dimensionKeyValues, ex);
                }

                IEnumerable<TModel> models = await dbRetrieve(database).ConfigureAwait(false);

                if (models.IsNotNullOrEmpty())
                {
                    UpdateCache(models, cache);

                    logger.LogCacheMissed(typeof(TModel).Name, dimensionKeyName, dimensionKeyValues);
                }
                else
                {
                    logger.LogCacheGetEmpty(typeof(TModel).Name, dimensionKeyName, dimensionKeyValues);
                }

                return models;
            }
            else
            {
                logger.LogCacheLockAcquireFailed(typeof(TModel).Name, dimensionKeyName, dimensionKeyValues, @lock.Status.ToString());

                return await dbRetrieve(database).ConfigureAwait(false);
            }

        }

        private static void UpdateCache<TModel>(IEnumerable<TModel> models, Cache.IModelCache cache) where TModel : Common.Cache.CacheModels.ICacheModel, new()
        {
            #region 普通缓存，加锁的做法
            //using IDistributedLock distributedLock = await _lockManager.LockModelsAsync(models, OccupiedTime, PatienceTime).ConfigureAwait(false);

            //if (!distributedLock.IsAcquired)
            //{
            //    _logger.LogWarning($"锁未能占用. Model:{nameof(TModel)}, Guids:{models.Select(e => e.Guid).ToJoinedString(",")}, Lock Status:{distributedLock.Status}");
            //    return;
            //}

            ////Double Check
            //(IEnumerable<TModel>? cachedModels, bool allExists) = await _cache.GetModelsAsync<TModel>(models).ConfigureAwait(false);

            ////版本检查
            //if (allExis ts)
            //{
            //    return;
            //}

            //_logger.LogInformation($"Cache Missed. Model:{nameof(TModel)}, Guids:{models.Select(e => e.Guid).ToJoinedString(",")}");

            //await _cache.SetModelsAsync(models).ConfigureAwait(false);
            #endregion

            #region 有版本控制的Cache. 就一句话，爽不爽

            cache.SetModelsAsync(models).SafeFireAndForget(OnException);

            #endregion

        }

        public static void InvalidateCache<TModel>(IEnumerable<TModel> models, Cache.IModelCache cache) where TModel : Common.Cache.CacheModels.ICacheModel, new()
        {
            if (cache.IsModelEnabled<TModel>())
            {
                cache.RemoveModelsAsync(models).SafeFireAndForget(OnException);
            }
        }

        private static void OnException(Exception ex)
        {
            //TODO: 是否要停用缓存？停机等等。
            GlobalSettings.Logger.LogCritical(ex, "ModelCacheStrategy 中缓存Update或者Invalidate出错，Cache中可能出现脏数据/旧数据.请重建缓存或其他操作");
        }
    }
}

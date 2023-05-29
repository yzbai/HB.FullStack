using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Cache;
using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Repository.CacheStrategies
{
    internal static class ModelCacheStrategy
    {
        public static async Task<IList<TModel>> GetUsingCacheAsideAsync<TModel>(
            string keyName,
            IEnumerable keyValues,
            Func<IDbReader, Task<IList<TModel>>> dbRetrieve,
            IDatabase database,
            ICache cache,
            IMemoryLockManager memoryLockManager,
            ILogger logger) where TModel : IModel
        {
            if (!cache.IsModelCachable<TModel>())
            {
                return await dbRetrieve(database).ConfigureAwait(false);
            }

            try
            {
                (IList<TModel>? cachedModels, bool allExists) = await cache.GetModelsAsync<TModel>(keyName, keyValues).ConfigureAwait(false);

                if (allExists)
                {
                    return cachedModels!;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                logger.LogCacheGetError(keyName, keyValues, ex);
            }

            //常规做法是先获取锁（参看历史）。
            //但如果仅从当前dimension来锁的话，有可能被别人从其他dimension操作同一个model，
            //所以这里改变常规做法，先做database retrieve

            //以上是针对无version版本cache的。现在不用担心从其他dimension操作同一个model了，cache会自带version来判断。
            //而且如果刚开始很多请求直接打到数据库上，数据库撑不住，还是得加锁。
            //但可以考虑加单机本版的锁就可，这个锁主要为了降低数据库压力，不再是为了数据一致性（带version的cache自己解决）。
            //所以可以使用单机版本的锁即可。一个主机同时放一个db请求，还是没问题的。

            List<string> resources = new List<string>();

            foreach (object keyValue in keyValues)
            {
                resources.Add(keyName + keyValue.ToString());
            }

            using var @lock = memoryLockManager.Lock(typeof(TModel).Name, resources, Consts.OccupiedTime, Consts.PatienceTime);

            if (@lock.IsAcquired)
            {

                try
                {
                    //Double check
                    (IList<TModel>? cachedModels, bool allExists) = await cache.GetModelsAsync<TModel>(keyName, keyValues).ConfigureAwait(false);

                    if (allExists)
                    {
                        logger.LogInformation("//TODO: 请求同一项Cache，等待锁并获取锁后，发现Cache已存在。Model:{Model},KeyName:{KeyName}, KeyValues:{@KeyValues}", typeof(TModel).Name, keyName, keyValues);
                        return cachedModels!;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    logger.LogCacheGetError(keyName, keyValues, ex);
                }

                IList<TModel> models = await dbRetrieve(database).ConfigureAwait(false);

                if (models.IsNotNullOrEmpty())
                {
                    SetCache(models, cache);

                    logger.LogCacheMissed(typeof(TModel).Name, keyName, keyValues);
                }
                else
                {
                    logger.LogCacheGetEmpty(typeof(TModel).Name, keyName, keyValues);
                }

                return models;
            }
            else
            {
                logger.LogCacheLockAcquireFailed(typeof(TModel).Name, keyName, keyValues, @lock.Status.ToString());

                return await dbRetrieve(database).ConfigureAwait(false);
            }
        }

        private static void SetCache<TModel>(IEnumerable<TModel> models, ICache cache) where TModel : IModel
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

        public static void InvalidateCache<T>(IEnumerable<T> models, ICache cache) where T : IDbModel
        {
            cache.RemoveModelsAsync(models).SafeFireAndForget(OnException);
        }

        public static void InvalidateCache<T>(T model, ICache cache) where T : IDbModel
        {
            cache.RemoveModelAsync(model).SafeFireAndForget(OnException);
        }

        public static void InvalidateCache<T>(IEnumerable<PropertyChangePack> cps, DbModelDef modelDef, ICache cache)
        {
            List<object> ids = new List<object>();

            foreach (var cp in cps)
            {
                if (cp.AddtionalProperties.TryGetValue(nameof(IDbModel.Id), out JsonElement idElement))
                {
                    object? id = SerializeUtil.FromJsonElement(modelDef.PrimaryKeyPropertyDef.Type, idElement);

                    if (id != null)
                    {
                        ids.Add(id);
                    }
                }
            }

            cache.RemoveModelByIdsAsync<T>(ids).SafeFireAndForget(OnException);
        }

        private static void OnException(Exception ex)
        {
            //TODO: 是否要停用缓存？停机等等。
            Globals.Logger.LogCritical(ex, "ModelCacheStrategy 中缓存Update或者Invalidate出错，Cache中可能出现脏数据/旧数据.请重建缓存或其他操作");
        }
    }
}

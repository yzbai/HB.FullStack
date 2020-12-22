using System;
using System.Collections;
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
    internal static class EntityCacheStrategy
    {
        public static async Task<IEnumerable<TEntity>> CacheAsideAsync<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, Func<IDatabaseReader, Task<IEnumerable<TEntity>>> dbRetrieve,
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

            List<string> resources = new List<string>();
            foreach (object dimensionKeyValue in dimensionKeyValues)
            {
                resources.Add(dimensionKeyName + dimensionKeyValue.ToString());
            }

            using var @lock = memoryLockManager.Lock(typeof(TEntity).Name, resources, Consts.OccupiedTime, Consts.PatienceTime);

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

        public static void InvalidateCache<TEntity>(IEnumerable<TEntity> entities, ICache cache) where TEntity : Entity, new()
        {
            if (ICache.IsEntityEnabled<TEntity>())
            {
                cache.RemoveEntitiesAsync(entities).Fire();
            }
        }

    }
}

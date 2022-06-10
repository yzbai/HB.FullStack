using HB.FullStack.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Cache
{
    public static class ICacheEntitiesExtensions
    {
        public static Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(this ICache cache, IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            string dimensionKeyName = entityDef.KeyProperty.Name;
            var dimensionKeyValues = entities.Select(e => entityDef.KeyProperty.GetValue(e)).ToList();

            return cache.GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token);
        }

        public static async Task<(TEntity?, bool)> GetEntityAsync<TEntity>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, CancellationToken token = default) where TEntity : Entity, new()
        {
            (IEnumerable<TEntity>? results, bool exist) = await cache.GetEntitiesAsync<TEntity>(dimensionKeyName, new object[] { dimensionKeyValue }, token).ConfigureAwait(false);

            if (exist)
            {
                return (results!.ElementAt(0), true);
            }

            return (null, false);
        }

        public static Task<(TEntity?, bool)> GetEntityAsync<TEntity>(this ICache cache, TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.KeyProperty.Name;
            string dimensionKeyValue = entityDef.KeyProperty.GetValue(entity)!.ToString()!;

            return cache.GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>      
        public static Task RemoveEntitiesAsync<TEntity>(this ICache cache, IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            if (!entities.Any())
            {
                return Task.CompletedTask;
            }

            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            string dimensionKeyName = entityDef.KeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = entities.Select(e => entityDef.KeyProperty.GetValue(e)!.ToString()!).ToList();
            IEnumerable<int> updatedVersions = entities.Select(e => e.Version).ToList();

            return cache.RemoveEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, updatedVersions, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        public static Task RemoveEntityAsync<TEntity>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, int updatedVersion, CancellationToken token = default) where TEntity : Entity, new()
        {
            return cache.RemoveEntitiesAsync<TEntity>(dimensionKeyName, new object[] { dimensionKeyValue }, new int[] { updatedVersion }, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        public static Task RemoveEntityAsync<TEntity>(this ICache cache, TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.KeyProperty.Name;
            string dimensionKeyValue = entityDef.KeyProperty.GetValue(entity)!.ToString()!;

            return cache.RemoveEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, entity.Version, token);
        }

        /// <summary>
        /// 返回是否成功更新。false是数据版本小于缓存中的
        /// </summary>
        public static async Task<bool> SetEntityAsync<TEntity>(this ICache cache, TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            IEnumerable<bool> results = await cache.SetEntitiesAsync<TEntity>(new TEntity[] { entity }, token).ConfigureAwait(false);

            return results.ElementAt(0);
        }
    }
}

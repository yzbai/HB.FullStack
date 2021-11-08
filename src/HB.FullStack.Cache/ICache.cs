

using HB.FullStack.Common;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace HB.FullStack.Cache
{
    /// <summary>
    /// string,int,generic 都可以存储空值
    /// Entity操作不可以 
    /// </summary>
    public interface ICache
    {
        void Close();

        void Dispose();

        #region Entities

        
        Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, CancellationToken token = default) where TEntity : Entity, new();

        
        Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            string dimensionKeyName = entityDef.KeyProperty.Name;
            var dimensionKeyValues = entities.Select(e => entityDef.KeyProperty.GetValue(e)).ToList();

            return GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token);
        }

        
        async Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, object dimensionKeyValue, CancellationToken token = default) where TEntity : Entity, new()
        {
            (IEnumerable<TEntity>? results, bool exist) = await GetEntitiesAsync<TEntity>(dimensionKeyName, new object[] { dimensionKeyValue }, token).ConfigureAwait(false);

            if (exist)
            {
                return (results!.ElementAt(0), true);
            }

            return (null, false);
        }

        
        Task<(TEntity?, bool)> GetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.KeyProperty.Name;
            string dimensionKeyValue = entityDef.KeyProperty.GetValue(entity)!.ToString()!;

            return GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token);
        }


        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, IEnumerable<int> updatedVersions, CancellationToken token = default) where TEntity : Entity, new();

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>      
        Task RemoveEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            if (!entities.Any())
            {
                return Task.CompletedTask;
            }

            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            string dimensionKeyName = entityDef.KeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = entities.Select(e => entityDef.KeyProperty.GetValue(e)!.ToString()!).ToList();
            IEnumerable<int> updatedVersions = entities.Select(e => e.Version).ToList();

            return RemoveEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, updatedVersions, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveEntityAsync<TEntity>(string dimensionKeyName, object dimensionKeyValue, int updatedVersion, CancellationToken token = default) where TEntity : Entity, new()
        {
            return RemoveEntitiesAsync<TEntity>(dimensionKeyName, new object[] { dimensionKeyValue }, new int[] { updatedVersion }, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.KeyProperty.Name;
            string dimensionKeyValue = entityDef.KeyProperty.GetValue(entity)!.ToString()!;

            return RemoveEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, entity.Version, token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entities"></param>
        /// <param name="token"></param>
        /// <returns>是否成功更新。false是数据版本小于缓存中的</returns>

        Task<IEnumerable<bool>> SetEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new();

        /// <returns>是否成功更新。false是数据版本小于缓存中的</returns>
        async Task<bool> SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            IEnumerable<bool> results = await SetEntitiesAsync<TEntity>(new TEntity[] { entity }, token).ConfigureAwait(false);

            return results.ElementAt(0);
        }
        
        static bool IsEntityEnabled<TEntity>() where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            return entityDef.IsCacheable;
        }

        #endregion

        #region Timestamp Cache
        
        Task<byte[]?> GetAsync(string key, CancellationToken token = default);

        /// <summary>
        /// utcTicks是指数据刚刚从数据库中取出来后的时间
        /// 所以数据库取出后需要赶紧记录UtcNowTicks
        /// </summary>
        Task<bool> SetAsync(string key, byte[] value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// 返回是否找到了
        /// utcTicks是指数据刚刚从数据库中取出来后的时间
        /// </summary>       
        Task<bool> RemoveAsync(string key, UtcNowTicks utcTicks, CancellationToken token = default);

        Task<bool> RemoveAsync(string[] keys, UtcNowTicks utcTicks, CancellationToken token = default);

        /// <summary>
        /// 删除以keyPrefix开头的key
        /// </summary>
        /// <param name="keyPrefix"></param>
        /// <param name="utcTikcs"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task RemoveByKeyPrefixAsync(string keyPrefix, UtcNowTicks utcTikcs, CancellationToken token = default);

        #endregion
    }
}

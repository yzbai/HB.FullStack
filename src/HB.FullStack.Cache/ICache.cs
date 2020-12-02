using HB.FullStack.Common.Entities;

using Microsoft.Extensions.Caching.Distributed;

using System;
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

        Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default) where TEntity : Entity, new();

        Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            string dimensionKeyName = entityDef.GuidKeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = entities.Select(e => entityDef.GuidKeyProperty.GetValue(e).ToString());

            return GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token);
        }

        async Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default) where TEntity : Entity, new()
        {
            (IEnumerable<TEntity>? results, bool exist) = await GetEntitiesAsync<TEntity>(dimensionKeyName, new string[] { dimensionKeyValue }, token).ConfigureAwait(false);

            if (exist)
            {
                return (results.ElementAt(0), true);
            }

            return (null, false);
        }

        Task<(TEntity?, bool)> GetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.GuidKeyProperty.Name;
            string dimensionKeyValue = entityDef.GuidKeyProperty.GetValue(entity).ToString();

            return GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token);
        }


        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dimensionKeyName"></param>
        /// <param name="dimensionKeyValues"></param>
        /// <param name="updatedVersions"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task RemoveEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, IEnumerable<int> updatedVersions, CancellationToken token = default) where TEntity : Entity, new();

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            string dimensionKeyName = entityDef.GuidKeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = entities.Select(e => entityDef.GuidKeyProperty.GetValue(e).ToString());
            IEnumerable<int> updatedVersions = entities.Select(e => e.Version);

            return RemoveEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, updatedVersions, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, int updatedVersion, CancellationToken token = default) where TEntity : Entity, new()
        {
            return RemoveEntitiesAsync<TEntity>(dimensionKeyName, new string[] { dimensionKeyValue }, new int[] { updatedVersion }, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.GuidKeyProperty.Name;
            string dimensionKeyValue = entityDef.GuidKeyProperty.GetValue(entity).ToString();

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


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="token"></param>
        /// <returns>是否成功更新。false是数据版本小于缓存中的</returns>
        async Task<bool> SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            IEnumerable<bool> results = await SetEntitiesAsync<TEntity>(new TEntity[] { entity }, token).ConfigureAwait(false);

            return results.ElementAt(0);
        }

        //static bool IsEntityBatchEnabled<TEntity>() where TEntity : Entity, new()
        //{
        //    CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

        //    return entityDef.IsBatchEnabled;
        //}

        static bool IsEntityEnabled<TEntity>() where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            return entityDef.IsCacheable;
        }

        #endregion

        #region Timestamp Cache

        Task<byte[]?> GetAsync(string key, CancellationToken token = default(CancellationToken));

        Task<bool> SetAsync(string key, byte[] value, long utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// 返回是否找到了
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timestampInUnixMilliseconds"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string key, long utcTicks, CancellationToken token = default(CancellationToken));

        #endregion
    }
}

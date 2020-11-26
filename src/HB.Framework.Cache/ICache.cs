using HB.Framework.Common.Entities;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Cache
{
    /// <summary>
    /// string,int,generic 都可以存储空值
    /// Entity操作不可以 
    /// </summary>
    public interface ICache : IDistributedCache
    {
        void Close();
        void Dispose();
        Task<(T?, bool)> GetAsync<T>(string key, CancellationToken token = default) where T : class;
        Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default) where TEntity : Entity, new();

        Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            string dimensionKeyName = entityDef.GuidKeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = entities.Select(e => entityDef.GuidKeyProperty.GetValue(e).ToString());

            return GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token);
        }
        Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default) where TEntity : Entity, new();

        Task<(TEntity?, bool)> GetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.GuidKeyProperty.Name;
            string dimensionKeyValue = entityDef.GuidKeyProperty.GetValue(entity).ToString();

            return GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token);
        }

        Task<(int, bool)> GetIntAsync(string key, CancellationToken token = default);
        Task<(string?, bool)> GetStringAsync(string key, CancellationToken token = default);
        Task<bool> IsExistThenRemoveAsync(string key, CancellationToken token = default);
        Task RemoveEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default) where TEntity : Entity, new();
        Task RemoveEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default) where TEntity : Entity, new();

        Task RemoveEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            string dimensionKeyName = entityDef.GuidKeyProperty.Name;
            string dimensionKeyValue = entityDef.GuidKeyProperty.GetValue(entity).ToString();

            return RemoveEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token);
        }
        Task SetAsync<T>(string key, T? value, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class;

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
        Task<bool> SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new();
        Task SetIntAsync(string key, int value, DistributedCacheEntryOptions options, CancellationToken token = default);
        Task SetStringAsync(string key, string? value, DistributedCacheEntryOptions options, CancellationToken token = default);

        static bool IsBatchEnabled<TEntity>() where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            return entityDef.IsBatchEnabled;
        }

        static bool IsEnabled<TEntity>() where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            return entityDef.IsCacheable;
        }
    }
}

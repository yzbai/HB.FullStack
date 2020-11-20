using HB.Framework.Common.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Common.Cache
{
    public interface ICache : IDistributedCache
    {
        #region Basic

        Task SetIntAsync(string key, int value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken));

        Task<(int, bool)> GetIntAsync(string key, CancellationToken token = default(CancellationToken));

        Task SetStringAsync(string key, string? value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken));

        Task<(string?, bool)> GetStringAsync(string key, CancellationToken token = default(CancellationToken));

        #endregion

        #region Generic

        /// <summary>
        /// 可以存储空值
        /// </summary>
        Task SetAsync<T>(string key, T? value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken)) where T : class;

        /// <summary>
        /// 返回（数据，是否存在）
        /// </summary>
        Task<(T?, bool)> GetAsync<T>(string key, CancellationToken token = default(CancellationToken)) where T : class;

        /// <summary>
        /// 如果存在就移除
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns>true:存在; false: 不存在</returns>
        /// <exception cref="CacheException"></exception>
        Task<bool> IsExistThenRemoveAsync(string key, CancellationToken token = default(CancellationToken));

        #endregion Generic

        #region Entity

        /// <summary>
        /// 返回值：（cached，isExist）
        /// </summary>
        Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken))
            where TEntity : Entity, new();

        Task SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new();

        Task RemoveEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new();

        bool IsEnabled<TEntity>() where TEntity : Entity, new();

        #endregion

        #region Batch Entity

        /// <summary>
        /// (结果：全部是否存在)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="cacheKeyName"></param>
        /// <param name="cacheKeyValue"></param>
        /// <returns></returns>
        Task<(IEnumerable<TEntity?>, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken))
            where TEntity : Entity, new();

        Task SetEntitiesAsync<TEntity>(IEnumerable<TEntity> entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new();

        /// <summary>
        /// 可以存储空值
        /// </summary>
        Task SetEntitiesAsync<TEntity>(IEnumerable<string> dimensionKeyNames, IEnumerable<string> dimensionKeyValues, IEnumerable<TEntity?> entities, CancellationToken token = default(CancellationToken))
            where TEntity : Entity, new();

        Task RemoveEntitiesAsync<TEntity>(IEnumerable<string> dimensionKeyNames, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new();

        #endregion
    }
}

﻿

using HB.FullStack.Common;
using HB.FullStack.Common.Cache.CacheModels;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace HB.FullStack.Cache
{
    /// <summary>
    /// string,int,generic 都可以存储空值
    /// Model操作不可以
    /// </summary>
    public interface ICache
    {
        void Close();

        void Dispose();

        #region Models

        //NOTICE: 因为.net standard 2.0 不支持static member in interface，所以定义了ICacheExtensions
        //static bool IsModelEnabled<TModel>() where TModel : Model, new()
        //{
        //    CacheModelDef modelDef = CacheModelDefFactory.Get<TModel>();

        //    return modelDef.IsCacheable;
        //}

        Task<(IEnumerable<TCacheModel>?, bool)> GetModelsAsync<TCacheModel>(string dimensionKeyName, IEnumerable dimensionKeyValues, CancellationToken token = default) where TCacheModel : ICacheModel, new();

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        Task RemoveModelsAsync<TCacheModel>(string dimensionKeyName, IEnumerable dimensionKeyValues, IEnumerable<int> updatedVersions, CancellationToken token = default) where TCacheModel : ICacheModel, new();

        /// <summary>
        /// 返回是否成功更新。false是数据版本小于缓存中的
        /// </summary>
        Task<IEnumerable<bool>> SetModelsAsync<TCacheModel>(IEnumerable<TCacheModel> models, CancellationToken token = default) where TCacheModel : ICacheModel, new();

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

        #endregion

        #region Collection

        Task<byte[]?> GetFromCollectionAsync(string collectionKey, string itemKey, CancellationToken token = default);

        Task<bool> SetToCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, IEnumerable<byte[]> itemValues, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default);

        Task<bool> RemoveFromCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, UtcNowTicks utcTicks, CancellationToken token = default);

        Task<bool> RemoveCollectionAsync(string collectionKey, CancellationToken token = default);

        #endregion
    }


    /// <summary>
    /// //NOTICE: 因为.net standard 2.0 不支持static member in interface，所以定义了ICacheExtensions
    /// </summary>
    public static class ICacheExtensions
    {
        public static bool IsModelEnabled<TCacheModel>(this ICache cache) where TCacheModel : ICacheModel, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TCacheModel>();

            return modelDef.IsCacheable;
        }
    }
}

﻿using HB.FullStack.Common;
using HB.FullStack.Common.Cache.CacheModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Cache
{
    public static class ICacheModelsExtensions
    {
        public static Task<(IEnumerable<TCacheModel>?, bool)> GetModelsAsync<TCacheModel>(this ICache cache, IEnumerable<TCacheModel> models, CancellationToken token = default) where TCacheModel : ICacheModel, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TCacheModel>();
            string dimensionKeyName = modelDef.KeyProperty.Name;
            var dimensionKeyValues = models.Select(e => modelDef.KeyProperty.GetValue(e)).ToList();

            return cache.GetModelsAsync<TCacheModel>(dimensionKeyName, dimensionKeyValues, token);
        }

        public static async Task<(TCacheModel?, bool)> GetModelAsync<TCacheModel>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, CancellationToken token = default) where TCacheModel : ICacheModel, new()
        {
            (IEnumerable<TCacheModel>? results, bool exist) = await cache.GetModelsAsync<TCacheModel>(dimensionKeyName, new object[] { dimensionKeyValue }, token).ConfigureAwait(false);

            if (exist)
            {
                return (results!.ElementAt(0), true);
            }

            return (default, false);
        }

        public static Task<(TCacheModel?, bool)> GetModelAsync<TCacheModel>(this ICache cache, TCacheModel model, CancellationToken token = default) where TCacheModel : ICacheModel, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TCacheModel>();

            string dimensionKeyName = modelDef.KeyProperty.Name;
            string dimensionKeyValue = modelDef.KeyProperty.GetValue(model)!.ToString()!;

            return cache.GetModelAsync<TCacheModel>(dimensionKeyName, dimensionKeyValue, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>      
        public static Task RemoveModelsAsync<TCacheModel>(this ICache cache, IEnumerable<TCacheModel> models, CancellationToken token = default) where TCacheModel : ICacheModel, new()
        {
            if (!models.Any())
            {
                return Task.CompletedTask;
            }

            CacheModelDef modelDef = CacheModelDefFactory.Get<TCacheModel>();
            string dimensionKeyName = modelDef.KeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = models.Select(e => modelDef.KeyProperty.GetValue(e)!.ToString()!).ToList();
            IEnumerable<int> updatedVersions = models.Select(e => e.Version).ToList();

            return cache.RemoveModelsAsync<TCacheModel>(dimensionKeyName, dimensionKeyValues, updatedVersions, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        public static Task RemoveModelAsync<TCacheModel>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, int updatedVersion, CancellationToken token = default) where TCacheModel : ICacheModel, new()
        {
            return cache.RemoveModelsAsync<TCacheModel>(dimensionKeyName, new object[] { dimensionKeyValue }, new int[] { updatedVersion }, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        public static Task RemoveModelAsync<TCacheModel>(this ICache cache, TCacheModel model, CancellationToken token = default) where TCacheModel : ICacheModel, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TCacheModel>();

            string dimensionKeyName = modelDef.KeyProperty.Name;
            string dimensionKeyValue = modelDef.KeyProperty.GetValue(model)!.ToString()!;

            return cache.RemoveModelAsync<TCacheModel>(dimensionKeyName, dimensionKeyValue, model.Version, token);
        }

        /// <summary>
        /// 返回是否成功更新。false是数据版本小于缓存中的
        /// </summary>
        public static async Task<bool> SetModelAsync<TCacheModel>(this ICache cache, TCacheModel model, CancellationToken token = default) where TCacheModel : ICacheModel, new()
        {
            IEnumerable<bool> results = await cache.SetModelsAsync<TCacheModel>(new TCacheModel[] { model }, token).ConfigureAwait(false);

            return results.ElementAt(0);
        }
    }
}

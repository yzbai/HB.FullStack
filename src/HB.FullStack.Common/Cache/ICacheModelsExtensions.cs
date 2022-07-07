using HB.FullStack.Common;

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
        public static Task<(IEnumerable<TModel>?, bool)> GetModelsAsync<TModel>(this ICache cache, IEnumerable<TModel> models, CancellationToken token = default) where TModel : Model, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TModel>();
            string dimensionKeyName = modelDef.KeyProperty.Name;
            var dimensionKeyValues = models.Select(e => modelDef.KeyProperty.GetValue(e)).ToList();

            return cache.GetModelsAsync<TModel>(dimensionKeyName, dimensionKeyValues, token);
        }

        public static async Task<(TModel?, bool)> GetModelAsync<TModel>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, CancellationToken token = default) where TModel : Model, new()
        {
            (IEnumerable<TModel>? results, bool exist) = await cache.GetModelsAsync<TModel>(dimensionKeyName, new object[] { dimensionKeyValue }, token).ConfigureAwait(false);

            if (exist)
            {
                return (results!.ElementAt(0), true);
            }

            return (null, false);
        }

        public static Task<(TModel?, bool)> GetModelAsync<TModel>(this ICache cache, TModel model, CancellationToken token = default) where TModel : Model, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TModel>();

            string dimensionKeyName = modelDef.KeyProperty.Name;
            string dimensionKeyValue = modelDef.KeyProperty.GetValue(model)!.ToString()!;

            return cache.GetModelAsync<TModel>(dimensionKeyName, dimensionKeyValue, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>      
        public static Task RemoveModelsAsync<TModel>(this ICache cache, IEnumerable<TModel> models, CancellationToken token = default) where TModel : Model, new()
        {
            if (!models.Any())
            {
                return Task.CompletedTask;
            }

            CacheModelDef modelDef = CacheModelDefFactory.Get<TModel>();
            string dimensionKeyName = modelDef.KeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = models.Select(e => modelDef.KeyProperty.GetValue(e)!.ToString()!).ToList();
            IEnumerable<int> updatedVersions = models.Select(e => e.Version).ToList();

            return cache.RemoveModelsAsync<TModel>(dimensionKeyName, dimensionKeyValues, updatedVersions, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        public static Task RemoveModelAsync<TModel>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, int updatedVersion, CancellationToken token = default) where TModel : Model, new()
        {
            return cache.RemoveModelsAsync<TModel>(dimensionKeyName, new object[] { dimensionKeyValue }, new int[] { updatedVersion }, token);
        }

        /// <summary>
        /// 只能放在数据库Updated之后，因为version需要update之后的version
        /// </summary>
        public static Task RemoveModelAsync<TModel>(this ICache cache, TModel model, CancellationToken token = default) where TModel : Model, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TModel>();

            string dimensionKeyName = modelDef.KeyProperty.Name;
            string dimensionKeyValue = modelDef.KeyProperty.GetValue(model)!.ToString()!;

            return cache.RemoveModelAsync<TModel>(dimensionKeyName, dimensionKeyValue, model.Version, token);
        }

        /// <summary>
        /// 返回是否成功更新。false是数据版本小于缓存中的
        /// </summary>
        public static async Task<bool> SetModelAsync<TModel>(this ICache cache, TModel model, CancellationToken token = default) where TModel : Model, new()
        {
            IEnumerable<bool> results = await cache.SetModelsAsync<TModel>(new TModel[] { model }, token).ConfigureAwait(false);

            return results.ElementAt(0);
        }
    }
}

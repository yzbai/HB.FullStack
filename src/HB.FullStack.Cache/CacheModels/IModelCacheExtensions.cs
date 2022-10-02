using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.Cache
{
    public static class IModelCacheExtensions
    {
        public static async Task<(IEnumerable<TCacheModel>?, bool)> GetModelsAsync<TCacheModel>(this ICache cache, IEnumerable<TCacheModel> models, CancellationToken token = default) where TCacheModel : ITimestampModel, new()
        {
            CacheModelDef? modelDef = CacheModelDefFactory.Get<TCacheModel>();

            if (modelDef == null)
            {
                return (null, false);
            }

            string dimensionKeyName = modelDef.KeyProperty.Name;
            var dimensionKeyValues = models.Select(e => modelDef.KeyProperty.GetValue(e)).ToList();

            return await cache.GetModelsAsync<TCacheModel>(dimensionKeyName, dimensionKeyValues, token).ConfigureAwait(false);
        }

        public static async Task<(TCacheModel?, bool)> GetModelAsync<TCacheModel>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, CancellationToken token = default) where TCacheModel : ITimestampModel, new()
        {
            (IEnumerable<TCacheModel>? results, bool exist) = await cache.GetModelsAsync<TCacheModel>(dimensionKeyName, new object[] { dimensionKeyValue }, token).ConfigureAwait(false);

            if (exist)
            {
                return (results!.ElementAt(0), true);
            }

            return (default, false);
        }

        public static async Task<(TCacheModel?, bool)> GetModelAsync<TCacheModel>(this ICache cache, TCacheModel model, CancellationToken token = default) where TCacheModel : ITimestampModel, new()
        {
            CacheModelDef? modelDef = CacheModelDefFactory.Get<TCacheModel>();

            if (modelDef == null)
            {
                return (default, false);
            }

            string dimensionKeyName = modelDef.KeyProperty.Name;

            //TODO: 应该是类似TypeValueToDbValue那样进行转换
            string dimensionKeyValue = modelDef.KeyProperty.GetValue(model)!.ToString()!;

            return await cache.GetModelAsync<TCacheModel>(dimensionKeyName, dimensionKeyValue, token).ConfigureAwait(false);
        }

        public static async Task<bool> SetModelAsync<TCacheModel>(this ICache cache, TCacheModel model, CancellationToken token = default) where TCacheModel : ITimestampModel, new()
        {
            IEnumerable<bool> results = await cache.SetModelsAsync(new TCacheModel[] { model }, token).ConfigureAwait(false);

            return results.ElementAt(0);
        }

        public static async Task RemoveModelsAsync<T>(this ICache cache, IEnumerable<T> models, CancellationToken token = default)
        {
            if (!models.Any())
            {
                return;
            }

            CacheModelDef? modelDef = CacheModelDefFactory.Get<T>();

            if (modelDef == null)
            {
                return;
            }

            string dimensionKeyName = modelDef.KeyProperty.Name;
            IEnumerable<string> dimensionKeyValues = models.Select(e => modelDef.KeyProperty.GetValue(e)!.ToString()!).ToList();

            await cache.RemoveModelsAsync<T>(dimensionKeyName, dimensionKeyValues, token).ConfigureAwait(false);
        }

        public static Task RemoveModelAsync<T>(this ICache cache, string dimensionKeyName, object dimensionKeyValue, CancellationToken token = default)
        {
            return cache.RemoveModelsAsync<T>(dimensionKeyName, new object[] { dimensionKeyValue }, token);
        }

        public static async Task RemoveModelAsync<TCacheModel>(this ICache cache, TCacheModel model, CancellationToken token = default)
        {
            CacheModelDef? modelDef = CacheModelDefFactory.Get<TCacheModel>();

            if (modelDef == null)
            {
                return;
            }

            string dimensionKeyName = modelDef.KeyProperty.Name;
            string dimensionKeyValue = modelDef.KeyProperty.GetValue(model)!.ToString()!;

            await cache.RemoveModelAsync<TCacheModel>(dimensionKeyName, dimensionKeyValue, token).ConfigureAwait(false);
        }

        public static async Task RemoveModelByIdAsync<TCacheModel>(this ICache cache, object id, CancellationToken token = default)
        {
            ThrowIf.Null(id, nameof(id));

            CacheModelDef? modelDef = CacheModelDefFactory.Get<TCacheModel>();

            if (modelDef == null)
            {
                return;
            }

            string dimensionKeyName = modelDef.KeyProperty.Name;
            string dimensionKeyValue = id!.ToString()!;

            await cache.RemoveModelAsync<TCacheModel>(dimensionKeyName, dimensionKeyValue, token).ConfigureAwait(false);
        }
    }
}

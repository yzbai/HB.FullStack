using System.Threading.Tasks;
using HB.Framework.Common;
using System.Text;
using System;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class IDistributedCacheExtensions
    {
        #region Generic

        public static void Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            ThrowIf.Null(cache, nameof(cache));
            cache.Set(key, JsonUtil.Serialize(value), options);
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            ThrowIf.Null(cache, nameof(cache));
            return cache.SetAsync(key, JsonUtil.Serialize(value), options);
        }

        public static T Get<T>(this IDistributedCache cache, string key)
        {
            ThrowIf.Null(cache, nameof(cache));
            byte[] bytes = cache.Get(key);
            return JsonUtil.DeSerialize<T>(bytes);
        }

        public static Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            ThrowIf.Null(cache, nameof(cache));

            return cache.GetAsync(key).ContinueWith(t => {

                return JsonUtil.DeSerialize<T>(t.Result);
            }, TaskScheduler.Default);
        }

        public static void SetInt(this IDistributedCache cache, string key, int value, DistributedCacheEntryOptions options)
        {
            cache.SetString(key, Convert.ToString(value, GlobalSettings.Culture), options);
        }

        public static int? GetInt(this IDistributedCache cache, string key)
        {
            string value = cache.GetString(key);

            return Convert.ToInt32(value, GlobalSettings.Culture);
        }

        #endregion
        
    }
}

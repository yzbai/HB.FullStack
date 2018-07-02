using System.Threading.Tasks;
using HB.Framework.Common;
using System.Text;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class IDistributedCacheExtensions
    {
        #region Generic

        public static void Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            cache.Set(key, DataConverter.Serialize(value), options);
        }

        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            return cache.SetAsync(key, DataConverter.Serialize(value), options);
        }

        public static T Get<T>(this IDistributedCache cache, string key)
        {
            byte[] bytes = cache.Get(key);
            return DataConverter.DeSerialize<T>(bytes);
        }

        public static Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            return cache.GetAsync(key).ContinueWith(t => { return DataConverter.DeSerialize<T>(t.Result); });
        }

        public static void SetInt(this IDistributedCache cache, string key, int value, DistributedCacheEntryOptions options)
        {
            cache.Set(key, DataConverter.SerializeInt(value), options);
        }

        public static int? GetInt(this IDistributedCache cache, string key)
        {
            byte[] value = cache.Get(key);

            int? result = null;

            if (value != null)
            {
                result = DataConverter.DeserializeInt(value);
            }

            return result;
        }

        #endregion
        
    }
}

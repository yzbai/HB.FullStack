using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Common.Cache
{
    /// <summary>
    /// 这里的timestamp表明数据的LastTime，类似version
    /// </summary>
    public static class ITimestampCacheExtensions
    {

        public static Task<bool> SetIntAsync(this ICache cache, string key, int value, long timestamp, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return cache.SetStringAsync(key, value.ToString(GlobalSettings.Culture), timestamp, options, token);
        }

        public static async Task<int?> GetIntAsync(this ICache cache, string key, CancellationToken token = default)
        {
            try
            {
                string? value = await cache.GetStringAsync(key, token).ConfigureAwait(false);

                if (value == null)
                {
                    return null;
                }



                return System.Convert.ToInt32(value, GlobalSettings.Culture);
            }
            catch (FormatException ex)
            {
                throw CacheExceptions.ConvertError(key: key, innerException: ex);
            }
            catch (OverflowException ex)
            {
                throw CacheExceptions.ConvertError(key: key, innerException: ex);
            }
        }

        public static async Task<bool> SetStringAsync(this ICache cache, string key, string value, long timestamp, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            try
            {
                byte[] bytes = SerializeUtil.Serialize(value);

                return await cache.SetAsync(key, bytes, timestamp, options, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ErrorCodeException)
            {
                throw CacheExceptions.Unkown(key: key, value: value, nameof(SetStringAsync), innerException: ex);
            }
        }

        public static async Task<string?> GetStringAsync(this ICache cache, string key, CancellationToken token = default)
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                return SerializeUtil.Deserialize<string?>(bytes);
            }
            catch (Exception ex) when (ex is not ErrorCodeException)
            {
                throw CacheExceptions.Unkown(key, null, nameof(GetStringAsync), ex);
            }
        }

        /// <summary>
        /// timestamp是ICacheModel.Timestamp
        /// </summary>
        public static async Task<bool> SetAsync<T>(this ICache cache, string key, T value, long timestamp, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            try
            {
                byte[] bytes = SerializeUtil.Serialize(value);

                return await cache.SetAsync(key, bytes, timestamp, options, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ErrorCodeException)
            {
                throw CacheExceptions.Unkown(key, value, nameof(SetAsync), ex);
            }
        }

        public static async Task<T?> GetAsync<T>(this ICache cache, string key, CancellationToken token = default)
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                return SerializeUtil.Deserialize<T>(bytes);
            }
            catch (Exception ex) when (ex is not ErrorCodeException)
            {
                throw CacheExceptions.Unkown(key, null, nameof(GetAsync), ex);
            }
        }
    }
}

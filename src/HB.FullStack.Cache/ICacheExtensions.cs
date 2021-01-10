using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

namespace HB.FullStack.Cache
{
    public static class ICacheExtensions
    {
        /// <summary>
        /// SetIntAsync
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="utcTicks"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public static Task SetIntAsync(this ICache cache, string key, int value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return cache.SetStringAsync(key, value.ToString(CultureInfo.InvariantCulture), utcTicks, options, token);
        }

        /// <exception cref="CacheException"></exception>
        public static async Task<int?> GetIntAsync(this ICache cache, string key, CancellationToken token = default)
        {
            try
            {
                string? value = await cache.GetStringAsync(key, token).ConfigureAwait(false);

                if (value == null)
                {
                    return null;
                }

                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new CacheException(CacheErrorCode.ConvertError, $"Key:{key}", ex);
            }
            catch(OverflowException ex)
            {
                throw new CacheException(CacheErrorCode.ConvertError, $"Key:{key}", ex);
            }
        }

        /// <summary>
        /// SetStringAsync
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="utcTicks"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public static async Task SetStringAsync(this ICache cache, string key, string value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            try
            {
                byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

                await cache.SetAsync(key, bytes, utcTicks, options, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not CacheException)
            {
                throw new CacheException(CacheErrorCode.Unkown, $"Key:{key}, Value:{SerializeUtil.ToJson(value!)}", ex);
            }
        }

        /// <summary>
        /// GetStringAsync
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public static async Task<string?> GetStringAsync(this ICache cache, string key, CancellationToken token = default)
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                return await SerializeUtil.UnPackAsync<string>(bytes).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not CacheException)
            {
                throw new CacheException(CacheErrorCode.Unkown, $"Key:{key}", ex);
            }
        }

        /// <summary>
        /// SetAsync
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="utcTicks"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public static async Task SetAsync<T>(this ICache cache, string key, T value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
        {
            try
            {
                byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

                await cache.SetAsync(key, bytes, utcTicks, options, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not CacheException)
            {
                throw new CacheException(CacheErrorCode.Unkown, $"Key:{key}, Value:{SerializeUtil.ToJson(value!)}", ex);
            }
        }

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public static async Task<T?> GetAsync<T>(this ICache cache, string key, CancellationToken token = default) where T : class
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                return await SerializeUtil.UnPackAsync<T>(bytes).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not CacheException)
            {
                throw new CacheException(CacheErrorCode.Unkown, $"Key:{key}", ex);
            }
        }
    }
}

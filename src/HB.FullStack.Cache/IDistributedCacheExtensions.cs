using HB.FullStack.Common.Entities;

using Microsoft.Extensions.Caching.Distributed;


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.Cache
{
    public static class IDistributedCacheExtensions
    {
        public static async Task SetIntAsync(this IDistributedCache cache, string key, int value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await cache.SetString2Async(key, Convert.ToString(value, CultureInfo.InvariantCulture), options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new CacheException($"Cache SetIntAsync Error. Key:{key}, Value:{value}", ex);
            }
        }

        public static async Task<(int, bool)> GetIntAsync(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken))
        {
            try
            {
                (string? value, bool exists) = await cache.GetString2Async(key, token).ConfigureAwait(false);

                if (!exists)
                {
                    return (0, false);
                }

                return (value!.ToInt32(), true);
            }
            catch (Exception ex)
            {
                throw new CacheException($"Cache GetIntAsync Error. Key:{key}", ex);
            }
        }

        public static async Task SetString2Async(this IDistributedCache cache, string key, string? value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

            await cache.SetAsync(key, bytes, options, token).ConfigureAwait(false);
        }

        public static async Task<(string?, bool)> GetString2Async(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken))
        {
            byte[]? data = await cache.GetAsync(key, token).ConfigureAwait(false);

            if (data == null)
            {
                return (null, false);
            }

            string? result = await SerializeUtil.UnPackAsync<string>(data).ConfigureAwait(false);

            return (result, true);
        }

        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T? value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken)) where T : class
        {
            try
            {
                byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

                await cache.SetAsync(key, bytes, options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new CacheException($"Cache SetAsync Error. Key:{key}, Value:{SerializeUtil.ToJson(value!)}", ex);
            }
        }

        public static async Task<(T?, bool)> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken)) where T : class
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                if (bytes == null)
                {
                    return (null, false);
                }

                T? result = await SerializeUtil.UnPackAsync<T>(bytes).ConfigureAwait(false);
                return (result, true);
            }
            catch (Exception ex)
            {
                throw new CacheException($"Cache GetAsync Error. Key:{key}", ex);
            }
        }


    }
}

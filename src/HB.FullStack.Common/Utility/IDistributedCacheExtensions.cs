using HB.FullStack.Common.Entities;

using Microsoft.Extensions.Caching.Distributed;


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class IDistributedCacheExtensions
    {
        public static async Task SetIntAsync(this IDistributedCache cache, string key, int value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await cache.SetStringAsync(key, Convert.ToString(value, CultureInfo.InvariantCulture), options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FrameworkException($"Cache SetIntAsync Error. Key:{key}, Value:{value}", ex);
            }
        }

        public static async Task<int?> GetIntAsync(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken))
        {
            try
            {
                string? value = await cache.GetStringAsync(key, token).ConfigureAwait(false);

                if (value == null)
                {
                    return null;
                }

                return value!.ToInt32();
            }
            catch (Exception ex)
            {
                throw new FrameworkException($"Cache GetIntAsync Error. Key:{key}", ex);
            }
        }

        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken)) where T : class
        {
            try
            {
                byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

                await cache.SetAsync(key, bytes, options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FrameworkException($"Cache SetAsync Error. Key:{key}, Value:{SerializeUtil.ToJson(value!)}", ex);
            }
        }

        public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default(CancellationToken)) where T : class
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                return await SerializeUtil.UnPackAsync<T>(bytes).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FrameworkException($"Cache GetAsync Error. Key:{key}", ex);
            }
        }


    }
}

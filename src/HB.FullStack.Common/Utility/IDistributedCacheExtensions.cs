﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class IDistributedCacheExtensions
    {
        public static Task SetIntAsync(this IDistributedCache cache, string key, int value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return cache.SetStringAsync(key, Convert.ToString(value, GlobalSettings.Culture), options, token);

        }

        public static async Task<int?> GetIntAsync(this IDistributedCache cache, string key, CancellationToken token = default)
        {

            string? value = await cache.GetStringAsync(key, token).ConfigureAwait(false);

            if (value == null)
            {
                return null;
            }

            return Convert.ToInt32(value, GlobalSettings.Culture);
        }

        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
        {

            byte[] bytes = SerializeUtil.Serialize(value);

            await cache.SetAsync(key, bytes, options, token).ConfigureAwait(false);

        }

        public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default) where T : class
        {

            byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

            return SerializeUtil.Deserialize<T>(bytes);
        }


    }
}

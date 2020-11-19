using HB.Framework.Common.Cache;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.Cache
{
    internal partial class RedisCache
    {
        public async Task SetIntAsync(string key, int value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await SetStringAsync(key, Convert.ToString(value, CultureInfo.InvariantCulture), options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new CacheException($"Cache SetIntAsync Error. Key:{key}, Value:{value}", ex);
            }
        }

        public async Task<(int, bool)> GetIntAsync(string key, CancellationToken token = default(CancellationToken))
        {
            try
            {
                (string? value, bool exists) = await GetStringAsync(key, token).ConfigureAwait(false);

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

        public async Task SetStringAsync(string key, string? value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

            await SetAsync(key, bytes, options, token).ConfigureAwait(false);
        }

        public async Task<(string?, bool)> GetStringAsync(string key, CancellationToken token = default(CancellationToken))
        {
            byte[]? data = await GetAsync(key, token).ConfigureAwait(false);

            if (data == null)
            {
                return (null, false);
            }

            string? result = await SerializeUtil.UnPackAsync<string>(data).ConfigureAwait(false);

            return (result, true);
        }

        public async Task SetAsync<T>(string key, T? value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken)) where T : class
        {
            try
            {
                byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

                await SetAsync(key, bytes, options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new CacheException($"Cache SetAsync Error. Key:{key}, Value:{SerializeUtil.ToJson(value!)}", ex);
            }
        }

        public async Task<(T?, bool)> GetAsync<T>(string key, CancellationToken token = default(CancellationToken)) where T : class
        {
            try
            {
                byte[]? bytes = await GetAsync(key, token).ConfigureAwait(false);

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

        public async Task<bool> IsExistThenRemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            token.ThrowIfCancellationRequested();

            return await database.KeyDeleteAsync(GetRealKey(key)).ConfigureAwait(false);
        }
    }
}

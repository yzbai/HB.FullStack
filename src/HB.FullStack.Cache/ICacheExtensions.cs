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
        public static Task SetIntAsync(this ICache cache, string key, int value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return cache.SetStringAsync(key, value.ToString(GlobalSettings.Culture), utcTicks, options, token);
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

                return Convert.ToInt32(value, GlobalSettings.Culture);
            }
            catch (FormatException ex)
            {
                throw CacheExceptions.ConvertError(key:key, innerException: ex);
            }
            catch(OverflowException ex)
            {
                throw CacheExceptions.ConvertError(key:key, innerException: ex);
            }
        }
        
        public static async Task SetStringAsync(this ICache cache, string key, string value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            try
            {
                byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

                await cache.SetAsync(key, bytes, utcTicks, options, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ErrorCode2Exception)
            {
                throw CacheExceptions.Unkown(key:key, value:value, innerException: ex);
            }
        }
        
        public static async Task<string?> GetStringAsync(this ICache cache, string key, CancellationToken token = default)
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                return await SerializeUtil.UnPackAsync<string>(bytes).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ErrorCode2Exception)
            {
                throw CacheExceptions.Unkown(key, null, ex);
            }
        }
       
        public static async Task SetAsync<T>(this ICache cache, string key, T value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
        {
            try
            {
                byte[] bytes = await SerializeUtil.PackAsync(value).ConfigureAwait(false);

                await cache.SetAsync(key, bytes, utcTicks, options, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ErrorCode2Exception)
            {
                throw CacheExceptions.Unkown(key, value, ex);
            }
        }
        
        public static async Task<T?> GetAsync<T>(this ICache cache, string key, CancellationToken token = default) where T : class
        {
            try
            {
                byte[]? bytes = await cache.GetAsync(key, token).ConfigureAwait(false);

                return await SerializeUtil.UnPackAsync<T>(bytes).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not ErrorCode2Exception)
            {
                throw CacheExceptions.Unkown(key, null, ex);
            }
        }
    }
}

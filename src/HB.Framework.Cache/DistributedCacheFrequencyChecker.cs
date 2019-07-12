using HB.Framework.Common;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Cache
{
    public class DistributedCacheFrequencyChecker : IFrequencyChecker
    {
        private const string _Prefix = "Freq_C";

        private readonly IDistributedCache _cache;

        public DistributedCacheFrequencyChecker(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> CheckAsync(string resourceType, string resource, TimeSpan aliveTimeSpan)
        {
            string key = GetKey(resourceType, resource);

            string value = await _cache.GetStringAsync(key).ConfigureAwait(false);

            if (string.IsNullOrEmpty(value))
            {
                await _cache.SetStringAsync(key, "Hit", new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = aliveTimeSpan }).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public bool Check(string resourceType, string resource, TimeSpan aliveTimeSpan)
        {
            string key = GetKey(resourceType, resource);

            string value = _cache.GetString(key);

            if (string.IsNullOrEmpty(value))
            {
                _cache.SetString(key, "Hit", new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = aliveTimeSpan });
                return true;
            }

            return false;
        }

        public void Reset(string resourceType, string resource)
        {
            string key = GetKey(resourceType, resource);

            _cache.Remove(key);
        }

        public async Task ResetAsync(string resourceType, string resource)
        {
            string key = GetKey(resourceType, resource);

            await _cache.RemoveAsync(key).ConfigureAwait(false);
        }

        private static string GetKey(string resourceType, string resource)
        {
            return $"{_Prefix}:{resourceType}:{resource}";
        }
    }
}

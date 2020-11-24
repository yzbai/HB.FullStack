#nullable enable

using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace System
{
    public class DistributedCacheFrequencyChecker2
    {
        private const string _prefix = "Freq_C";

        private readonly IDistributedCache _cache;

        public DistributedCacheFrequencyChecker2(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> CheckAsync(string resourceType, string resource, TimeSpan aliveTimeSpan)
        {
            string key = GetKey(resourceType, resource);

            string? value = await _cache.GetStringAsync(key).ConfigureAwait(false);

            if (string.IsNullOrEmpty(value))
            {
                await _cache.SetStringAsync(key, "Hit", new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = aliveTimeSpan }).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public Task RemoveAsync(string resourceType, string resource)
        {
            string key = GetKey(resourceType, resource);

            return _cache.RemoveAsync(key);
        }

        private static string GetKey(string resourceType, string resource)
        {
            return $"{_prefix}:{resourceType}:{resource}";
        }
    }
}
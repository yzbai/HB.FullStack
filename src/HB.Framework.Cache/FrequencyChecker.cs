using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Cache
{
    public class FrequencyChecker : IFrequencyChecker
    {
        private const string Frequency_Check_Key_Prefix = "Freq_C";

        private readonly IDistributedCache _cache;

        public FrequencyChecker(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> CheckAsync(string resource, TimeSpan aliveTimeSpan)
        {
            string key = $"{Frequency_Check_Key_Prefix}:{resource}";

            string value = await _cache.GetStringAsync(key).ConfigureAwait(false);

            if (string.IsNullOrEmpty(value))
            {
                await _cache.SetStringAsync(key, "Hit", new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = aliveTimeSpan }).ConfigureAwait(false);
                return true;
            }

            return false;
        }
    }
}

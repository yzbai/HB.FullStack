#nullable enable

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    public class MemoryFrequencyChecker
    {
        private const int _maxItems = 100;

        private static readonly TimeSpan _maxAliveTimeSpan = TimeSpan.FromHours(1);

        /// <summary>
        /// key : timstamp in seconds
        /// </summary>
        private readonly ConcurrentDictionary<string, long> _timestamps = new ConcurrentDictionary<string, long>();

        private int _addedCount;

        /// <summary>
        /// 查询是否过频.true：不过频，通过；false：过频，不通过
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resource"></param>
        /// <param name="aliveTimeSpan"></param>
        /// <returns>true：不过频，通过；false：过频，不通过</returns>
        public bool Check(string resourceType, string resource, TimeSpan aliveTimeSpan)
        {
            string key = GetKey(resourceType, resource);

            long currentTimestampSeconds = TimeUtil.CurrentTimestampSeconds();

            //不存在
            if (!_timestamps.TryGetValue(key, out long storedTimestamp))
            {
                _timestamps[key] = currentTimestampSeconds;

                _addedCount++;

                Clear();

                return true;
            }

            //过期
            if (TimeUtil.CurrentTimestampSeconds() - storedTimestamp > aliveTimeSpan.TotalSeconds)
            {
                _timestamps[key] = currentTimestampSeconds;
                return true;
            }

            //未过期
            return false;
        }

        private void Clear()
        {
            if (_addedCount > _maxItems)
            {
                long current = TimeUtil.CurrentTimestampSeconds();

                _timestamps.Where(kv => current - kv.Value > _maxAliveTimeSpan.TotalSeconds)
                    .Select(kv => kv.Key)
                    .ForEach(key => _timestamps.Remove(key, out _));
            }
        }

        public void Reset(string resourceType, string resource)
        {
            string key = GetKey(resourceType, resource);

            _timestamps.TryRemove(key, out _);
        }

        private static string GetKey(string resourceType, string resource)
        {
            return resourceType + resource;
        }
    }
}
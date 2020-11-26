#nullable enable

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace System
{

    public class MemoryLockManager
    {
        private struct ResourceExpiryInfo
        {
            public long Timestamp;

            public long ExpirySeconds;
        }

        private const int _maxItems = 1000;

        /// <summary>
        /// key : timstamp in seconds
        /// </summary>
        private readonly ConcurrentDictionary<string, ResourceExpiryInfo> _timestamps = new ConcurrentDictionary<string, ResourceExpiryInfo>();

        private int _addedCount;

        private static object _locker = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resource"></param>
        /// <param name="aliveTimeSpan"></param>
        /// <returns>上锁成功,资源占用成功</returns>
        public bool Lock(string resource, long expirySeconds)
        {
            long now = TimeUtil.CurrentUnixTimeSeconds();

            Clear(now);

            //不存在
            if (!_timestamps.TryGetValue(resource, out ResourceExpiryInfo storedInfo))
            {
                lock (_locker)
                {
                    if (!_timestamps.TryGetValue(resource, out storedInfo))
                    {
                        _timestamps[resource] = new ResourceExpiryInfo { Timestamp = now, ExpirySeconds = expirySeconds };

                        _addedCount++;

                        return true;
                    }
                }
            }

            //存在但过期
            if (now - storedInfo.Timestamp >= storedInfo.ExpirySeconds)
            {
                _timestamps[resource] = new ResourceExpiryInfo { Timestamp = now, ExpirySeconds = expirySeconds };
                return true;
            }

            //未过期
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>是否解锁成功</returns>
        public bool UnLock(string resource)
        {
            return _timestamps.TryRemove(resource, out _);
        }

        private void Clear(long now)
        {
            if (_addedCount > _maxItems)
            {
                IEnumerable<string> toRemoveKeys = _timestamps
                    .Where(kv => now - kv.Value.Timestamp >= kv.Value.ExpirySeconds)
                    .Select(kv => kv.Key);

                foreach (string key in toRemoveKeys)
                {
                    _timestamps.TryRemove(key, out _);
                }
            }
        }
    }
}
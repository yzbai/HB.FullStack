#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Extensions.Logging;

namespace HB.FullStack.Common
{
    /// <summary>
    /// 警告：这是NoWaitLock，即两个前程前后或同时抢夺，一个成功，另一个不等待，直接返回，即不保证都能运行。
    /// 注意与Semphro区分开.
    /// 适合做RequestLimiter，即一段时间内请求数量限制
    /// </summary>
    public class MemorySimpleLocker
    {
        private const int _maxItems = 100;

        /// <summary>
        /// resource : time info
        /// </summary>
        private readonly Dictionary<string, ResourceExpiryInfo> _resourceExpiryInfoDict = new Dictionary<string, ResourceExpiryInfo>();

        private int _resourceCount;

        private static readonly object _locker = new object();

        /// <summary>
        /// 警告：这是NoWaitLock，即两个前程前后或同时抢夺，一个成功，另一个不等待，直接返回，即不保证都能运行。
        /// 注意与Semphro区分开
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resource"></param>
        /// <param name="aliveTimeSpan"></param>
        /// <returns>上锁成功,资源占用成功</returns>
        public bool NoWaitLock(string resourceType, string resource, TimeSpan expiryTime)
        {
            string key = GetKey(resourceType, resource);

            lock (_locker)
            {
                long now = TimeUtil.UtcNowUnixTimeMilliseconds;

                Clear(now);

                //不存在 or 存在但过期
                if (!_resourceExpiryInfoDict.TryGetValue(key, out ResourceExpiryInfo? storedInfo) || now - storedInfo.Timestamp >= storedInfo.ExpiryMilliseconds)
                {
                    _resourceExpiryInfoDict[key] = new ResourceExpiryInfo(now, (long)expiryTime.TotalMilliseconds);

                    _resourceCount++;

                    return true;
                }
                else
                {
                    //未过期
                    return false;
                }
            }
        }

        private static string GetKey(string resourceType, string resource)
        {
            return resourceType + resource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>是否解锁成功</returns>
        public bool UnLock(string resourceType, string resource)
        {
            string key = GetKey(resourceType, resource);

            //这里不加锁也没关系，因为不影响上面Lock时的三个判断
            //比如：判断“存在但过期”，这里删掉了。不影响
            return _resourceExpiryInfoDict.Remove(key);
        }

        private void Clear(long now)
        {
            if (_resourceCount > _maxItems)
            {
                IEnumerable<string> toRemoveKeys = _resourceExpiryInfoDict
                    .Where(kv => now - kv.Value.Timestamp >= kv.Value.ExpiryMilliseconds)
                    .Select(kv => kv.Key);

                foreach (string key in toRemoveKeys)
                {
                    _resourceExpiryInfoDict.Remove(key);
                }
            }
        }

        class ResourceExpiryInfo
        {
            public long Timestamp { get; set; }

            public long ExpiryMilliseconds { get; set; }

            public ResourceExpiryInfo(long timestamp, long expiryMilliseconds)
            {
                Timestamp = timestamp;
                ExpiryMilliseconds = expiryMilliseconds;
            }
        }
    }
}
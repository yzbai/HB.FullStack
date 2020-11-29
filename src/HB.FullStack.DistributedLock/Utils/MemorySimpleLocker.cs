#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Lock
{
    public class MemorySimpleLocker
    {
        private const int _maxItems = 1000;

        /// <summary>
        /// resource : time info
        /// </summary>
        private readonly Dictionary<string, MemoryLockResourceExpiryInfo> _resourceExpiryInfoDict = new Dictionary<string, MemoryLockResourceExpiryInfo>();

        private int _resourceCount;

        private static readonly object _locker = new object();

        /// <summary>
        /// 不等待立即返回
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="resource"></param>
        /// <param name="aliveTimeSpan"></param>
        /// <returns>上锁成功,资源占用成功</returns>
        public bool NoWaitLock(string resource, TimeSpan expiryTime)
        {
            lock (_locker)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                Clear(now);

                //不存在 or 存在但过期
                if (!_resourceExpiryInfoDict.TryGetValue(resource, out MemoryLockResourceExpiryInfo storedInfo) || now - storedInfo.Timestamp >= storedInfo.ExpiryMilliseconds)
                {
                    _resourceExpiryInfoDict[resource] = new MemoryLockResourceExpiryInfo(now, (long)expiryTime.TotalMilliseconds, string.Empty);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns>是否解锁成功</returns>
        public bool UnLock(string resource)
        {
            //这里不加锁也没关系，因为不影响上面Lock时的三个判断
            //比如：判断“存在但过期”，这里删掉了。不影响
            return _resourceExpiryInfoDict.Remove(resource);
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
    }
}
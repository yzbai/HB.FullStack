using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Common;
using HB.Framework.Common.Utility;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.DuplicateCheck
{
    /// <summary>
    /// 使用方法：每次都要先Lock，最后Release
    /// </summary>
    public class DuplicateChecker
    {
        private readonly string _instanceName;
        private readonly long _aliveSeconds;
        private readonly IRedisInstanceManager _redisInstanceManager;

        private readonly ConcurrentDictionary<string, string> _tokenDict = new ConcurrentDictionary<string, string>();

        public DuplicateChecker(IRedisInstanceManager redisInstanceManager, string instanceName, long aliveSeconds)
        {
            _redisInstanceManager = redisInstanceManager;
            _instanceName = instanceName;
            _aliveSeconds = aliveSeconds;
        }

        public bool Lock(string setName, string id, out string token)
        {
            token = SecurityUtil.CreateUniqueToken();

            string tokenDictKey = TokenDictKey(setName, id);

            if (_tokenDict.TryAdd(tokenDictKey, token))
            {
                return true;
            }

            return false;
        }

        public void Release(string setName, string id, string token)
        {
            if (CheckToken(setName, id, token))
            {
                _tokenDict.TryRemove(TokenDictKey(setName, id), out string storedValue);
            }
        }

        public bool? IsExist(string setName, string id, string token)
        {
            if (CheckToken(setName, id, token))
            {
                IDatabase database = _redisInstanceManager.GetDatabase(_instanceName);

                if (database.SortedSetRank(setName, id) == null)
                {
                    return false;
                }

                return true;
            }

            return null;
        }

        public void Add(string setName, string id, long timestamp, string token)
        {
            if (CheckToken(setName, id, token))
            {
                IDatabase database = _redisInstanceManager.GetDatabase(_instanceName);

                database.SortedSetAdd(setName, id, timestamp, CommandFlags.None);
            }

            ClearTimeout(setName);
        }

        private void ClearTimeout(string setName)
        {
            long stopTimestamp = TimeUtil.CurrentTimestampSeconds() - _aliveSeconds;

            IDatabase database = _redisInstanceManager.GetDatabase(_instanceName);

            //寻找小于stopTimestamp的，删除他们

            database.SortedSetRemoveRangeByScore(setName, 0, stopTimestamp);

        }

        private static string TokenDictKey(string setName, string id)
        {
            return setName + "_" + id;
        }

        private bool CheckToken(string setName, string id, string token)
        {
            if (_tokenDict.TryGetValue(TokenDictKey(setName, id), out string storedToken))
            {
                if(storedToken.Equals(token, GlobalSettings.Comparison))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

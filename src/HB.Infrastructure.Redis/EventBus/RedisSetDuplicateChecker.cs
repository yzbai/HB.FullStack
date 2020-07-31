using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Common;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.DuplicateCheck
{
    /// <summary>
    /// 使用方法：每次都要先Lock，最后Release
    /// </summary>
    internal class RedisSetDuplicateChecker
    {
        private readonly RedisInstanceSetting _setting;
        private readonly long _aliveSeconds;
        private readonly ILogger _logger;

        private static readonly ConcurrentDictionary<string, string> _tokenDict = new ConcurrentDictionary<string, string>();

        public RedisSetDuplicateChecker(RedisInstanceSetting setting, long aliveSeconds, ILogger logger)
        {
            _setting = setting;
            _aliveSeconds = aliveSeconds;
            _logger = logger;
        }

        #region Lock & Release

        public static bool Lock(string setName, string id, out string token)
        {
            token = SecurityUtil.CreateUniqueToken();

            string tokenDictKey = TokenDictKey(setName, id);

            return _tokenDict.TryAdd(tokenDictKey, token);
        }

        public static void Release(string setName, string id, string token)
        {
            if (CheckToken(setName, id, token))
            {
                _tokenDict.TryRemove(TokenDictKey(setName, id), out _);
            }
        }

        private static bool CheckToken(string setName, string id, string token)
        {
            if (_tokenDict.TryGetValue(TokenDictKey(setName, id), out string storedToken))
            {
                if (storedToken.Equals(token, GlobalSettings.Comparison))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        public async Task<bool> IsExistAsync(string setName, string id, string token)
        {
            if (CheckToken(setName, id, token))
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(_setting, _logger).ConfigureAwait(false);

                if (database.SortedSetRank(setName, id) == null)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public async Task AddAsync(string setName, string id, long timestamp, string token)
        {
            if (CheckToken(setName, id, token))
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(_setting, _logger).ConfigureAwait(false);

                database.SortedSetAdd(setName, id, timestamp, CommandFlags.None);
            }

            await ClearTimeoutAsync(setName).ConfigureAwait(false);
        }

        private async Task ClearTimeoutAsync(string setName)
        {
            long stopTimestamp = TimeUtil.CurrentTimestampSeconds() - _aliveSeconds;

            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(_setting, _logger).ConfigureAwait(false);

            //寻找小于stopTimestamp的，删除他们

            database.SortedSetRemoveRangeByScore(setName, 0, stopTimestamp);

        }

        private static string TokenDictKey(string setName, string id)
        {
            return setName + "_" + id;
        }


    }
}

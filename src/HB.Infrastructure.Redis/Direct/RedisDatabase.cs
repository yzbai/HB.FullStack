using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.Framework.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Direct
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "<Pending>")]
    internal class RedisDatabase : IRedisDatabase
    {
        private readonly ILogger _logger;

        private readonly RedisDatabaseOptions _options;

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;

        public RedisDatabase(IOptions<RedisDatabaseOptions> options, ILogger<RedisDatabase> logger)
        {
            _options = options.Value;
            _logger = logger;

            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);
        }

        #region Key

        public bool KeySetIfNotExist(string redisInstanceName, string key, long expireSeconds)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), _logger);

            return database.StringSet(key, "", TimeSpan.FromSeconds(expireSeconds), When.NotExists);
        }

        #endregion

        #region Hash

        public void HashSetInt(string redisInstanceName, string hashName, IEnumerable<string> fields, IEnumerable<int> values)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), _logger);

            HashEntry[] hashEntries = new HashEntry[fields.Count()];

            for (int i = 0; i < fields.Count(); ++i)
            {
                hashEntries[i] = new HashEntry(fields.ElementAt(i), values.ElementAt(i));
            }
           
            database.HashSet(hashName, hashEntries);
        }

        public IEnumerable<int> HashGetInt(string redisInstanceName, string hashName, IEnumerable<string> fields)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), _logger);

            RedisValue[] values = database.HashGet(hashName, fields.Select<string, RedisValue>(t => t).ToArray());

            return values.Select(t => (int)t);
        }

        #endregion

        #region List

        public T PopAndPush<T>(string redisInstanceName, string fromQueueName, string toQueueName) where T : class
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), _logger);

            byte[] data = database.ListRightPopLeftPush(fromQueueName, toQueueName);

            return SerializeUtil.UnPack<T>(data);
        }

        public async Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data) where T : class
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), _logger);

            return await database.ListLeftPushAsync(queueName, SerializeUtil.Pack<T>(data)).ConfigureAwait(false);
        }

        public ulong QueueLength(string redisInstanceName, string queueName)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), _logger);

            return Convert.ToUInt64(database.ListLength(queueName));
        }

        #endregion

        #region Script

        //TODO: use LoadedLuaScript
        public int ScriptEvaluate(string redisInstanceName, string script, string[] keys, string[] argvs)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), _logger);

            RedisResult result = database.ScriptEvaluate(script, keys.Select<string, RedisKey>(t=>t).ToArray(), argvs.Select<string, RedisValue>(t=>t).ToArray());

            return (int)result;
        }

        #endregion

        private RedisInstanceSetting GetRedisInstanceSetting(string instanceName)
        {
            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return setting;
            }

            return null;
        }
    }
}
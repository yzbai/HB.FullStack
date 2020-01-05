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

        public async Task<bool> KeySetIfNotExistAsync(string redisInstanceName, string key, long expireSeconds)
        {
            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

            return database.StringSet(key, "", TimeSpan.FromSeconds(expireSeconds), When.NotExists);
        }

        #endregion

        #region Hash

        public async Task HashSetIntAsync(string redisInstanceName, string hashName, IEnumerable<string> fields, IEnumerable<int> values)
        {
            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

            HashEntry[] hashEntries = new HashEntry[fields.Count()];

            for (int i = 0; i < fields.Count(); ++i)
            {
                hashEntries[i] = new HashEntry(fields.ElementAt(i), values.ElementAt(i));
            }
           
            database.HashSet(hashName, hashEntries);
        }

        public async Task<IEnumerable<int>> HashGetIntAsync(string redisInstanceName, string hashName, IEnumerable<string> fields)
        {
            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

            RedisValue[] values = database.HashGet(hashName, fields.Select<string, RedisValue>(t => t).ToArray());

            return values.Select(t => (int)t);
        }

        #endregion

        #region List

        public async Task<T> PopAndPushAsync<T>(string redisInstanceName, string fromQueueName, string toQueueName) where T : class
        {
            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

            byte[] data = database.ListRightPopLeftPush(fromQueueName, toQueueName);

            return SerializeUtil.UnPack<T>(data);
        }

        public async Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data) where T : class
        {
            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

            return await database.ListLeftPushAsync(queueName, SerializeUtil.Pack<T>(data)).ConfigureAwait(false);
        }

        public async Task<ulong> QueueLengthAsync(string redisInstanceName, string queueName)
        {
            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

            return Convert.ToUInt64(database.ListLength(queueName));
        }

        #endregion

        #region Script

        //TODO: use LoadedLuaScript
        public async Task<int> ScriptEvaluateAsync(string redisInstanceName, string script, string[] keys, string[] argvs)
        {
            IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

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

            throw new RedisDatabaseException($"No RedisInstanceSetting for instanceName:{instanceName}");
        }
    }
}
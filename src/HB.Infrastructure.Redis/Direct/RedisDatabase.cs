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
        private readonly ILogger logger;

        private readonly RedisDatabaseOptions options;

        private readonly IDictionary<string, RedisInstanceSetting> instanceSettingDict;

        public RedisDatabase(IOptions<RedisDatabaseOptions> options, ILogger<RedisDatabase> logger)
        {
            this.options = options.Value;
            this.logger = logger;

            instanceSettingDict = this.options.ConnectionSettings.ToDictionary(s => s.InstanceName);
        }

        #region Key

        public bool KeySetIfNotExist(string redisInstanceName, string key, long expireSeconds)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), logger);

            return database.StringSet(key, "", TimeSpan.FromSeconds(expireSeconds), When.NotExists);
        }

        #endregion

        #region Hash

        public void HashSetInt(string redisInstanceName, string hashName, IEnumerable<string> fields, IEnumerable<int> values)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), logger);

            HashEntry[] hashEntries = new HashEntry[fields.Count()];

            for (int i = 0; i < fields.Count(); ++i)
            {
                hashEntries[i] = new HashEntry(fields.ElementAt(i), values.ElementAt(i));
            }
           
            database.HashSet(hashName, hashEntries);
        }

        public IEnumerable<int> HashGetInt(string redisInstanceName, string hashName, IEnumerable<string> fields)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), logger);

            RedisValue[] values = database.HashGet(hashName, fields.Select<string, RedisValue>(t => t).ToArray());

            return values.Select(t => (int)t);
        }

        #endregion

        #region List

        public T PopAndPush<T>(string redisInstanceName, string fromQueueName, string toQueueName)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), logger);

            byte[] data = database.ListRightPopLeftPush(fromQueueName, toQueueName);

            return JsonUtil.DeSerialize<T>(data);
            
        }

        public async Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), logger);

            return await database.ListLeftPushAsync(queueName, JsonUtil.ToJson(data)).ConfigureAwait(false);
        }

        public ulong QueueLength(string redisInstanceName, string queueName)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), logger);

            return Convert.ToUInt64(database.ListLength(queueName));
        }

        #endregion

        #region Script

        //TODO: use LoadedLuaScript
        public int ScriptEvaluate(string redisInstanceName, string script, string[] keys, string[] argvs)
        {
            IDatabase database = RedisInstanceManager.GetDatabase(GetRedisInstanceSetting(redisInstanceName), logger);

            RedisResult result = database.ScriptEvaluate(script, keys.Select<string, RedisKey>(t=>t).ToArray(), argvs.Select<string, RedisValue>(t=>t).ToArray());

            return (int)result;
        }

        #endregion

        private RedisInstanceSetting GetRedisInstanceSetting(string instanceName)
        {
            if (instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return setting;
            }

            return null;
        }
    }
}
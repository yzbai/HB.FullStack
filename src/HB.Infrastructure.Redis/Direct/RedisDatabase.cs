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
    internal class RedisDatabase : IRedisDatabase
    {
        private readonly IRedisInstanceManager _redisConnectionManager;

        private readonly ILogger<RedisDatabase> _logger;

        public RedisDatabase(IRedisInstanceManager redisConnectionManager, ILogger<RedisDatabase> logger)
        {
            _redisConnectionManager = redisConnectionManager;
            _logger = logger;
        }

        #region Key

        public bool KeySetIfNotExist(string redisInstanceName, string key, long expireSeconds)
        {
            IDatabase database = _redisConnectionManager.GetDatabase(redisInstanceName);

            return database.StringSet(key, "", TimeSpan.FromSeconds(expireSeconds), When.NotExists);
        }

        #endregion

        #region Hash

        public void HashSetInt(string redisInstanceName, string hashName, IEnumerable<string> fields, IEnumerable<int> values)
        {
            IDatabase database = _redisConnectionManager.GetDatabase(redisInstanceName);

            HashEntry[] hashEntries = new HashEntry[fields.Count()];

            for (int i = 0; i < fields.Count(); ++i)
            {
                hashEntries[i] = new HashEntry(fields.ElementAt(i), values.ElementAt(i));
            }
           
            database.HashSet(hashName, hashEntries);
        }

        public IEnumerable<int> HashGetInt(string redisInstanceName, string hashName, IEnumerable<string> fields)
        {
            IDatabase database = _redisConnectionManager.GetDatabase(redisInstanceName);

            RedisValue[] values = database.HashGet(hashName, fields.Select<string, RedisValue>(t => t).ToArray());

            return values.Select(t => (int)t);
        }

        #endregion

        #region List

        public T PopAndPush<T>(string redisInstanceName, string fromQueueName, string toQueueName)
        {
            IDatabase database = _redisConnectionManager.GetDatabase(redisInstanceName);

            byte[] data = database.ListRightPopLeftPush(fromQueueName, toQueueName);

            return JsonUtil.DeSerialize<T>(data);
            
        }

        public async Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data)
        {
            IDatabase database = _redisConnectionManager.GetDatabase(redisInstanceName);

            return await database.ListLeftPushAsync(queueName, JsonUtil.ToJson(data)).ConfigureAwait(false);
        }

        public ulong QueueLength(string redisInstanceName, string queueName)
        {
            IDatabase database = _redisConnectionManager.GetDatabase(redisInstanceName);

            return Convert.ToUInt64(database.ListLength(queueName));
        }

        #endregion

        #region Script

        //TODO: use LoadedLuaScript
        public int ScriptEvaluate(string redisInstanceName, string script, string[] keys, string[] argvs)
        {
            IDatabase database = _redisConnectionManager.GetDatabase(redisInstanceName);

            RedisResult result = database.ScriptEvaluate(script, keys.Select<string, RedisKey>(t=>t).ToArray(), argvs.Select<string, RedisValue>(t=>t).ToArray());

            return (int)result;
        }

        #endregion
    }
}
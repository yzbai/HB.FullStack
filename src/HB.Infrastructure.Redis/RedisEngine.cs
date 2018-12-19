using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.Framework.Common;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    public class RedisEngine : RedisEngineBase, IRedisEngine
    {
        public RedisEngine(RedisEngineOptions options, ILogger<RedisEngine> logger) : base(options.Value, logger) { }

        #region Key

        public bool KeySetIfNotExist(string redisInstanceName, string id, long expireSeconds)
        {
            IDatabase database = GetDatabase(redisInstanceName);

            return database.StringSet(id, "", TimeSpan.FromSeconds(expireSeconds), When.NotExists);
        }

        #endregion

        #region Hash

        public void HashSetInt(string redisInstanceName, string hashName, IList<string> fields, IList<int> values)
        {
            IDatabase database = GetDatabase(redisInstanceName);

            HashEntry[] hashEntries = new HashEntry[fields.Count];

            for (int i = 0; i < fields.Count; ++i)
            {
                hashEntries[i] = new HashEntry(fields[i], values[i]);
            }

            database.HashSet(hashName, hashEntries);
        }
        
        #endregion

        #region List

        public T PopAndPush<T>(string redisInstanceName, string fromQueueName, string toQueueName)
        {
            IDatabase database = GetDatabase(redisInstanceName);

            byte[] data = database.ListRightPopLeftPush(fromQueueName, toQueueName);

            return DataConverter.DeSerialize<T>(data);

        }

        public async Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data)
        {
            IDatabase database = GetDatabase(redisInstanceName);

            return await database.ListLeftPushAsync(queueName, DataConverter.ToJson(data)).ConfigureAwait(false);
        }

        public ulong QueueLength(string redisInstanceName, string queueName)
        {
            IDatabase database = GetDatabase(redisInstanceName);

            return Convert.ToUInt64(database.ListLength(queueName));
        }

        #endregion

        #region Script

        //TODO: use LoadedLuaScript
        public int ScriptEvaluate(string redisInstanceName, string script, string[] keys, string[] argvs)
        {
            IDatabase database = GetDatabase(redisInstanceName);

            RedisResult result = database.ScriptEvaluate(script, keys.Select<string, RedisKey>(t=>t).ToArray(), argvs.Select<string, RedisValue>(t=>t).ToArray());

            return (int)result;
        }

        #endregion

        private IDatabase GetDatabase(string redisInstanceName)
        {
            return GetDatabase(redisInstanceName, 0, true);
        }
    }
}
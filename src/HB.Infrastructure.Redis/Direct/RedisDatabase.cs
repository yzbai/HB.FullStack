using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// KeySetIfNotExistAsync
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
        public async Task<bool> KeySetIfNotExistAsync(string redisInstanceName, string key, long expireSeconds)
        {
            try
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

                return await database.StringSetAsync(key, "", TimeSpan.FromSeconds(expireSeconds), When.NotExists).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new RedisDatabaseException($"RedisDatabase.KeySetIfNotExistAsync Error. Instance:{redisInstanceName}, key:{key}, expireSeconds:{expireSeconds}", ex);
            }
        }

        #endregion

        #region Hash

        /// <summary>
        /// HashSetIntAsync
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="hashName"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
        public async Task HashSetIntAsync(string redisInstanceName, string hashName, IEnumerable<string> fields, IEnumerable<int> values)
        {
            try
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

                HashEntry[] hashEntries = new HashEntry[fields.Count()];

                for (int i = 0; i < fields.Count(); ++i)
                {
                    hashEntries[i] = new HashEntry(fields.ElementAt(i), values.ElementAt(i));
                }

                await database.HashSetAsync(hashName, hashEntries).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new RedisDatabaseException($"RedisDatabase.HashSetIntAsync Error. Instance:{redisInstanceName}, hashName:{hashName}, fields:{fields.ToJoinedString(",")}", ex);
            }
        }

        /// <summary>
        /// HashGetIntAsync
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="hashName"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
        public async Task<IEnumerable<int>> HashGetIntAsync(string redisInstanceName, string hashName, IEnumerable<string> fields)
        {
            try
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

                RedisValue[] values = await database.HashGetAsync(hashName, fields.Select<string, RedisValue>(t => t).ToArray()).ConfigureAwait(false);

                return values.Select(t => (int)t);
            }
            catch (Exception ex)
            {
                throw new RedisDatabaseException($"RedisDatabase.HashGetIntAsync Error. Instance:{redisInstanceName}, hashName:{hashName}, fields:{fields.ToJoinedString(",")}", ex);
            }
        }

        #endregion

        #region List

        /// <summary>
        /// PopAndPushAsync
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="fromQueueName"></param>
        /// <param name="toQueueName"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
        public async Task<T?> PopAndPushAsync<T>(string redisInstanceName, string fromQueueName, string toQueueName) where T : class
        {
            try
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

                byte[]? data = await database.ListRightPopLeftPushAsync(fromQueueName, toQueueName).ConfigureAwait(false);

                return await SerializeUtil.UnPackAsync<T>(data).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new RedisDatabaseException($"RedisDatabase.PopAndPushAsync Error. Instance:{redisInstanceName}, fromQueueName:{fromQueueName}, toQueueName:{toQueueName}", ex);
            }
        }

        /// <summary>
        /// PushAsync
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="queueName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
        public async Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data) where T : class
        {
            try
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

                return await database.ListLeftPushAsync(queueName, await SerializeUtil.PackAsync(data).ConfigureAwait(false)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new RedisDatabaseException($"RedisDatabase.PushAsync Error. Instance:{redisInstanceName}, queueName:{queueName}, data:{SerializeUtil.ToJson(data)}", ex);
            }
        }

        /// <summary>
        /// QueueLengthAsync
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
        public async Task<ulong> QueueLengthAsync(string redisInstanceName, string queueName)
        {
            try
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

                return Convert.ToUInt64(await database.ListLengthAsync(queueName).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                throw new RedisDatabaseException($"RedisDatabase.QueueLengthAsync Error. Instance:{redisInstanceName}, queueName:{queueName}", ex);
            }
        }

        #endregion

        #region Script

        //TODO: use LoadedLuaScript
        /// <summary>
        /// ScriptEvaluateAsync
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="script"></param>
        /// <param name="keys"></param>
        /// <param name="argvs"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
        public async Task<int> ScriptEvaluateAsync(string redisInstanceName, string script, string[] keys, string[] argvs)
        {
            try
            {
                IDatabase database = await RedisInstanceManager.GetDatabaseAsync(GetRedisInstanceSetting(redisInstanceName), _logger).ConfigureAwait(false);

                RedisResult result = database.ScriptEvaluate(script, keys.Select<string, RedisKey>(t => t).ToArray(), argvs.Select<string, RedisValue>(t => t).ToArray());

                return (int)result;
            }
            catch (Exception ex)
            {
                throw new RedisDatabaseException($"RedisDatabase.ScriptEvaluateAsync Error. Instance:{redisInstanceName}, script:{script}", ex);
            }
        }

        #endregion

        /// <summary>
        /// GetRedisInstanceSetting
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        /// <exception cref="HB.Infrastructure.Redis.Direct.RedisDatabaseException"></exception>
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
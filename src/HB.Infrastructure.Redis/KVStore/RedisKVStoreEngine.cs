using HB.Framework.Common;
using HB.Framework.KVStore.Engine;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using HB.Framework.KVStore;
using System.Text;
using System.Globalization;

namespace HB.Infrastructure.Redis.KVStore
{
    internal partial class RedisKVStoreEngine : IKVStoreEngine
    {
        //private static readonly string luaAddScript = @"if redis.call('HSETNX',KEYS[1], ARGV[1], ARGV[2]) == 1 then redis.call('HSET', KEYS[2], ARGV[1], ARGV[3]) return 1 else return 9 end";
        //private static readonly string luaDeleteScript = @"if redis.call('HGET', KEYS[2], ARGV[1]) ~= ARGV[2] then return 7 else redis.call('HDEL', KEYS[2], ARGV[1]) return redis.call('HDEL', KEYS[1], ARGV[1]) end";
        //private static readonly string luaUpdateScript = @"if redis.call('HGET', KEYS[2], ARGV[1]) ~= ARGV[2] then return 7 else redis.call('HSET', KEYS[2], ARGV[1], ARGV[3]) redis.call('HSET', KEYS[1], ARGV[1], ARGV[4]) return 1 end";


        private const string _luaBatchAddExistCheckTemplate = @"if redis.call('HEXISTS', KEYS[1], ARGV[{0}]) == 1 then return 9 end ";
        private const string _luaBatchAddTemplate = @"redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HSET', KEYS[2], ARGV[{0}], 0) ";
        private const string _luaBatchAddReturnTemplate = @"return 1";

        private const string _luaBatchUpdateVersionCheckTemplate = @"if redis.call('HGET', KEYS[2], ARGV[{0}]) ~= ARGV[{1}] then return 7 end ";
        private const string _luaBatchUpdateTemplate = @"redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HSET', KEYS[2], ARGV[{0}], ARGV[{2}]+1) ";
        private const string _luaBatchUpdateReturnTemplate = @"return 1";

        private const string _luaBatchDeleteVersionCheckTemplate = @"if redis.call('HGET', KEYS[2], ARGV[{0}]) ~= ARGV[{1}] then return 7 end ";
        private const string _luaBatchDeleteTemplate = @"redis.call('HDEL', KEYS[1], ARGV[{0}]) redis.call('HDEL', KEYS[2], ARGV[{0}]) ";
        private const string _luaBatchDeleteReturnTemplate = @"return 1";

        private readonly RedisKVStoreOptions _options;
        private readonly ILogger _logger;

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;

        public KVStoreSettings Settings { get { return _options.KVStoreSettings; } }

        public string FirstDefaultInstanceName { get { return _options.ConnectionSettings[0].InstanceName; } }

        public RedisKVStoreEngine(IOptions<RedisKVStoreOptions> options, ILogger<RedisKVStoreEngine> logger)
        {
            _logger = logger;
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);
        }

        private async Task<IDatabase> GetDatabaseAsync(string instanceName)
        {
            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return await RedisInstanceManager.GetDatabaseAsync(setting, _logger).ConfigureAwait(false);
            }

            throw new KVStoreException($"Can not found Such Redis Instance: {instanceName}");
        }

        private static string EntityVersionName(string entityName)
        {
            return entityName + ":Version";
        }

        private static KVStoreError MapResult(RedisResult redisResult)
        {
            int result = (int)redisResult;

            KVStoreError error = result switch
            {
                9 => KVStoreError.ExistAlready,
                7 => KVStoreError.VersionNotMatched,
                0 => KVStoreError.InnerError,
                1 => KVStoreError.Succeeded,
                _ => KVStoreError.UnKown,
            };

            return error;
        }

        private static string AssembleBatchAddLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchAddExistCheckTemplate, i + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchAddTemplate, i + 1, i + count + 1);
            }

            stringBuilder.Append(_luaBatchAddReturnTemplate);

            return stringBuilder.ToString();
        }

        private static string AssembleBatchUpdateLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchUpdateVersionCheckTemplate, i + 1, i + count + count + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchUpdateTemplate, i + 1, i + count + 1, i + count + count + 1);
            }

            stringBuilder.Append(_luaBatchUpdateReturnTemplate);

            return stringBuilder.ToString();
        }

        private static string AssembleBatchDeleteLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchDeleteVersionCheckTemplate, i + 1, i + count + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchDeleteTemplate, i + 1);
            }

            stringBuilder.Append(_luaBatchDeleteReturnTemplate);

            return stringBuilder.ToString();
        }

        public async Task<string> EntityGetAsync(string storeName, string entityName, string entityKey)
        {
            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

            return await db.HashGetAsync(entityName, entityKey).ConfigureAwait(false);

        }

        public async Task<IEnumerable<string>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys)
        {
            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

            RedisValue[] values = entityKeys.Select(str => (RedisValue)str).ToArray();

            RedisValue[] redisValues = await db.HashGetAsync(entityName, values).ConfigureAwait(false);

            return redisValues.Select<RedisValue, string>(t => t);
        }

        public async Task<IEnumerable<string>> EntityGetAllAsync(string storeName, string entityName)
        {
            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

            HashEntry[] results = await db.HashGetAllAsync(entityName).ConfigureAwait(false);

            return results.Select<HashEntry, string>(t => t.Value);
        }

        public Task EntityAddAsync(string storeName, string entityName, string entityKey, string entityJson)
        {
            return EntityAddAsync(storeName, entityName, new string[] { entityKey }, new List<string> { entityJson });
        }

        public async Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons)
        {
            string luaScript = AssembleBatchAddLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };

            IEnumerable<RedisValue> argvs1 = entityKeys.Select(str => (RedisValue)str);
            IEnumerable<RedisValue> argvs2 = entityJsons.Select(bytes => (RedisValue)bytes);

            RedisValue[] argvs = argvs1.Concat(argvs2).ToArray();

            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

            RedisResult result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);

            KVStoreError error = MapResult(result);

            if (error != KVStoreError.Succeeded)
            {
                throw new KVStoreException(error, entityName, "");
            }
        }

        public Task EntityUpdateAsync(string storeName, string entityName, string entityKey, string entityJson, int entityVersion)
        {
            return EntityUpdateAsync(storeName, entityName, new string[] { entityKey }, new List<string>() { entityJson }, new int[] { entityVersion });
        }

        public async Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchUpdateLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t => (RedisValue)t)
                .Concat(entityJsons.Select(t => (RedisValue)t))
                .Concat(entityVersions.Select(t => (RedisValue)t)).ToArray();

            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

            RedisResult result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);

            KVStoreError error = MapResult(result);

            if (error != KVStoreError.Succeeded)
            {
                throw new KVStoreException(error, entityName, "");
            }
        }

        public Task EntityDeleteAsync(string storeName, string entityName, string entityKey, int entityVersion)
        {
            return EntityDeleteAsync(storeName, entityName, new string[] { entityKey }, new int[] { entityVersion });
        }

        public async Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchDeleteLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t => (RedisValue)t).Concat(entityVersions.Select(t => (RedisValue)t)).ToArray();

            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

            RedisResult result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);

            KVStoreError error = MapResult(result);

            if (error != KVStoreError.Succeeded)
            {
                throw new KVStoreException(error, entityName, "");
            }
        }

        public async Task<bool> EntityDeleteAllAsync(string storeName, string entityName)
        {
            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

            return await db.KeyDeleteAsync(entityName).ConfigureAwait(false);
        }



        public void Close()
        {
            _instanceSettingDict.ForEach(kv =>
            {
                RedisInstanceManager.Close(kv.Value);
            });
        }
    }
}

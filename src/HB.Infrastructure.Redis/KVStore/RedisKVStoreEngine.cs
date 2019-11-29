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

        public RedisKVStoreEngine(IOptions<RedisKVStoreOptions> options, ILogger<RedisKVStoreEngine> logger)
        {
            _logger = logger;
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);
        }

        private IDatabase GetDatabase(string instanceName)
        {
            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return RedisInstanceManager.GetDatabase(setting, _logger);
            }

            return null;
        }

        private static string EntityVersionName(string entityName)
        {
            return entityName + ":Version";
        }

        private static KVStoreResult MapResult(RedisResult redisResult)
        {
            int result = (int)redisResult;

            return result switch
            {
                9 => KVStoreResult.ExistAlready(),
                7 => KVStoreResult.VersionNotMatched(),
                0 => KVStoreResult.Failed(),
                1 => KVStoreResult.Succeeded(),
                _ => KVStoreResult.Failed(),
            };
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

        public string EntityGet(string storeName, string entityName, string entityKey)
        {
            IDatabase db = GetDatabase(storeName);

            return db.HashGet(entityName, entityKey);
        }

        public IEnumerable<string> EntityGet(string storeName, string entityName, IEnumerable<string> entityKeys)
        {
            IDatabase db = GetDatabase(storeName);

            RedisValue[] result = db.HashGet(entityName, entityKeys.Select(str => (RedisValue)str).ToArray());

            return result.Select(rs => rs.ToString());
        }

        public IEnumerable<string> EntityGetAll(string storeName, string entityName)
        {
            IDatabase db = GetDatabase(storeName);

            return db.HashGetAll(entityName).Select<HashEntry, string>(t => t.Value);
        }

        public KVStoreResult EntityAdd(string storeName, string entityName, string entityKey, string entityJson)
        {
            return EntityAdd(storeName, entityName, new string[] { entityKey }, new List<string> { entityJson });
        }

        public KVStoreResult EntityAdd(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons)
        {
            string luaScript = AssembleBatchAddLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };

            RedisValue[] argvs = entityKeys.Select(t => (RedisValue)t).Concat(entityJsons.Select(t => (RedisValue)t)).ToArray();

            IDatabase db = GetDatabase(storeName);

            RedisResult result = db.ScriptEvaluate(luaScript, keys, argvs);

            return MapResult(result);
        }

        public KVStoreResult EntityUpdate(string storeName, string entityName, string entityKey, string entityJson, int entityVersion)
        {
            return EntityUpdate(storeName, entityName, new string[] { entityKey }, new List<string>() { entityJson }, new int[] { entityVersion });
        }

        public KVStoreResult EntityUpdate(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchUpdateLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t => (RedisValue)t)
                .Concat(entityJsons.Select(t => (RedisValue)t))
                .Concat(entityVersions.Select(t => (RedisValue)t)).ToArray();

            IDatabase db = GetDatabase(storeName);

            RedisResult result = db.ScriptEvaluate(luaScript, keys, argvs);

            return MapResult(result);
        }

        public KVStoreResult EntityDelete(string storeName, string entityName, string entityKey, int entityVersion)
        {
            return EntityDelete(storeName, entityName, new string[] { entityKey }, new int[] { entityVersion });
        }

        public KVStoreResult EntityDelete(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchDeleteLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t => (RedisValue)t).Concat(entityVersions.Select(t => (RedisValue)t)).ToArray();

            IDatabase db = GetDatabase(storeName);

            RedisResult result = db.ScriptEvaluate(luaScript, keys, argvs);

            return MapResult(result);
        }

        public KVStoreResult EntityDeleteAll(string storeName, string entityName)
        {
            IDatabase db = GetDatabase(storeName);

            bool result = db.KeyDelete(entityName);

            return result ? KVStoreResult.Succeeded() : KVStoreResult.Failed();
        }

        public KVStoreSettings Settings {
            get {
                return _options.KVStoreSettings;
            }
        }

        public string FirstDefaultInstanceName {
            get {
                return _options.ConnectionSettings[0].InstanceName;
            }
        }

        public void Close()
        {
            _instanceSettingDict.ForEach(kv => {
                RedisInstanceManager.Close(kv.Value);
            });
        }
    }
}

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
using Microsoft.AspNetCore.Hosting;
using System.Globalization;

namespace HB.Infrastructure.Redis.KVStore
{
    public partial class RedisKVStoreEngine : IKVStoreEngine
    {
        //private static readonly string luaAddScript = @"if redis.call('HSETNX',KEYS[1], ARGV[1], ARGV[2]) == 1 then redis.call('HSET', KEYS[2], ARGV[1], ARGV[3]) return 1 else return 9 end";
        //private static readonly string luaDeleteScript = @"if redis.call('HGET', KEYS[2], ARGV[1]) ~= ARGV[2] then return 7 else redis.call('HDEL', KEYS[2], ARGV[1]) return redis.call('HDEL', KEYS[1], ARGV[1]) end";
        //private static readonly string luaUpdateScript = @"if redis.call('HGET', KEYS[2], ARGV[1]) ~= ARGV[2] then return 7 else redis.call('HSET', KEYS[2], ARGV[1], ARGV[3]) redis.call('HSET', KEYS[1], ARGV[1], ARGV[4]) return 1 end";


        private const string luaBatchAddExistCheckTemplate = @"if redis.call('HEXISTS', KEYS[1], ARGV[{0}]) == 1 then return 9 end ";
        private const string luaBatchAddTemplate = @"redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HSET', KEYS[2], ARGV[{0}], 0) ";
        private const string luaBatchAddReturnTemplate = @"return 1";

        private const string luaBatchUpdateVersionCheckTemplate = @"if redis.call('HGET', KEYS[2], ARGV[{0}]) ~= ARGV[{1}] then return 7 end ";
        private const string luaBatchUpdateTemplate = @"redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HSET', KEYS[2], ARGV[{0}], ARGV[{2}]+1) ";
        private const string luaBatchUpdateReturnTemplate = @"return 1";

        private const string luaBatchDeleteVersionCheckTemplate = @"if redis.call('HGET', KEYS[2], ARGV[{0}]) ~= ARGV[{1}] then return 7 end ";
        private const string luaBatchDeleteTemplate = @"redis.call('HDEL', KEYS[1], ARGV[{0}]) redis.call('HDEL', KEYS[2], ARGV[{0}]) ";
        private const string luaBatchDeleteReturnTemplate = @"return 1";

        private ILogger _logger;
        private IRedisConnectionManager _redisConnectionManager;

        public RedisKVStoreEngine(IRedisConnectionManager redisConnectionManager, ILogger<RedisKVStoreEngine> logger)
        {
            _logger = logger;
            _redisConnectionManager = redisConnectionManager;
        }
        

        private static string EntityVersionName(string entityName)
        {
            return entityName + ":Version";
        }

        private static KVStoreResult MapResult(RedisResult redisResult)
        {
            int result = (int)redisResult;

            switch(result)
            {
                case 9: return KVStoreResult.ExistAlready();
                case 7: return KVStoreResult.VersionNotMatched();
                case 0: return KVStoreResult.Failed();
                case 1: return KVStoreResult.Succeeded();
                default: return KVStoreResult.Failed();
            }
        }

        private static string AssembleBatchAddLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, luaBatchAddExistCheckTemplate, i + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, luaBatchAddTemplate, i + 1, i + count + 1);
            }

            stringBuilder.Append(luaBatchAddReturnTemplate);

            return stringBuilder.ToString();
        }

        private static string AssembleBatchUpdateLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, luaBatchUpdateVersionCheckTemplate, i + 1, i + count + count + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, luaBatchUpdateTemplate, i + 1, i + count + 1, i + count + count + 1);
            }

            stringBuilder.Append(luaBatchUpdateReturnTemplate);

            return stringBuilder.ToString();
        }

        private static string AssembleBatchDeleteLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, luaBatchDeleteVersionCheckTemplate, i + 1, i + count + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, luaBatchDeleteTemplate, i + 1);
            }

            stringBuilder.Append(luaBatchDeleteReturnTemplate);

            return stringBuilder.ToString();
        }

        public string EntityGet(string storeName, int storeIndex, string entityName, string entityKey)
        {
            IDatabase db = _redisConnectionManager.GetReadDatabase(storeName, storeIndex);

            return db.HashGet(entityName, entityKey);
        }

        public IEnumerable<string> EntityGet(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys)
        {
            IDatabase db = _redisConnectionManager.GetReadDatabase(storeName, storeIndex);

            RedisValue[] result = db.HashGet(entityName, entityKeys.Select(str=>(RedisValue)str).ToArray());

            return result.Select(rs => rs.ToString());
        }

        public IEnumerable<string> EntityGetAll(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = _redisConnectionManager.GetReadDatabase(storeName, storeIndex);

            return db.HashGetAll(entityName).Select<HashEntry, string>(t => t.Value);
        }

        public KVStoreResult EntityAdd(string storeName, int storeIndex, string entityName, string entityKey, string entityJson)
        {
            return EntityAdd(storeName, storeIndex, entityName, new string[] { entityKey }, new List<string> { entityJson });
        }

        public KVStoreResult EntityAdd(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons)
        {
            string luaScript = AssembleBatchAddLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };


            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t).Concat(entityJsons.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            RedisResult result = db.ScriptEvaluate(luaScript, keys, argvs);

            return MapResult(result);

        }

        public KVStoreResult EntityUpdate(string storeName, int storeIndex, string entityName, string entityKey, string entityJson, int entityVersion)
        {
            return EntityUpdate(storeName, storeIndex, entityName, new string[] { entityKey }, new List<string>() { entityJson }, new int[] { entityVersion });
        }

        public KVStoreResult EntityUpdate(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchUpdateLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t)
                .Concat(entityJsons.Select(t=>(RedisValue)t))
                .Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            RedisResult result = db.ScriptEvaluate(luaScript, keys, argvs);

            return MapResult(result);
        }

        public KVStoreResult EntityDelete(string storeName, int storeIndex, string entityName, string entityKey, int entityVersion)
        {
            return EntityDelete(storeName, storeIndex, entityName, new string[] { entityKey }, new int[] { entityVersion });
        }

        public KVStoreResult EntityDelete(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchDeleteLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t).Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            RedisResult result = db.ScriptEvaluate(luaScript, keys, argvs);

            return MapResult(result);
        }

        public KVStoreResult EntityDeleteAll(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            bool result = db.KeyDelete(entityName);

            return result ? KVStoreResult.Succeeded() : KVStoreResult.Failed();
        }

        
    }
}

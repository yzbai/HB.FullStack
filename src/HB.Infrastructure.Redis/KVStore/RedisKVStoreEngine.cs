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

namespace HB.Infrastructure.Redis.KVStore
{
    public partial class RedisKVStoreEngine : RedisEngineBase, IKVStoreEngine
    {
        //private static readonly string luaAddScript = @"if redis.call('HSETNX',KEYS[1], ARGV[1], ARGV[2]) == 1 then redis.call('HSET', KEYS[2], ARGV[1], ARGV[3]) return 1 else return 9 end";
        //private static readonly string luaDeleteScript = @"if redis.call('HGET', KEYS[2], ARGV[1]) ~= ARGV[2] then return 7 else redis.call('HDEL', KEYS[2], ARGV[1]) return redis.call('HDEL', KEYS[1], ARGV[1]) end";
        //private static readonly string luaUpdateScript = @"if redis.call('HGET', KEYS[2], ARGV[1]) ~= ARGV[2] then return 7 else redis.call('HSET', KEYS[2], ARGV[1], ARGV[3]) redis.call('HSET', KEYS[1], ARGV[1], ARGV[4]) return 1 end";

        private static readonly string luaBatchAddExistCheckTemplate = @"if redis.call('HEXISTS', KEYS[1], ARGV[{0}]) == 1 then return 9 end ";
        private static readonly string luaBatchAddTemplate = @"redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HSET', KEYS[2], ARGV[{0}], 0) ";
        private static readonly string luaBatchAddReturnTemplate = @"return 1";

        private static readonly string luaBatchUpdateVersionCheckTemplate = @"if redis.call('HGET', KEYS[2], ARGV[{0}]) ~= ARGV[{1}] then return 7 end ";
        private static readonly string luaBatchUpdateTemplate = @"redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HSET', KEYS[2], ARGV[{0}], ARGV[{2}]+1) ";
        private static readonly string luaBatchUpdateReturnTemplate = @"return 1";

        private static readonly string luaBatchDeleteVersionCheckTemplate = @"if redis.call('HGET', KEYS[2], ARGV[{0}]) ~= ARGV[{1}] then return 7 end ";
        private static readonly string luaBatchDeleteTemplate = @"redis.call('HDEL', KEYS[1], ARGV[{0}]) redis.call('HDEL', KEYS[2], ARGV[{0}]) ";
        private static readonly string luaBatchDeleteReturnTemplate = @"return 1";

        public RedisKVStoreEngine(IApplicationLifetime applicationLifetime, IOptions<RedisEngineOptions> options, ILogger<RedisKVStoreEngine> logger) : base(applicationLifetime, options.Value, logger) { }

        private string entityVersionName(string entityName)
        {
            return entityName + ":Version";
        }

        private static KVStoreResult mapResult(RedisResult redisResult)
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

        private static string assembleBatchAddLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(luaBatchAddExistCheckTemplate, i + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(luaBatchAddTemplate, i + 1, i + count + 1);
            }

            stringBuilder.Append(luaBatchAddReturnTemplate);

            return stringBuilder.ToString();
        }

        private static string assembleBatchUpdateLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(luaBatchUpdateVersionCheckTemplate, i + 1, i + count + count + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(luaBatchUpdateTemplate, i + 1, i + count + 1, i + count + count + 1);
            }

            stringBuilder.Append(luaBatchUpdateReturnTemplate);

            return stringBuilder.ToString();
        }

        private static string assembleBatchDeleteLuaScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(luaBatchDeleteVersionCheckTemplate, i + 1, i + count + 1);
            }

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(luaBatchDeleteTemplate, i + 1);
            }

            stringBuilder.Append(luaBatchDeleteReturnTemplate);

            return stringBuilder.ToString();
        }

        public byte[] EntityGet(string storeName, int storeIndex, string entityName, string entityKey)
        {
            IDatabase db = GetReadDatabase(storeName, storeIndex);

            return db.HashGet(entityName, entityKey);
        }

        public IEnumerable<byte[]> EntityGet(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys)
        {
            IDatabase db = GetReadDatabase(storeName, storeIndex);

            RedisValue[] result = db.HashGet(entityName, entityKeys.Select(str=>(RedisValue)str).ToArray());

            return result.Select(rs => (byte[])rs);
        }

        public IEnumerable<byte[]> EntityGetAll(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = GetReadDatabase(storeName, storeIndex);

            return db.HashGetAll(entityName).Select<HashEntry, byte[]>(t => t.Value);
        }

        public KVStoreResult EntityAdd(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue)
        {
            return EntityAdd(storeName, storeIndex, entityName, new string[] { entityKey }, new List<byte[]> { entityValue });
        }

        public KVStoreResult EntityAdd(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues)
        {
            string luaScript = assembleBatchAddLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, entityVersionName(entityName) };


            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t).Concat(entityValues.Select(bytes=>(RedisValue)bytes)).ToArray();

            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            RedisResult result = db.ScriptEvaluate(luaScript.ToString(), keys, argvs);

            return mapResult(result);

        }

        public KVStoreResult EntityUpdate(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue, int entityVersion)
        {
            return EntityUpdate(storeName, storeIndex, entityName, new string[] { entityKey }, new List<byte[]>() { entityValue }, new int[] { entityVersion });
        }

        public KVStoreResult EntityUpdate(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues, IEnumerable<int> entityVersions)
        {
            string luaScript = assembleBatchUpdateLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, entityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t)
                .Concat(entityValues.Select(t=>(RedisValue)t))
                .Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            RedisResult result = db.ScriptEvaluate(luaScript.ToString(), keys, argvs);

            return mapResult(result);
        }

        public KVStoreResult EntityDelete(string storeName, int storeIndex, string entityName, string entityKey, int entityVersion)
        {
            return EntityDelete(storeName, storeIndex, entityName, new string[] { entityKey }, new int[] { entityVersion });
        }

        public KVStoreResult EntityDelete(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions)
        {
            string luaScript = assembleBatchDeleteLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, entityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t).Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            RedisResult result = db.ScriptEvaluate(luaScript.ToString(), keys, argvs);

            return mapResult(result);
        }

        public KVStoreResult EntityDeleteAll(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            bool result = db.KeyDelete(entityName);

            return result ? KVStoreResult.Succeeded() : KVStoreResult.Failed();
        }
    }
}

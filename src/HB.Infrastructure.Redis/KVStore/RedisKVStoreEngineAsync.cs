using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.KVStore;
using HB.Framework.KVStore.Engine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.KVStore
{
    public partial class RedisKVStoreEngine : IKVStoreEngineAsync
    {
        public async Task<string> EntityGetAsync(string storeName, int storeIndex, string entityName, string entityKey)
        {
            IDatabase db = _redisConnectionManager.GetReadDatabase(storeName, storeIndex);

            return await db.HashGetAsync(entityName, entityKey).ConfigureAwait(false);

        }

        public async Task<IEnumerable<string>> EntityGetAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys)
        {
            IDatabase db = _redisConnectionManager.GetReadDatabase(storeName, storeIndex);

            RedisValue[] values = entityKeys.Select(str => (RedisValue)str).ToArray();

            RedisValue[] redisValues = await db.HashGetAsync(entityName, values).ConfigureAwait(false);

            return redisValues.Select<RedisValue, string>(t => t);
        }

        public async Task<IEnumerable<string>> EntityGetAllAsync(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = _redisConnectionManager.GetReadDatabase(storeName, storeIndex);

            HashEntry[] results = await db.HashGetAllAsync(entityName).ConfigureAwait(false);

            return results.Select<HashEntry, string>(t => t.Value);
        }

        public Task<KVStoreResult> EntityAddAsync(string storeName, int storeIndex, string entityName, string entityKey, string entityJson)
        {
            return EntityAddAsync(storeName, storeIndex, entityName, new string[] { entityKey }, new List<string> { entityJson });
        }

        public Task<KVStoreResult> EntityAddAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons)
        {
            string luaScript = AssembleBatchAddLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };

            IEnumerable<RedisValue> argvs1 = entityKeys.Select(str => (RedisValue)str);
            IEnumerable<RedisValue> argvs2 = entityJsons.Select(bytes => (RedisValue)bytes);

            RedisValue[] argvs = argvs1.Concat(argvs2).ToArray();

            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            return db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ContinueWith(t=>MapResult(t.Result), TaskScheduler.Default);
        }

        public Task<KVStoreResult> EntityUpdateAsync(string storeName, int storeIndex, string entityName, string entityKey, string entityJson, int entityVersion)
        {
            return EntityUpdateAsync(storeName, storeIndex, entityName, new string[] { entityKey }, new List<string>() { entityJson }, new int[] { entityVersion });
        }

        public Task<KVStoreResult> EntityUpdateAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchUpdateLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t)
                .Concat(entityJsons.Select(t=>(RedisValue)t))
                .Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            return db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ContinueWith(t => MapResult(t.Result), TaskScheduler.Default);
        }

        public Task<KVStoreResult> EntityDeleteAsync(string storeName, int storeIndex, string entityName, string entityKey, int entityVersion)
        {
            return EntityDeleteAsync(storeName, storeIndex, entityName, new string[] { entityKey }, new int[] { entityVersion });
        }

        public Task<KVStoreResult> EntityDeleteAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchDeleteLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t).Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            return db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ContinueWith(t => MapResult(t.Result), TaskScheduler.Default);
        }

        public Task<KVStoreResult> EntityDeleteAllAsync(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = _redisConnectionManager.GetWriteDatabase(storeName, storeIndex);

            return db.KeyDeleteAsync(entityName).ContinueWith(t=>t.Result? KVStoreResult.Succeeded() : KVStoreResult.Failed(), TaskScheduler.Default);
        }
    }
}

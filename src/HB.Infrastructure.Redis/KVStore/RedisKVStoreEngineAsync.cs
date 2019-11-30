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
    internal partial class RedisKVStoreEngine : IKVStoreEngineAsync
    {
        public async Task<string> EntityGetAsync(string storeName, string entityName, string entityKey)
        {
            IDatabase db = GetDatabase(storeName);

            return await db.HashGetAsync(entityName, entityKey).ConfigureAwait(false);

        }

        public async Task<IEnumerable<string>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys)
        {
            IDatabase db = GetDatabase(storeName);

            RedisValue[] values = entityKeys.Select(str => (RedisValue)str).ToArray();

            RedisValue[] redisValues = await db.HashGetAsync(entityName, values).ConfigureAwait(false);

            return redisValues.Select<RedisValue, string>(t => t);
        }

        public async Task<IEnumerable<string>> EntityGetAllAsync(string storeName, string entityName)
        {
            IDatabase db = GetDatabase(storeName);

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

            IDatabase db = GetDatabase(storeName);

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
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t)
                .Concat(entityJsons.Select(t=>(RedisValue)t))
                .Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = GetDatabase(storeName);

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
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t).Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = GetDatabase(storeName);

            RedisResult result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);

            KVStoreError error = MapResult(result);

            if (error != KVStoreError.Succeeded)
            {
                throw new KVStoreException(error, entityName, "");
            }
        }

        public Task<bool> EntityDeleteAllAsync(string storeName, string entityName)
        {
            IDatabase db = GetDatabase(storeName);

            return db.KeyDeleteAsync(entityName);
        }
    }
}

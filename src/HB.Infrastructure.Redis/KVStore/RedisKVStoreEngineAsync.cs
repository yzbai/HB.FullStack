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
        public Task<byte[]> EntityGetAsync(string storeName, int storeIndex, string entityName, string entityKey)
        {
            IDatabase db = GetReadDatabase(storeName, storeIndex);

            return db.HashGetAsync(entityName, entityKey)
                .ContinueWith<byte[]>(t=>t.Result, TaskScheduler.Default);
        }

        public Task<IEnumerable<byte[]>> EntityGetAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys)
        {
            IDatabase db = GetReadDatabase(storeName, storeIndex);

            RedisValue[] values = entityKeys.Select(str => (RedisValue)str).ToArray();

            return db.HashGetAsync(entityName, values).ContinueWith(t=>t.Result.Select(rv=>(byte[])rv), TaskScheduler.Default);
        }

        public Task<IEnumerable<byte[]>> EntityGetAllAsync(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = GetReadDatabase(storeName, storeIndex);

            return db.HashGetAllAsync(entityName).ContinueWith(task=>task.Result.Select<HashEntry, byte[]>(t => t.Value), TaskScheduler.Default);
        }

        public Task<KVStoreResult> EntityAddAsync(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue)
        {
            return EntityAddAsync(storeName, storeIndex, entityName, new string[] { entityKey }, new List<byte[]> { entityValue });
        }

        public Task<KVStoreResult> EntityAddAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues)
        {
            string luaScript = AssembleBatchAddLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };

            IEnumerable<RedisValue> argvs1 = entityKeys.Select<string, RedisValue>(str => (RedisValue)str);
            IEnumerable<RedisValue> argvs2 = entityValues.Select<byte[], RedisValue>(bytes => (RedisValue)bytes);

            RedisValue[] argvs = argvs1.Concat(argvs2).ToArray();

            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            return db.ScriptEvaluateAsync(luaScript.ToString(_culture), keys, argvs).ContinueWith(t=>MapResult(t.Result), TaskScheduler.Default);
        }

        public Task<KVStoreResult> EntityUpdateAsync(string storeName, int storeIndex, string entityName, string entityKey, byte[] entityValue, int entityVersion)
        {
            return EntityUpdateAsync(storeName, storeIndex, entityName, new string[] { entityKey }, new List<byte[]>() { entityValue }, new int[] { entityVersion });
        }

        public Task<KVStoreResult> EntityUpdateAsync(string storeName, int storeIndex, string entityName, IEnumerable<string> entityKeys, IEnumerable<byte[]> entityValues, IEnumerable<int> entityVersions)
        {
            string luaScript = AssembleBatchUpdateLuaScript(entityKeys.Count());

            RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
            RedisValue[] argvs = entityKeys.Select(t=>(RedisValue)t)
                .Concat(entityValues.Select(t=>(RedisValue)t))
                .Concat(entityVersions.Select(t=>(RedisValue)t)).ToArray();

            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            return db.ScriptEvaluateAsync(luaScript.ToString(_culture), keys, argvs).ContinueWith(t => MapResult(t.Result), TaskScheduler.Default);
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

            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            return db.ScriptEvaluateAsync(luaScript.ToString(_culture), keys, argvs).ContinueWith(t => MapResult(t.Result), TaskScheduler.Default);
        }

        public Task<KVStoreResult> EntityDeleteAllAsync(string storeName, int storeIndex, string entityName)
        {
            IDatabase db = GetWriteDatabase(storeName, storeIndex);

            return db.KeyDeleteAsync(entityName).ContinueWith(t=>t.Result? KVStoreResult.Succeeded() : KVStoreResult.Failed(), TaskScheduler.Default);
        }
    }
}

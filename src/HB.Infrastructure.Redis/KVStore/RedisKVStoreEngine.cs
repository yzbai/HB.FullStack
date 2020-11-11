﻿using HB.Framework.KVStore;
using HB.Framework.KVStore.Engine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private const string _luaBatchUpdateTemplate = @"redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HINCRBY', KEYS[2], ARGV[{0}], 1) ";
        private const string _luaBatchUpdateReturnTemplate = @"return 1";

        private const string _luaBatchDeleteVersionCheckTemplate = @"if redis.call('HGET', KEYS[2], ARGV[{0}]) ~= ARGV[{1}] then return 7 end ";
        private const string _luaBatchDeleteTemplate = @"redis.call('HDEL', KEYS[1], ARGV[{0}]) redis.call('HDEL', KEYS[2], ARGV[{0}]) ";
        private const string _luaBatchDeleteReturnTemplate = @"return 1";

        private const string _luaBatchAddOrUpdateTemplate = @"if redis.call('HEXISTS', KEYS[1], ARGV[{0}]) == 1 then redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) local version= redis.call('HINCRBY', KEYS[2], ARGV[{0}],1) redis.call('RPUSH', KEYS[3], version) else redis.call('HSET', KEYS[1], ARGV[{0}], ARGV[{1}]) redis.call('HSET', KEYS[2], ARGV[{0}], 0) redis.call('RPUSH', KEYS[3], 0) end ";
        private const string _luaBatchAddOrUpdateReturnTemplate = @" return redis.call('LRANGE', KEYS[3], 0, -1)  ";

        private const string _luaBatchGetTemplate = @" local array = {{}} array[1] = redis.call('HMGET', KEYS[1], {0}) array[2] = redis.call('HMGET', KEYS[2], {0}) return array";

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

        public async Task<IEnumerable<Tuple<string?, int>>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys)
        {
            if (!entityKeys.Any())
            {
                return new List<Tuple<string?, int>>();
            }

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                StringBuilder stringBuilder = new StringBuilder();

                foreach (string key in entityKeys)
                {
                    stringBuilder.Append($"'{key}',");
                }

                stringBuilder.Remove(stringBuilder.Length - 1, 1);

                string lua = string.Format(GlobalSettings.Culture, _luaBatchGetTemplate, stringBuilder.ToString());

                RedisResult result = await db.ScriptEvaluateAsync(lua, new RedisKey[] { entityName, EntityVersionName(entityName) }).ConfigureAwait(false);

                RedisResult[] results = (RedisResult[])result;

                string[] values = (string[])results[0];
                int[] version = (int[])results[1];

                List<Tuple<string?, int>> rt = new List<Tuple<string?, int>>();

                for (int i = 0; i < values.Length; ++i)
                {
                    rt.Add(new Tuple<string?, int>(values[i], version[i]));
                }

                return rt;

            }
            catch (RedisConnectionException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisConnectionFailed, entityName, "", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisTimeout, entityName, "", ex);
            }
            catch (Exception ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreError, entityName, "", ex);
            }
        }


        public async Task<IEnumerable<string>> EntityGetAllAsync(string storeName, string entityName)
        {
            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                HashEntry[] results = await db.HashGetAllAsync(entityName).ConfigureAwait(false);

                return results.Select<HashEntry, string>(t => t.Value);
            }
            catch (RedisConnectionException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisConnectionFailed, entityName, "", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisTimeout, entityName, "", ex);
            }
            catch (Exception ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreError, entityName, "", ex);
            }
        }

        public async Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons)
        {
            RedisResult result = null!;

            try
            {
                string luaScript = AssembleBatchAddLuaScript(entityKeys.Count());

                RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };

                IEnumerable<RedisValue> argvs1 = entityKeys.Select(str => (RedisValue)str);
                IEnumerable<RedisValue> argvs2 = entityJsons.Select(bytes => (RedisValue)bytes);

                RedisValue[] argvs = argvs1.Concat(argvs2).ToArray();

                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);

            }
            catch (RedisConnectionException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisConnectionFailed, entityName, "", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisTimeout, entityName, "", ex);
            }
            catch (Exception ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreError, entityName, "", ex);
            }

            ErrorCode error = MapResult(result);

            if (!error.IsSuccessful())
            {
                throw new KVStoreException(error, entityName, "");
            }
        }

        /// <summary>
        /// 返回最新的Version
        /// </summary>
        public async Task<IEnumerable<int>> EntityAddOrUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons)
        {
            RedisResult result = null!;

            try
            {
                string luaScript = AssembleBatchAddOrUpdateScript(entityKeys.Count());

                string tempListName = "lst" + SecurityUtil.CreateUniqueToken();

                RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName), tempListName };

                IEnumerable<RedisValue> argvs1 = entityKeys.Select(str => (RedisValue)str);
                IEnumerable<RedisValue> argvs2 = entityJsons.Select(bytes => (RedisValue)bytes);

                RedisValue[] argvs = argvs1.Concat(argvs2).ToArray();

                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);

                await db.ScriptEvaluateAsync(@" redis.call('DEL', KEYS[1]) ", new RedisKey[] { tempListName }).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisConnectionFailed, entityName, "", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisTimeout, entityName, "", ex);
            }
            catch (Exception ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreError, entityName, "", ex);
            }

            return (int[])result;
        }

        public async Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string> entityJsons, IEnumerable<int> entityVersions)
        {
            RedisResult result = null!;

            try
            {
                string luaScript = AssembleBatchUpdateLuaScript(entityKeys.Count());

                RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
                RedisValue[] argvs = entityKeys.Select(t => (RedisValue)t)
                    .Concat(entityJsons.Select(t => (RedisValue)t))
                    .Concat(entityVersions.Select(t => (RedisValue)t)).ToArray();

                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisConnectionFailed, entityName, "", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisTimeout, entityName, "", ex);
            }
            catch (Exception ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreError, entityName, "", ex);
            }

            ErrorCode error = MapResult(result);

            if (!error.IsSuccessful())
            {
                throw new KVStoreException(error, entityName, "");
            }
        }

        public async Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions)
        {
            RedisResult result = null!;

            try
            {
                string luaScript = AssembleBatchDeleteLuaScript(entityKeys.Count());

                RedisKey[] keys = new RedisKey[] { entityName, EntityVersionName(entityName) };
                RedisValue[] argvs = entityKeys.Select(t => (RedisValue)t).Concat(entityVersions.Select(t => (RedisValue)t)).ToArray();

                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                result = await db.ScriptEvaluateAsync(luaScript.ToString(GlobalSettings.Culture), keys, argvs).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisConnectionFailed, entityName, "", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisTimeout, entityName, "", ex);
            }
            catch (Exception ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreError, entityName, "", ex);
            }

            ErrorCode error = MapResult(result);

            if (!error.IsSuccessful())
            {
                throw new KVStoreException(error, entityName, "");
            }
        }


        public async Task<bool> EntityDeleteAllAsync(string storeName, string entityName)
        {
            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                return await db.KeyDeleteAsync(entityName).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisConnectionFailed, entityName, "", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreRedisTimeout, entityName, "", ex);
            }
            catch (Exception ex)
            {
                throw new KVStoreException(ErrorCode.KVStoreError, entityName, "", ex);
            }
        }

        public void Close()
        {
            _instanceSettingDict.ForEach(kv =>
            {
                RedisInstanceManager.Close(kv.Value);
            });
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

        private static ErrorCode MapResult(RedisResult redisResult)
        {
            int result = (int)redisResult;

            ErrorCode error = result switch
            {
                9 => ErrorCode.KVStoreExistAlready,
                7 => ErrorCode.KVStoreVersionNotMatched,
                0 => ErrorCode.KVStoreError,
                1 => ErrorCode.OK,
                _ => ErrorCode.KVStoreError,
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

        private static string AssembleBatchAddOrUpdateScript(int count)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchAddOrUpdateTemplate, i + 1, i + count + 1);
            }

            stringBuilder.Append(_luaBatchAddOrUpdateReturnTemplate);

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
                stringBuilder.AppendFormat(GlobalSettings.Culture, _luaBatchUpdateTemplate, i + 1, i + count + 1);
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
    }
}

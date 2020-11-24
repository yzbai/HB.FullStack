using HB.Framework.KVStore;
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
    internal class RedisKVStoreEngine : IKVStoreEngine
    {
        /// <summary>
        /// keys:entityNameKey, entityVersionKey
        /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_value, entity2_value, entity3_value
        /// </summary>
        private const string _luaBatchAdd = @"
local count = tonumber(ARGV[1])
for i = 1, count do
    if (redis.call('hexists', KEYS[1], ARGV[i+1]) == 1) then
        return 9
    end
end

for i =1, count do
    redis.call('hset', KEYS[1], ARGV[i+1], ARGV[count + i + 1])
    redis.call('hset', KEYS[2], ARGV[i+1], 0)
end
return 1";

        /// <summary>
        /// keys:entityNameKey, entityVersionKey
        /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_value, entity2_value, entity3_value, entity1_version, entity2_version, entity3_version
        /// </summary>
        private const string _luaBatchUpdate = @"
local count = tonumber(ARGV[1])
for i =1,count do
    if redis.call('HGET',KEYS[2],ARGV[i+1])~=ARGV[count + count + i + 1] then return 7 end
end

for i = 1, count do
    redis.call('HSET',KEYS[1],ARGV[i+1],ARGV[count+i+1]) 
    redis.call('HINCRBY',KEYS[2],ARGV[i+1],1)
end

return 1";

        /// <summary>
        /// keys:entityNameKey, entityVersionKey
        /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_version, entity2_version, entity3_version
        /// </summary>
        private const string _luaBatchDelete = @"
local count=tonumber(ARGV[1])
for i = 1, count do
    if redis.call('HGET',KEYS[2],ARGV[i+1])~=ARGV[count+i+1] then
        return 7 
    end
end

for i=1,count do
    redis.call('HDEL',KEYS[1],ARGV[i+1]) 
    redis.call('HDEL',KEYS[2],ARGV[i+1]) 
end

return 1
";

        /// <summary>
        /// keys:entityNameKey, entityVersionKey, tempListKey
        /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_value, entity2_value, entity3_value
        /// </summary>
        private const string _luaBatchAddOrUpdate = @"
local count=tonumber(ARGV[1])
for i = 1, count dot
    if (redis.call('HEXISTS',KEYS[1],ARGV[i+1])==1) then 
        redis.call('HSET',KEYS[1],ARGV[i+1],ARGV[count+i+1]) 
        local version= redis.call('HINCRBY',KEYS[2],ARGV[i+1],1) 
        redis.call('RPUSH',KEYS[3],version) 
    else 
        redis.call('HSET',KEYS[1],ARGV[i+1],ARGV[count+i+1]) 
        redis.call('HSET',KEYS[2],ARGV[i+1],0) 
        redis.call('RPUSH',KEYS[3],0) 
    end 
end

local result = redis.call('LRANGE',KEYS[3],0,-1)

redis.call('DEL', KEYS[3])

return result";

        /// <summary>
        /// keys:entityNameKey, entityVersionKey
        /// argv:entity1_key, entity2_key, entity3_key
        /// </summary>
        private const string _luaBatchGet = @"
local array={{}} 
array[1]=redis.call('HMGET',KEYS[1],unpack(ARGV)) 
array[2]=redis.call('HMGET',KEYS[2],unpack(ARGV)) 
return array
";

        /// <summary>
        /// keys:entityNameKey, entityVersionKey
        /// </summary>
        private const string _luaGetAll = @"
local array={{}} 
array[1]=redis.call('HGETALL',KEYS[1]) 
array[2]=redis.call('HGETALL',KEYS[2]) 
return array";

        private readonly RedisKVStoreOptions _options;
        private readonly ILogger _logger;

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;
        private readonly IDictionary<string, LoadedLuas> _loadedLuaDict = new Dictionary<string, LoadedLuas>();

        public KVStoreSettings Settings { get { return _options.KVStoreSettings; } }

        public string FirstDefaultInstanceName { get { return _options.ConnectionSettings[0].InstanceName; } }

        public RedisKVStoreEngine(IOptions<RedisKVStoreOptions> options, ILogger<RedisKVStoreEngine> logger)
        {
            _logger = logger;
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);

            InitLoadedLuas();
        }

        private void InitLoadedLuas()
        {
            foreach (RedisInstanceSetting setting in _options.ConnectionSettings)
            {
                IServer server = RedisInstanceManager.GetServer(setting);
                LoadedLuas loadedLuas = new LoadedLuas();

                loadedLuas.LoadedBatchAddLua = server.ScriptLoad(_luaBatchAdd);
                loadedLuas.LoadedBatchUpdateLua = server.ScriptLoad(_luaBatchUpdate);
                loadedLuas.LoadedBatchDeleteLua = server.ScriptLoad(_luaBatchDelete);
                loadedLuas.LodedeBatchAddOrUpdateLua = server.ScriptLoad(_luaBatchAddOrUpdate);
                loadedLuas.LoadedBatchGetLua = server.ScriptLoad(_luaBatchGet);
                loadedLuas.LoadedGetAllLua = server.ScriptLoad(_luaGetAll);

                _loadedLuaDict[setting.InstanceName] = loadedLuas;
            }
        }

        private LoadedLuas GetLoadedLuas(string instanceName)
        {
            if (_loadedLuaDict.TryGetValue(instanceName, out LoadedLuas loadedLuas))
            {
                return loadedLuas;
            }

            InitLoadedLuas();

            if (_loadedLuaDict.TryGetValue(instanceName, out LoadedLuas loadedLuas2))
            {
                return loadedLuas2;
            }

            throw new CacheException(ErrorCode.CacheLoadedLuaNotFound, $"Can not found LoadedLua Redis Instance: {instanceName}");
        }

        public async Task<IEnumerable<Tuple<string?, int>>> EntityGetAsync(string storeName, string entityName, IEnumerable<string> entityKeys)
        {
            if (!entityKeys.Any())
            {
                return new List<Tuple<string?, int>>();
            }

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareEntityGetRedisInfo(entityName, entityKeys, redisKeys, redisValues);


            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedBatchGetLua;

            try
            {
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                return MapResultToStringWithVersion(result);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await EntityGetAsync(storeName, entityName, entityKeys).ConfigureAwait(false);
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

        private void PrepareEntityGetRedisInfo(string entityName, IEnumerable<string> entityKeys, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            redisKeys.Add(EntityNameKey(entityName));
            redisKeys.Add(EntityVersionNameKey(entityName));


            foreach (string entityKey in entityKeys)
            {
                redisValues.Add(entityKey);
            }
        }

        public async Task<IEnumerable<Tuple<string?, int>>> EntityGetAllAsync(string storeName, string entityName)
        {
            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedGetAllLua;

            List<RedisKey> redisKeys = new List<RedisKey>();

            PrepareEntityGetAllRedisInfo(entityName, redisKeys);

            try
            {
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    new RedisKey[] { EntityNameKey(entityName), EntityVersionNameKey(entityName) }).ConfigureAwait(false);

                return MapGetAllResultToStringWithVersion(result);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await EntityGetAllAsync(storeName, entityName).ConfigureAwait(false);
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

        private void PrepareEntityGetAllRedisInfo(string entityName, List<RedisKey> redisKeys)
        {
            redisKeys.Add(EntityNameKey(entityName));
            redisKeys.Add(EntityVersionNameKey(entityName));
        }

        public async Task EntityAddAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons)
        {
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedBatchAddLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareEntityAddRedisInfo(entityName, entityKeys, entityJsons, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (!error.IsSuccessful())
                {
                    throw new KVStoreException(error, entityName, "");
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await EntityAddAsync(storeName, entityName, entityKeys, entityJsons).ConfigureAwait(false);
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

        private void PrepareEntityAddRedisInfo(string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys:entityNameKey, entityVersionKey
            /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_value, entity2_value, entity3_value

            redisKeys.Add(EntityNameKey(entityName));
            redisKeys.Add(EntityVersionNameKey(entityName));

            redisValues.Add(entityKeys.Count());

            foreach (string entityKey in entityKeys)
            {
                redisValues.Add(entityKey);
            }

            foreach (string? json in entityJsons)
            {
                redisValues.Add(json);
            }
        }

        /// <summary>
        /// 返回最新的Version
        /// </summary>
        public async Task<IEnumerable<int>> EntityAddOrUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons)
        {
            byte[] loadedScript = GetLoadedLuas(storeName).LodedeBatchAddOrUpdateLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareEntityAddOrUpdateRedisInfo(entityName, entityKeys, entityJsons, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                return (int[])result;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await EntityAddOrUpdateAsync(storeName, entityName, entityKeys, entityJsons).ConfigureAwait(false);
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

        private void PrepareEntityAddOrUpdateRedisInfo(string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys:entityNameKey, entityVersionKey, tempListKey
            /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_value, entity2_value, entity3_value

            redisKeys.Add(EntityNameKey(entityName));
            redisKeys.Add(EntityVersionNameKey(entityName));
            redisKeys.Add("tmpL" + SecurityUtil.CreateUniqueToken());


            redisValues.Add(entityKeys.Count());

            foreach (string entityKey in entityKeys)
            {
                redisValues.Add(entityKey);
            }

            foreach (string? json in entityJsons)
            {
                redisValues.Add(json);
            }
        }

        public async Task EntityUpdateAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons, IEnumerable<int> entityVersions)
        {
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedBatchUpdateLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareEntityUpdateRedisInfo(entityName, entityKeys, entityJsons, entityVersions, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (!error.IsSuccessful())
                {
                    throw new KVStoreException(error, entityName, "");
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await EntityUpdateAsync(storeName, entityName, entityKeys, entityJsons, entityVersions).ConfigureAwait(false);
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

        private void PrepareEntityUpdateRedisInfo(string entityName, IEnumerable<string> entityKeys, IEnumerable<string?> entityJsons, IEnumerable<int> entityVersions, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys:entityNameKey, entityVersionKey
            /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_value, entity2_value, entity3_value, entity1_version, entity2_version, entity3_version

            redisKeys.Add(EntityNameKey(entityName));
            redisKeys.Add(EntityVersionNameKey(entityName));

            redisValues.Add(entityKeys.Count());

            foreach (string entityKey in entityKeys)
            {
                redisValues.Add(entityKey);
            }

            foreach (string? json in entityJsons)
            {
                redisValues.Add(json);
            }

            foreach (int version in entityVersions)
            {
                redisValues.Add(version);
            }
        }

        public async Task EntityDeleteAsync(string storeName, string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions)
        {
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedBatchDeleteLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareEntityDeleteRedisInfo(entityName, entityKeys, entityVersions, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (!error.IsSuccessful())
                {
                    throw new KVStoreException(error, entityName, "");
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await EntityDeleteAsync(storeName, entityName, entityKeys, entityVersions).ConfigureAwait(false);
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

        private void PrepareEntityDeleteRedisInfo(string entityName, IEnumerable<string> entityKeys, IEnumerable<int> entityVersions, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys:entityNameKey, entityVersionKey
            /// argv:3(entity_number), entity1_key, entity2_key, entity3_key, entity1_version, entity2_version, entity3_version
            /// 
            redisKeys.Add(EntityNameKey(entityName));
            redisKeys.Add(EntityVersionNameKey(entityName));

            redisValues.Add(entityKeys.Count());

            foreach (string entityKey in entityKeys)
            {
                redisValues.Add(entityKey);
            }

            foreach (int version in entityVersions)
            {
                redisValues.Add(version);
            }
        }

        public async Task<bool> EntityDeleteAllAsync(string storeName, string entityName)
        {
            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                return await db.KeyDeleteAsync(EntityNameKey(entityName)).ConfigureAwait(false);
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
                return await RedisInstanceManager.GetDatabaseAsync(setting).ConfigureAwait(false);
            }

            throw new KVStoreException($"Can not found Such Redis Instance: {instanceName}");
        }

        private string EntityNameKey(string entityName)
        {
            return _options.ApplicationName + entityName;
        }

        private string EntityVersionNameKey(string entityName)
        {
            return _options.ApplicationName + entityName + "_V";
        }

        private static ErrorCode MapResultToErrorCode(RedisResult redisResult)
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

        private static IEnumerable<Tuple<string?, int>> MapResultToStringWithVersion(RedisResult result)
        {
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

        private static IEnumerable<Tuple<string?, int>> MapGetAllResultToStringWithVersion(RedisResult result)
        {
            RedisResult[] results = (RedisResult[])result;
            Dictionary<string, RedisResult> values = results[0].ToDictionary();
            Dictionary<string, RedisResult> versions = results[1].ToDictionary();

            List<Tuple<string?, int>> rt = new List<Tuple<string?, int>>();

            values.ForEach(kv =>
            {
                int version = (int)versions[kv.Key];
                rt.Add(new Tuple<string?, int>(kv.Value.ToString(), version));
            });

            return rt;
        }
    }
}

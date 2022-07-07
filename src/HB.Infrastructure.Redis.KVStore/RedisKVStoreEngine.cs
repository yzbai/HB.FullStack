using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Engine;
using HB.Infrastructure.Redis.Shared;

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
    public class RedisKVStoreEngine : IKVStoreEngine
    {
        /// <summary>
        /// keys:modelNameKey, modelVersionKey
        /// argv:3(model_number), model1_key, model2_key, model3_key, model1_value, model2_value, model3_value
        /// </summary>
        private const string LUA_BATCH_ADD = @"
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
        /// keys:modelNameKey, modelVersionKey
        /// argv:3(model_number), model1_key, model2_key, model3_key, model1_value, model2_value, model3_value, model1_version, model2_version, model3_version
        /// </summary>
        private const string LUA_BATCH_UPDATE = @"
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
        /// keys:modelNameKey, modelVersionKey
        /// argv:3(model_number), model1_key, model2_key, model3_key, model1_version, model2_version, model3_version
        /// </summary>
        private const string LUA_BATCH_DELETE = @"
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
        /// keys:modelNameKey, modelVersionKey
        /// argv:model1_key, model2_key, model3_key
        /// </summary>
        private const string LUA_BATCH_GET = @"
local array={{}}
array[1]=redis.call('HMGET',KEYS[1],unpack(ARGV))
array[2]=redis.call('HMGET',KEYS[2],unpack(ARGV))
return array
";

        /// <summary>
        /// keys:modelNameKey, modelVersionKey
        /// </summary>
        private const string LUA_GET_ALL = @"
local array={{}}
array[1]=redis.call('HGETALL',KEYS[1])
array[2]=redis.call('HGETALL',KEYS[2])
return array";

        private readonly RedisKVStoreOptions _options;
        private readonly ILogger _logger;

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;
        private readonly IDictionary<string, LoadedLuas> _loadedLuaDict = new Dictionary<string, LoadedLuas>();

        public KVStoreSettings Settings
        { get { return _options.KVStoreSettings; } }

        public string FirstDefaultInstanceName
        { get { return _options.ConnectionSettings[0].InstanceName; } }

        public RedisKVStoreEngine(IOptions<RedisKVStoreOptions> options, ILogger<RedisKVStoreEngine> logger)
        {
            _logger = logger;
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);

            InitLoadedLuas();

            _logger.LogInformation("RedisKVStoreEngine初始化完成");
        }

        private void InitLoadedLuas()
        {
            foreach (RedisInstanceSetting setting in _options.ConnectionSettings)
            {
                IServer server = RedisInstanceManager.GetServer(setting, _logger);
                LoadedLuas loadedLuas = new LoadedLuas
                {
                    LoadedBatchAddLua = server.ScriptLoad(LUA_BATCH_ADD),
                    LoadedBatchUpdateLua = server.ScriptLoad(LUA_BATCH_UPDATE),
                    LoadedBatchDeleteLua = server.ScriptLoad(LUA_BATCH_DELETE),
                    LoadedBatchGetLua = server.ScriptLoad(LUA_BATCH_GET),
                    LoadedGetAllLua = server.ScriptLoad(LUA_GET_ALL)
                };

                _loadedLuaDict[setting.InstanceName] = loadedLuas;
            }
        }

        /// <summary>
        /// GetLoadedLuas
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>

        private LoadedLuas GetLoadedLuas(string instanceName)
        {
            if (_loadedLuaDict.TryGetValue(instanceName, out LoadedLuas? loadedLuas))
            {
                return loadedLuas;
            }

            InitLoadedLuas();

            if (_loadedLuaDict.TryGetValue(instanceName, out LoadedLuas? loadedLuas2))
            {
                return loadedLuas2;
            }

            throw Exceptions.CacheLoadedLuaNotFound(instanceName: instanceName);
        }

        /// <summary>
        /// ModelGetAsync
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="modelName"></param>
        /// <param name="modelKeys"></param>
        /// <returns></returns>

        public async Task<IEnumerable<Tuple<string?, int>>> ModelGetAsync(string storeName, string modelName, IEnumerable<string> modelKeys)
        {
            if (!modelKeys.Any())
            {
                return new List<Tuple<string?, int>>();
            }

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareModelGetRedisInfo(modelName, modelKeys, redisKeys, redisValues);

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
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await ModelGetAsync(storeName, modelName, modelKeys).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw Exceptions.KVStoreRedisConnectionFailed(type: modelName, innerException: ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw Exceptions.KVStoreRedisTimeout(type: modelName, innerException: ex);
            }
            catch (Exception ex)
            {
                throw Exceptions.Unkown(type: modelName, innerException: ex);
            }
        }

        private void PrepareModelGetRedisInfo(string modelName, IEnumerable<string> modelKeys, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            redisKeys.Add(ModelNameKey(modelName));
            redisKeys.Add(ModelVersionNameKey(modelName));

            foreach (string modelKey in modelKeys)
            {
                redisValues.Add(modelKey);
            }
        }

        /// <summary>
        /// ModelGetAllAsync
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>

        public async Task<IEnumerable<Tuple<string?, int>>> ModelGetAllAsync(string storeName, string modelName)
        {
            IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedGetAllLua;

            List<RedisKey> redisKeys = new List<RedisKey>();

            PrepareModelGetAllRedisInfo(modelName, redisKeys);

            try
            {
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    new RedisKey[] { ModelNameKey(modelName), ModelVersionNameKey(modelName) }).ConfigureAwait(false);

                return MapGetAllResultToStringWithVersion(result);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await ModelGetAllAsync(storeName, modelName).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw Exceptions.KVStoreRedisConnectionFailed(type: modelName, innerException: ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw Exceptions.KVStoreRedisTimeout(type: modelName, innerException: ex);
            }
            catch (Exception ex)
            {
                throw Exceptions.Unkown(modelName, ex);
            }
        }

        private void PrepareModelGetAllRedisInfo(string modelName, List<RedisKey> redisKeys)
        {
            redisKeys.Add(ModelNameKey(modelName));
            redisKeys.Add(ModelVersionNameKey(modelName));
        }

        /// <summary>
        /// ModelAddAsync
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="modelName"></param>
        /// <param name="modelKeys"></param>
        /// <param name="modelJsons"></param>
        /// <returns></returns>

        public async Task ModelAddAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons)
        {
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedBatchAddLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareModelAddRedisInfo(modelName, modelKeys, modelJsons, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (error != ErrorCode.Empty)
                {
                    throw Exceptions.WriteError(type: modelName, storeName: storeName, keys: modelKeys, values: modelJsons, errorCode: error);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await ModelAddAsync(storeName, modelName, modelKeys, modelJsons).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw Exceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw Exceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw Exceptions.Unkown(modelName, storeName, modelKeys, ex);
            }
        }

        private void PrepareModelAddRedisInfo(string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys:modelNameKey, modelVersionKey
            /// argv:3(model_number), model1_key, model2_key, model3_key, model1_value, model2_value, model3_value

            redisKeys.Add(ModelNameKey(modelName));
            redisKeys.Add(ModelVersionNameKey(modelName));

            redisValues.Add(modelKeys.Count());

            foreach (string modelKey in modelKeys)
            {
                redisValues.Add(modelKey);
            }

            foreach (string? json in modelJsons)
            {
                redisValues.Add(json);
            }
        }

        /// <summary>
        /// ModelUpdateAsync
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="modelName"></param>
        /// <param name="modelKeys"></param>
        /// <param name="modelJsons"></param>
        /// <param name="modelVersions"></param>
        /// <returns></returns>

        public async Task ModelUpdateAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons, IEnumerable<int> modelVersions)
        {
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedBatchUpdateLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareModelUpdateRedisInfo(modelName, modelKeys, modelJsons, modelVersions, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (error != ErrorCode.Empty)
                {
                    throw Exceptions.WriteError(type: modelName, storeName: storeName, keys: modelKeys, values: modelJsons, errorCode: error);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await ModelUpdateAsync(storeName, modelName, modelKeys, modelJsons, modelVersions).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw Exceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw Exceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw Exceptions.Unkown(modelName, ex);
            }
        }

        private void PrepareModelUpdateRedisInfo(string modelName, IEnumerable<string> modelKeys, IEnumerable<string?> modelJsons, IEnumerable<int> modelVersions, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys:modelNameKey, modelVersionKey
            /// argv:3(model_number), model1_key, model2_key, model3_key, model1_value, model2_value, model3_value, model1_version, model2_version, model3_version

            redisKeys.Add(ModelNameKey(modelName));
            redisKeys.Add(ModelVersionNameKey(modelName));

            redisValues.Add(modelKeys.Count());

            foreach (string modelKey in modelKeys)
            {
                redisValues.Add(modelKey);
            }

            foreach (string? json in modelJsons)
            {
                redisValues.Add(json);
            }

            foreach (int version in modelVersions)
            {
                redisValues.Add(version);
            }
        }

        /// <summary>
        /// ModelDeleteAsync
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="modelName"></param>
        /// <param name="modelKeys"></param>
        /// <param name="modelVersions"></param>
        /// <returns></returns>

        public async Task ModelDeleteAsync(string storeName, string modelName, IEnumerable<string> modelKeys, IEnumerable<int> modelVersions)
        {
            byte[] loadedScript = GetLoadedLuas(storeName).LoadedBatchDeleteLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareModelDeleteRedisInfo(modelName, modelKeys, modelVersions, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (error != ErrorCode.Empty)
                {
                    throw Exceptions.WriteError(type: modelName, storeName: storeName, keys: modelKeys, values: modelVersions, errorCode: error);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await ModelDeleteAsync(storeName, modelName, modelKeys, modelVersions).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw Exceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw Exceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw Exceptions.Unkown(modelName, ex);
            }
        }

        private void PrepareModelDeleteRedisInfo(string modelName, IEnumerable<string> modelKeys, IEnumerable<int> modelVersions, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys:modelNameKey, modelVersionKey
            /// argv:3(model_number), model1_key, model2_key, model3_key, model1_version, model2_version, model3_version
            ///
            redisKeys.Add(ModelNameKey(modelName));
            redisKeys.Add(ModelVersionNameKey(modelName));

            redisValues.Add(modelKeys.Count());

            foreach (string modelKey in modelKeys)
            {
                redisValues.Add(modelKey);
            }

            foreach (int version in modelVersions)
            {
                redisValues.Add(version);
            }
        }

        /// <summary>
        /// ModelDeleteAllAsync
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>

        public async Task<bool> ModelDeleteAllAsync(string storeName, string modelName)
        {
            try
            {
                IDatabase db = await GetDatabaseAsync(storeName).ConfigureAwait(false);

                return await db.KeyDeleteAsync(ModelNameKey(modelName)).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw Exceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw Exceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw Exceptions.Unkown(modelName, ex);
            }
        }

        public void Close()
        {
            foreach (var kv in _instanceSettingDict)
            {
                RedisInstanceManager.Close(kv.Value);
            }
        }

        /// <summary>
        /// GetDatabaseAsync
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>

        private async Task<IDatabase> GetDatabaseAsync(string instanceName)
        {
            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting? setting))
            {
                return await RedisInstanceManager.GetDatabaseAsync(setting, _logger).ConfigureAwait(false);
            }

            throw Exceptions.NoSuchInstance(instanceName: instanceName);
        }

        private string ModelNameKey(string modelName)
        {
            return _options.ApplicationName + modelName;
        }

        private string ModelVersionNameKey(string modelName)
        {
            return _options.ApplicationName + modelName + "_V";
        }

        private static ErrorCode MapResultToErrorCode(RedisResult redisResult)
        {
            int result = (int)redisResult;

            ErrorCode error = result switch
            {
                9 => KVStoreErrorCodes.KVStoreExistAlready,
                7 => KVStoreErrorCodes.KVStoreVersionNotMatched,
                0 => KVStoreErrorCodes.KVStoreError,
                1 => ErrorCode.Empty,
                _ => KVStoreErrorCodes.KVStoreError,
            };

            return error;
        }

        private static IEnumerable<Tuple<string?, int>> MapResultToStringWithVersion(RedisResult result)
        {
            RedisResult[] results = (RedisResult[])result!;
            string[] values = (string[])results[0]!;
            int[] version = (int[])results[1]!;

            List<Tuple<string?, int>> rt = new List<Tuple<string?, int>>();

            for (int i = 0; i < values.Length; ++i)
            {
                rt.Add(new Tuple<string?, int>(values[i], version[i]));
            }

            return rt;
        }

        private static IEnumerable<Tuple<string?, int>> MapGetAllResultToStringWithVersion(RedisResult result)
        {
            RedisResult[] results = (RedisResult[])result!;
            Dictionary<string, RedisResult> values = results[0].ToDictionary();
            Dictionary<string, RedisResult> versions = results[1].ToDictionary();

            List<Tuple<string?, int>> rt = new List<Tuple<string?, int>>();

            foreach (var kv in values)
            {
                int version = (int)versions[kv.Key];
                rt.Add(new Tuple<string?, int>(kv.Value.ToString(), version));
            }

            return rt;
        }
    }
}
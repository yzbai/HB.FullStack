using HB.FullStack.KVStore.Config;
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
    //存储结构
    // modelNameKey ----------| model1_key  - model1_value
    //                        | model2_key  - model2_value   
    //                        | model3_key  - model3_value
    //
    // modelTimestampKey -----| model1_key  - model1_timestamp
    //                        | model2_key  - model2_timestamp
    //                        | model3_key  - model3_timestamp

    public class RedisKVStoreEngine : IKVStoreEngine
    {

        /// <summary>
        /// keys:modelNameKey, modelTimestampKey
        /// argv:3(model_number),new_timestamp, model1_key, model2_key, model3_key, model1_value, model2_value, model3_value
        /// </summary>
        private const string LUA_BATCH_ADD_2 = @"
local count = tonumber(ARGV[1])
for i = 1, count do
    if (redis.call('hexists', KEYS[1], ARGV[i+2]) == 1) then
        return 9
    end
end

for i =1, count do
    redis.call('hset', KEYS[1], ARGV[i+2], ARGV[count + i + 2])
    redis.call('hset', KEYS[2], ARGV[i+2], ARGV[2])
end
return 1";

        /// <summary>
        /// keys:modelNameKey, modelTimestampKey
        /// argv:3(model_number), new_timestamp, model1_key, model2_key, model3_key, model1_value, model2_value, model3_value, model1_old_timestamp, model2_old_timestamp, model3_old_timestamp
        /// </summary>
        private const string LUA_BATCH_UPDATE_2 = @"
local count = tonumber(ARGV[1])
for i =1,count do
    if redis.call('HGET',KEYS[2],ARGV[i+2])~=ARGV[count + count + i + 2] then return 7 end
end

for i = 1, count do
    redis.call('HSET',KEYS[1],ARGV[i+2],ARGV[count+i+2])
    redis.call('HSET',KEYS[2],ARGV[i+2],ARGV[2])
end

return 1";

        /// <summary>
        /// keys:modelNameKey, modelTimestampKey
        /// argv:3(model_number), model1_key, model2_key, model3_key, model1_old_timestamp, model2_old_timestamp, model3_old_timestamp
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
        /// keys:modelNameKey, modelTimestampKey
        /// argv:model1_key, model2_key, model3_key
        /// </summary>
        private const string LUA_BATCH_GET = @"
local array={{}}
array[1]=redis.call('HMGET',KEYS[1],unpack(ARGV))
array[2]=redis.call('HMGET',KEYS[2],unpack(ARGV))
return array
";

        /// <summary>
        /// keys:modelNameKey, modelTimestampKey
        /// </summary>
        private const string LUA_GET_ALL = @"
local array={{}}
array[1]=redis.call('HGETALL',KEYS[1])
array[2]=redis.call('HGETALL',KEYS[2])
return array";

        private readonly ILogger _logger;
        private KVStoreOptions _options = null!;

        private IDictionary<string, RedisInstanceSetting> _instanceSettingDict = null!;
        private readonly IDictionary<string, LoadedLuas> _loadedLuaDict = new Dictionary<string, LoadedLuas>();


        public RedisKVStoreEngine(ILogger<RedisKVStoreEngine> logger)
        {
            _logger = logger;
        }

        public void Initialize(KVStoreOptions options)
        {
            _options = options;

            _instanceSettingDict = options.KVStoreSchemas.Select(s => new RedisInstanceSetting
            {
                InstanceName = s.Name,
                ConnectionString = s.ConnectionString
            }).ToDictionary(r => r.InstanceName);

            InitLoadedLuas();

            _logger.LogInformation("RedisKVStoreEngine初始化完成");
        }

        private void InitLoadedLuas()
        {
            foreach (RedisInstanceSetting setting in _instanceSettingDict.Values)
            {
                IServer server = RedisInstanceManager.GetServer(setting, _logger);

                LoadedLuas loadedLuas = new LoadedLuas
                {
                    LoadedBatchAddLua = server.ScriptLoad(LUA_BATCH_ADD_2),
                    LoadedBatchUpdateLua = server.ScriptLoad(LUA_BATCH_UPDATE_2),
                    LoadedBatchDeleteLua = server.ScriptLoad(LUA_BATCH_DELETE),
                    LoadedBatchGetLua = server.ScriptLoad(LUA_BATCH_GET),
                    LoadedGetAllLua = server.ScriptLoad(LUA_GET_ALL)
                };

                _loadedLuaDict[setting.InstanceName] = loadedLuas;
            }
        }

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

            throw FullStack.KVStore.KVStoreExceptions.CacheLoadedLuaNotFound(instanceName: instanceName);
        }

        public async Task<IEnumerable<Tuple<byte[]?, long>>> GetAsync(string schemaName, string modelName, IEnumerable<string> modelKeys)
        {
            if (!modelKeys.Any())
            {
                return new List<Tuple<byte[]?, long>>();
            }

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareGetRedisInfo(modelName, modelKeys, redisKeys, redisValues);

            IDatabase db = await GetDatabaseAsync(schemaName).ConfigureAwait(false);
            byte[] loadedScript = GetLoadedLuas(schemaName).LoadedBatchGetLua;

            try
            {
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                return MapResultToBytesAndTimestamp(result);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await GetAsync(schemaName, modelName, modelKeys).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisConnectionFailed(type: modelName, innerException: ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisTimeout(type: modelName, innerException: ex);
            }
            catch (Exception ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.Unkown(type: modelName, innerException: ex);
            }

            void PrepareGetRedisInfo(string modelName, IEnumerable<string> modelKeys, List<RedisKey> redisKeys, List<RedisValue> redisValues)
            {
                redisKeys.Add(ModelNameKey(modelName));
                redisKeys.Add(ModelTimestampNameKey(modelName));

                foreach (string modelKey in modelKeys)
                {
                    redisValues.Add(modelKey);
                }
            }

            static IEnumerable<Tuple<byte[]?, long>> MapResultToBytesAndTimestamp(RedisResult result)
            {
                RedisResult[] results = (RedisResult[])result!;
                byte[]?[] values = (byte[]?[])results[0]!;
                long[] timestamps = (long[])results[1]!;

                List<Tuple<byte[]?, long>> rt = new List<Tuple<byte[]?, long>>();

                for (int i = 0; i < values.Length; ++i)
                {
                    rt.Add(new Tuple<byte[]?, long>(values[i], timestamps[i]));
                }

                return rt;
            }
        }

        public async Task<IEnumerable<Tuple<byte[]?, long>>> GetAllAsync(string schemaName, string modelName)
        {
            IDatabase db = await GetDatabaseAsync(schemaName).ConfigureAwait(false);
            byte[] loadedScript = GetLoadedLuas(schemaName).LoadedGetAllLua;

            List<RedisKey> redisKeys = new List<RedisKey>();

            PrepareGetAllRedisInfo(modelName, redisKeys);

            try
            {
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    new RedisKey[] { ModelNameKey(modelName), ModelTimestampNameKey(modelName) }).ConfigureAwait(false);

                return MapGetAllResultToBytesAndTimestamp(result);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await GetAllAsync(schemaName, modelName).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisConnectionFailed(type: modelName, innerException: ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisTimeout(type: modelName, innerException: ex);
            }
            catch (Exception ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.Unkown(modelName, ex);
            }

            void PrepareGetAllRedisInfo(string modelName, List<RedisKey> redisKeys)
            {
                redisKeys.Add(ModelNameKey(modelName));
                redisKeys.Add(ModelTimestampNameKey(modelName));
            }

            static IEnumerable<Tuple<byte[]?, long>> MapGetAllResultToBytesAndTimestamp(RedisResult result)
            {
                RedisResult[] results = (RedisResult[])result!;
                Dictionary<string, RedisResult> values = results[0].ToDictionary();
                Dictionary<string, RedisResult> timestamps = results[1].ToDictionary();

                List<Tuple<byte[]?, long>> rt = new List<Tuple<byte[]?, long>>();

                foreach (var kv in values)
                {
                    long timestamp = (long)timestamps[kv.Key];
                    rt.Add(new Tuple<byte[]?, long>((byte[]?)kv.Value, timestamp));
                }

                return rt;
            }
        }


        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        public async Task AddAsync(string schemaName, string modelName, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, long newTimestamp)
        {
            byte[] loadedScript = GetLoadedLuas(schemaName).LoadedBatchAddLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareAddRedisInfo(modelName, newTimestamp, modelKeys, models, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(schemaName).ConfigureAwait(false);
                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (error != ErrorCode.Empty)
                {
                    throw FullStack.KVStore.KVStoreExceptions.WriteError(type: modelName, schemaName: schemaName, keys: modelKeys, errorCode: error);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await AddAsync(schemaName, modelName, modelKeys, models, newTimestamp).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.Unkown(modelName, schemaName, modelKeys, ex);
            }

            void PrepareAddRedisInfo(string modelName, long newTimestamp, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, List<RedisKey> redisKeys, List<RedisValue> redisValues)
            {
                /// keys:modelNameKey, modelTimestampKey
                /// argv:3(model_number),new_timestamp, model1_key, model2_key, model3_key, model1_value, model2_value, model3_value

                redisKeys.Add(ModelNameKey(modelName));
                redisKeys.Add(ModelTimestampNameKey(modelName));

                redisValues.Add(modelKeys.Count());
                redisValues.Add(newTimestamp);

                foreach (string modelKey in modelKeys)
                {
                    redisValues.Add(modelKey);
                }

                foreach (var bytes in models)
                {
                    redisValues.Add(bytes);
                }
            }
        }

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        public async Task UpdateAsync(string schemaName, string modelName, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, IEnumerable<long> modelTimestamps, long newTimestamp)
        {
            byte[] loadedScript = GetLoadedLuas(schemaName).LoadedBatchUpdateLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareModelUpdateRedisInfo(modelName, newTimestamp, modelKeys, models, modelTimestamps, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(schemaName).ConfigureAwait(false);

                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (error != ErrorCode.Empty)
                {
                    throw FullStack.KVStore.KVStoreExceptions.WriteError(type: modelName, schemaName: schemaName, keys: modelKeys, errorCode: error);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await UpdateAsync(schemaName, modelName, modelKeys, models, modelTimestamps, newTimestamp).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.Unkown(modelName, ex);
            }

            void PrepareModelUpdateRedisInfo(string modelName, long newTimestamp, IEnumerable<string> modelKeys, IEnumerable<byte[]?> models, IEnumerable<long> modelTimestamps, List<RedisKey> redisKeys, List<RedisValue> redisValues)
            {
                /// keys:modelNameKey, modelTimestampKey
                /// argv:3(model_number), new_timestamp, model1_key, model2_key, model3_key, model1_value, model2_value, model3_value, model1_old_timestamp, model2_old_timestamp, model3_old_timestamp

                redisKeys.Add(ModelNameKey(modelName));
                redisKeys.Add(ModelTimestampNameKey(modelName));

                redisValues.Add(modelKeys.Count());
                redisValues.Add(newTimestamp);

                foreach (string modelKey in modelKeys)
                {
                    redisValues.Add(modelKey);
                }

                foreach (var bytes in models)
                {
                    redisValues.Add(bytes);
                }

                foreach (long timestamp in modelTimestamps)
                {
                    redisValues.Add(timestamp);
                }
            }
        }

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        public async Task DeleteAsync(string schemaName, string modelName, IEnumerable<string> modelKeys, IEnumerable<long> modelTimestamps)
        {
            byte[] loadedScript = GetLoadedLuas(schemaName).LoadedBatchDeleteLua;

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            PrepareDeleteRedisInfo(modelName, modelKeys, modelTimestamps, redisKeys, redisValues);

            try
            {
                IDatabase db = await GetDatabaseAsync(schemaName).ConfigureAwait(false);

                RedisResult result = await db.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                ErrorCode error = MapResultToErrorCode(result);

                if (error != ErrorCode.Empty)
                {
                    throw FullStack.KVStore.KVStoreExceptions.WriteError(type: modelName, schemaName: schemaName, keys: modelKeys, errorCode: error);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await DeleteAsync(schemaName, modelName, modelKeys, modelTimestamps).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.Unkown(modelName, ex);
            }

            void PrepareDeleteRedisInfo(string modelName, IEnumerable<string> modelKeys, IEnumerable<long> modelTimestamps, List<RedisKey> redisKeys, List<RedisValue> redisValues)
            {
                /// keys:modelNameKey, modelTimestampKey
                /// argv:3(model_number), model1_key, model2_key, model3_key, model1_old_timestamp, model2_old_timestamp, model3_old_timestamp

                redisKeys.Add(ModelNameKey(modelName));
                redisKeys.Add(ModelTimestampNameKey(modelName));

                redisValues.Add(modelKeys.Count());

                foreach (string modelKey in modelKeys)
                {
                    redisValues.Add(modelKey);
                }

                foreach (long timestamp in modelTimestamps)
                {
                    redisValues.Add(timestamp);
                }
            }
        }

        public async Task<bool> DeleteAllAsync(string schemaName, string modelName)
        {
            try
            {
                IDatabase db = await GetDatabaseAsync(schemaName).ConfigureAwait(false);

                return await db.KeyDeleteAsync(ModelNameKey(modelName)).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisConnectionFailed(modelName, ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.KVStoreRedisTimeout(modelName, ex);
            }
            catch (Exception ex)
            {
                throw FullStack.KVStore.KVStoreExceptions.Unkown(modelName, ex);
            }
        }

        //public void Close()
        //{
        //    foreach (var kv in _instanceSettingDict)
        //    {
        //        RedisInstanceManager.Close(kv.Value);
        //    }
        //}

        private async Task<IDatabase> GetDatabaseAsync(string schemaName)
        {
            if (_instanceSettingDict.TryGetValue(schemaName, out RedisInstanceSetting? setting))
            {
                return await RedisInstanceManager.GetDatabaseAsync(setting, _logger).ConfigureAwait(false);
            }

            throw FullStack.KVStore.KVStoreExceptions.NoSuchKVStoreSchema(schemaName: schemaName);
        }

        private string ModelNameKey(string modelName)
        {
            return _options.ApplicationName + modelName;
        }

        private string ModelTimestampNameKey(string modelName)
        {
            return _options.ApplicationName + modelName + "_TS";
        }

        private static ErrorCode MapResultToErrorCode(RedisResult redisResult)
        {
            int result = (int)redisResult;

            ErrorCode error = result switch
            {
                9 => ErrorCodes.KVStoreExistAlready,
                7 => ErrorCodes.KVStoreTimestampNotMatched,
                0 => ErrorCodes.KVStoreError,
                1 => ErrorCode.Empty,
                _ => ErrorCodes.KVStoreError,
            };

            return error;
        }


    }
}
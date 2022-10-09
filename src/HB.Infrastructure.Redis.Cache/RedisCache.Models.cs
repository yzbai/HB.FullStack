using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Cache
{
    /// <summary>
    /// Model in Redis:
    ///
    ///                                 |------ abexp    :  value
    /// Id------------------------------|
    ///                                 |------ data     :  jsonString
    ///                                 |------ dim      :  keyName
    ///                                 |------ timestamp 
    ///
    ///
    ///                                 |------- DimensionKeyValue_1   :  Id
    /// ModelName_DimensionKeyName----- |......
    ///                                 |------- DimensionKeyValue_n   :  Id
    ///                                 
    /// _minTS..Id ---------------------| 锁，保证model被删除后的一段时间内，没有比这个小的历史版本写到缓存里
    ///
    /// 所以ModelName_DimensionKeyName 这个key是一个索引key
    /// </summary>
    public partial class RedisCache : RedisCacheBase, ICache
    {
        /// <summary>
        /// keys:id1, id2, id3
        /// argv:3(id_number), sldexp, nowInUnixSeconds
        /// </summary>
        public const string LUA_MODELS_GET_AND_REFRESH = @"
local number = tonumber(ARGV[1])
local existCount = redis.call('exists', unpack(KEYS))
if (existCount ~= number) then
    return nil
end

local array={}

for j =1,number do
    local data = redis.call('hmget', KEYS[j], 'absexp','data','dim')

    if(data[1]~='-1') then
        local now = tonumber(ARGV[3])
        local absexp = tonumber(data[1])
        if(now>=absexp) then
            redis.call('del',KEYS[j])

            if (data[3]~='') then
                for i in string.gmatch(data[3], '%S+') do
                   redis.call('del', i)
                end
            end
            return nil
        end
    end

    if(ARGV[2]~='-1') then
        redis.call('expire', KEYS[j], ARGV[2])

        if (data[3]~='') then
            for k in string.gmatch(data[3], '%S+') do
               redis.call('expire', k, ARGV[2])
            end
        end
    end

    array[j]= data[2]
end
return array
";

        /// <summary>
        /// KEYS:dimensionKey1, dimensionKey2, dimensionKey3
        /// ARGV:3(model_count), sldexp, nowInUnixSeconds
        /// </summary>
        public const string LUA_MODELS_GET_AND_REFRESH_BY_DIMENSION = @"
local number = tonumber(ARGV[1])
local existCount = redis.call('exists', unpack(KEYS))
if (existCount ~= number) then
    return nil
end

local array={}

for j =1,number do
    local id = redis.call('get',KEYS[j])

    local data= redis.call('hmget',id, 'absexp','data','dim')

    if (not data[1]) then
        redis.call('del', KEYS[j])
        return nil
    end

    if(data[1]~='-1') then
        local now = tonumber(ARGV[3])
        local absexp = tonumber(data[1])
        if(now>=absexp) then
            redis.call('del',id)

            if (data[3]~='') then
                for i in string.gmatch(data[3], '%S+') do
                   redis.call('del', i)
                end
            end
            return nil
        end
    end

    if(ARGV[2]~='-1') then
        redis.call('expire', id, ARGV[2])

        if (data[3]~='') then
            for k in string.gmatch(data[3], '%S+') do
               redis.call('expire', k, ARGV[2])
            end
        end
    end

    array[j]= data[2]
end
return array";

        //TODO: 是否需要有一个timestamp冲突，那么整个写入失败？
        /// <summary>
        /// _minTS设置timestamp最小值锁。说明历史上最小的timestamp，比这个小的，版本不对，就不用写了。
        /// 返回0为未更新，返回1为更新
        /// keys: model1_idKey, model1_dimensionkey1, model1_dimensionkey2, model1_dimensionkey3, model2_idKey, model2_dimensionkey1, model2_dimensionkey2, model2_dimensionkey3
        /// argv: absexp_value, expire_value,2(model_cout), 3(dimensionkey_count), model1_data, model1_timestamp, model1_dimensionKeyJoinedString, model2_data, model2_timestamp, model2_dimensionKeyJoinedString
        /// </summary>
        public const string LUA_MODELS_SET = @"
local modelNum = tonumber(ARGV[3])
local dimNum = tonumber(ARGV[4])
local rt={}
for j=1, modelNum do
    rt[j]=0
    local keyIndex= 1 + (j-1) *(dimNum+1)

    local minTimestamp = redis.call('get', '_minTS'..KEYS[keyIndex])

    if ((not minTimestamp) or tonumber(minTimestamp)< tonumber(ARGV[6+(j-1)*3])) then

        local cached=redis.call('hget', KEYS[keyIndex], 'timestamp')
        if((not cached) or tonumber(cached)< tonumber(ARGV[6+(j-1) * 3])) then

            redis.call('hmset', KEYS[keyIndex],'absexp',ARGV[1],'data',ARGV[5+(j-1)*3], 'timestamp', ARGV[6+(j-1)*3], 'dim', ARGV[7+(j-1)*3])

            if(ARGV[2]~='-1') then
                redis.call('expire',KEYS[keyIndex], ARGV[2])
            end

            for i=keyIndex+1, keyIndex+dimNum do
                redis.call('set', KEYS[i], KEYS[keyIndex])
                if (ARGV[2]~='-1') then
                    redis.call('expire', KEYS[i], ARGV[2])
                end
            end
            rt[j]=1
        else
            rt[j] = 9
        end
    else
        rt[j]=8
    end

end
return rt";

        //        /// <summary>
        //        /// 删除，并设置最小timestamp锁
        //        /// keys: idKey1, idKey2, idKey3
        //        /// argv: 3(model_num), invalidationKey_expire_seconds, updated_timestamp_value1, updated_timestamp_value2, updated_timestamp_value3
        //        /// </summary>
        //        public const string LUA_MODELS_REMOVE = @"
        //local modelNum = tonumber(ARGV[1])
        //for j=1, modelNum do

        //    redis.call('set', '_minTS'..KEYS[j], ARGV[j+2], 'EX', ARGV[2])

        //    local data=redis.call('hget', KEYS[j], 'dim')

        //    redis.call('del', KEYS[j])

        //    if(data and data~='') then
        //        for i in string.gmatch(data, '%S+') do
        //            redis.call('del', i)
        //        end
        //    end
        //end
        //";

        /// <summary>
        /// 删除，并设置最小timestamp锁
        /// keys: idKey1, idKey2, idKey3
        /// argv: 3(model_num), invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_MODELS_REMOVE_2 = @"
local modelNum = tonumber(ARGV[1])
for j=1, modelNum do
    local data=redis.call('hmget', KEYS[j], 'dim','timestamp')
    if((not data) or (not data[1])) then
        redis.call('del', KEYS[j])        
    else
        if(data[2]) then
            redis.call('set', '_minTS'..KEYS[j], data[2], 'EX', ARGV[2])
        end
        redis.call('del', KEYS[j])
        local dim=data[1]
        if(dim and dim ~= '') then
            for i in string.gmatch(dim, '%S+') do
                redis.call('del', i)
            end
        end
    end
end
";

        //        /// <summary>
        //        /// keys:model1_dimensionkey, model2_dimensionkey, model3_dimensionKey
        //        /// argv: 3(model_count), invalidationKey_expire_seconds, updated_timestamp_value1, updated_timestamp_value2, updated_timestamp_value3
        //        /// </summary>
        //        public const string LUA_MODELS_REMOVE_BY_DIMENSION = @"
        //local modelNum = tonumber(ARGV[1])

        //for j = 1, modelNum do
        //    local id = redis.call('get',KEYS[j])

        //    if (id) then

        //        redis.call('set', '_minTS'..id, ARGV[j+2], 'EX', ARGV[2])

        //        local data= redis.call('hget',id, 'dim')

        //        if (not data) then
        //            redis.call('del', KEYS[1])
        //        else
        //            redis.call('del',id)

        //            if (data~='') then
        //                for i in string.gmatch(data, '%S+') do
        //                    redis.call('del', i)
        //                end
        //            end
        //        end
        //    end
        //end
        //";

        /// <summary>
        /// keys:model1_dimensionkey, model2_dimensionkey, model3_dimensionKey
        /// argv: 3(model_count), invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_MODELS_REMOVE_BY_DIMENSION_2 = @"
local modelNum = tonumber(ARGV[1])

for j = 1, modelNum do
    local id = redis.call('get',KEYS[j])

    if (id) then

        local data= redis.call('hmget',id, 'dim', 'timestamp')

        if ((not data) or (not data[1])) then
            redis.call('del', KEYS[1])
            redis.call('del', id)
        else
            redis.call('del',id)
            if(data[2]) then
                redis.call('set', '_minTS'..id, data[2], 'EX', ARGV[2])
            end
            local dim = data[1]
            
            if (dim and dim ~= '') then
                for i in string.gmatch(data, '%S+') do
                    redis.call('del', i)
                end
            end
        end
    end
end
";

        //        /// <summary>
        //        /// keys:model1_dimensionkey, model2_dimensionkey, model3_dimensionKey
        //        /// argv: 3(model_count)
        //        /// </summary>
        //        public const string LUA_MODELS_REMOVE_BY_DIMENSION_FORCED_NO_VERSION = @"
        //local modelNum = tonumber(ARGV[1])

        //for j = 1, modelNum do
        //    local id = redis.call('get',KEYS[j])

        //    if (id) then

        //        local data= redis.call('hget',id, 'dim')

        //        if (not data) then
        //            redis.call('del', KEYS[1])
        //        else
        //            redis.call('del',id)

        //            if (data~='') then
        //                for i in string.gmatch(data, '%S+') do
        //                    redis.call('del', i)
        //                end
        //            end
        //        end
        //    end
        //end
        //";

        //        /// <summary>
        //        /// keys: idKey1, idKey2, idKey3
        //        /// argv: 3(model_num)
        //        /// </summary>
        //        public const string LUA_MODELS_REMOVE_FORECED_NO_VERSION = @"
        //local modelNum = tonumber(ARGV[1])
        //for j=1, modelNum do

        //    local data=redis.call('hget', KEYS[j], 'dim')

        //    redis.call('del', KEYS[j])

        //    if(data and data~='') then
        //        for i in string.gmatch(data, '%S+') do
        //            redis.call('del', i)
        //        end
        //    end
        //end
        //";

        public RedisCache(IOptions<RedisCacheOptions> options, ILogger<RedisCache> logger) : base(options, logger)
        {
            Logger.LogInformation($"RedisCache初始化完成");
        }

        public async Task<(IList<TModel>?, bool)> GetModelsAsync<TModel>(string keyName, IEnumerable keyValues, CancellationToken token = default) where TModel : ITimestampModel, new()
        {
            ThrowIf.Null(keyValues, nameof(keyValues));

            CacheModelDef? modelDef = CacheModelDefFactory.Get<TModel>();

            if (modelDef == null)
            {
                throw CacheExceptions.NotEnabledForModel(typeof(TModel).FullName);
            }

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            byte[] loadedScript = AddGetModelsRedisInfo(keyName, keyValues, modelDef, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(modelDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                return MapGetModelsRedisResult<TModel>(result);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(modelDef.CacheInstanceName, modelDef.Name, nameof(GetModelsAsync));

                InitLoadedLuas();

                return await GetModelsAsync<TModel>(keyName, keyValues, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogGetModelsError(modelDef.CacheInstanceName, modelDef.Name, keyName, keyValues, ex);

                AggregateException? aggregateException = null;

                try
                {
                    await RemoveModelsAsync<TModel>(keyName, keyValues, token).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex2)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    aggregateException = new AggregateException(ex, ex2);
                }

                throw (Exception?)aggregateException ?? CacheExceptions.GetModelsErrorButDeleted(modelDef.CacheInstanceName, modelDef.Name, keyName, keyValues, ex);
            }
        }

        /// <summary>
        /// //TODO:并不把models作为一个整体看待，里面有的可能会因为timestamp冲突而不成功。
        /// 需要改变吗？
        /// </summary>
        public async Task<IEnumerable<bool>> SetModelsAsync<TModel>(IEnumerable<TModel> models, CancellationToken token = default) where TModel : ITimestampModel, new()
        {
            ThrowIf.NullOrEmpty(models, nameof(models));

            CacheModelDef? modelDef = CacheModelDefFactory.Get<TModel>();

            if (modelDef == null)
            {
                throw CacheExceptions.NotEnabledForModel(typeof(TModel).FullName);
            }

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            AddSetModelsRedisInfo(models, modelDef, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(modelDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    GetLoadedLuas(modelDef.CacheInstanceName!).LoadedModelsSetLua,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                List<bool> rts = new List<bool>();

                RedisResult[] results = (RedisResult[])result!;

                for (int i = 0; i < results.Length; ++i)
                {
                    int rt = (int)results[i];

                    rts.Add(rt == 1);

                    if (rt == 8)
                    {
                        Logger.LogCacheSetTimestampConcurrency(modelDef.CacheInstanceName, modelDef.Name, "与已删除的缓存项留下的minTimestamp冲突", models.ElementAt(i));
                    }
                    else if (rt == 9)
                    {
                        Logger.LogCacheSetTimestampConcurrency(modelDef.CacheInstanceName, modelDef.Name, "与已存在的缓存项Timestamp冲突", models.ElementAt(i));
                    }
                }

                return rts;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(modelDef.CacheInstanceName, modelDef.Name, nameof(SetModelsAsync));

                InitLoadedLuas();

                return await SetModelsAsync<TModel>(models, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.SetModelsError(modelDef.CacheInstanceName, modelDef.Name, models, ex);
            }
        }

        public async Task RemoveModelsAsync<TModel>(string keyName, IEnumerable keyValues, CancellationToken token = default) //where TModel : ITimestampModel, new()
        {
            CacheModelDef? modelDef = CacheModelDefFactory.Get<TModel>();
            //ThrowIfNotCacheEnabled(modelDef);

            if (modelDef == null)
            {
                Logger.LogWarning("尝试Remove一个没有启动Cache功能的Model {ModelName}, Key {KeyName}", typeof(TModel).Name, keyName);
                return;
            }

            ThrowIf.Null(keyValues, nameof(keyValues));

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            byte[] loadedScript = AddRemoveModelsRedisInfo<TModel>(keyName, keyValues, modelDef, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(modelDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                await database.ScriptEvaluateAsync(loadedScript, redisKeys.ToArray(), redisValues.ToArray()).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(modelDef.CacheInstanceName, modelDef.Name, nameof(RemoveModelsAsync));

                InitLoadedLuas();

                await RemoveModelsAsync<TModel>(keyName, keyValues, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.RemoveModelsError(modelDef.CacheInstanceName, modelDef.Name, keyName, keyValues, ex);
            }
        }

        //private async Task ForcedRemoveModelsAsync<TModel>(string keyName, IEnumerable keyValues, CancellationToken token = default) where TModel : IModelCache, new()
        //{
        //    CacheModelDef modelDef = CacheModelDefFactory.Get<TModel>();
        //    ThrowIfNotCacheEnabled(modelDef);
        //    ThrowIf.Null(keyValues, nameof(keyValues));

        //    List<RedisKey> redisKeys = new List<RedisKey>();
        //    List<RedisValue> redisValues = new List<RedisValue>();

        //    byte[] loadedScript = AddForcedRemoveModelsRedisInfo<TModel>(keyName, keyValues, modelDef, redisKeys, redisValues);

        //    IDatabase database = await GetDatabaseAsync(modelDef.CacheInstanceName).ConfigureAwait(false);

        //    try
        //    {
        //        await database.ScriptEvaluateAsync(loadedScript, redisKeys.ToArray(), redisValues.ToArray()).ConfigureAwait(false);
        //    }
        //    catch (RedisServerException ex) when (ex.Description.StartsWith("NOSCRIPT", StringComparison.Ordinal))
        //    {
        //        Logger.LogLuaScriptNotLoaded(modelDef.CacheInstanceName, modelDef.Name, nameof(ForcedRemoveModelsAsync));

        //        InitLoadedLuas();

        //        await ForcedRemoveModelsAsync<TModel>(keyName, keyValues, token).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw CommonExceptions.ForcedRemoveModelsError(modelDef.CacheInstanceName, modelDef.Name, keyName, keyValues, ex);
        //    }
        //}

        private byte[] AddRemoveModelsRedisInfo<TModel>(string keyName, IEnumerable keyValues, CacheModelDef modelDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) //where TModel : FullStack.Common.Cache.CacheModels.ITimestampModel, new()
        {
            byte[] loadedScript;

            if (modelDef.KeyProperty.Name == keyName)
            {
                foreach (object dimensionKeyValue in keyValues)
                {
                    redisKeys.Add(GetRealKey(modelDef.Name, dimensionKeyValue.ToString()!));
                }

                loadedScript = GetLoadedLuas(modelDef.CacheInstanceName!).LoadedModelsRemoveLua;
            }
            else
            {
                foreach (object dimensionKeyValue in keyValues)
                {
                    redisKeys.Add(GetModelDimensionKey(modelDef.Name, keyName, dimensionKeyValue.ToString()!));
                }

                loadedScript = GetLoadedLuas(modelDef.CacheInstanceName!).LoadedModelsRemoveByDimensionLua;
            }

            /// argv: 3(model_count), invalidationKey_expire_seconds, updated_version_value1, updated_version_value2, updated_version_value3

            redisValues.Add(redisKeys.Count);
            redisValues.Add(MININAL_TIMESTAMP_LOCK_EXPIRY_SECONDS);

            //foreach (int updatedTimestamp in updatedTimestamps)
            //{
            //    redisValues.Add(updatedTimestamp);
            //}

            return loadedScript;
        }

        //private byte[] AddForcedRemoveModelsRedisInfo<TModel>(string keyName, IEnumerable keyValues, CacheModelDef modelDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TModel : IModelCache, new()
        //{
        //    byte[] loadedScript;

        //    if (modelDef.KeyProperty.Name == keyName)
        //    {
        //        foreach (object dimensionKeyValue in keyValues)
        //        {
        //            redisKeys.Add(GetRealKey(modelDef.Name, dimensionKeyValue.ToString()!));
        //        }

        //        loadedScript = GetLoadedLuas(modelDef.CacheInstanceName!).LoadedModelsForcedRemoveLua;
        //    }
        //    else
        //    {
        //        foreach (object dimensionKeyValue in keyValues)
        //        {
        //            redisKeys.Add(GetModelDimensionKey(modelDef.Name, keyName, dimensionKeyValue.ToString()!));
        //        }

        //        loadedScript = GetLoadedLuas(modelDef.CacheInstanceName!).LoadedModelsForcedRemoveByDimensionLua;
        //    }

        //    /// argv: 3(model_count)

        //    redisValues.Add(redisKeys.Count);

        //    return loadedScript;
        //}

        private byte[] AddGetModelsRedisInfo(string keyName, IEnumerable keyValues, CacheModelDef modelDef, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            byte[] loadedScript;

            if (modelDef.KeyProperty.Name == keyName)
            {
                loadedScript = GetLoadedLuas(modelDef.CacheInstanceName!).LoadedModelsGetAndRefreshLua;

                foreach (object dk in keyValues)
                {
                    redisKeys.Add(GetRealKey(modelDef.Name, dk.ToString()!));
                }
            }
            else
            {
                loadedScript = GetLoadedLuas(modelDef.CacheInstanceName!).LoadedModelsGetAndRefreshByDimensionLua;

                foreach (object dk in keyValues)
                {
                    redisKeys.Add(GetModelDimensionKey(modelDef.Name, keyName, dk.ToString()!));
                }
            }

            redisValues.Add(redisKeys.Count);
            redisValues.Add(modelDef.SlidingTime?.TotalSeconds ?? -1);
            redisValues.Add(TimeUtil.UtcNowUnixTimeSeconds);

            return loadedScript;
        }

        private void AddSetModelsRedisInfo<TModel>(IEnumerable<TModel> models, CacheModelDef modelDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TModel : ITimestampModel, new()
        {
            /// keys: model1_idKey, model1_dimensionkey1, model1_dimensionkey2, model1_dimensionkey3, model2_idKey, model2_dimensionkey1, model2_dimensionkey2, model2_dimensionkey3
            /// argv: absexp_value, expire_value,2(model_cout), 3(dimensionkey_count), model1_data, model1_timestamp, model1_dimensionKeyJoinedString, model2_data, model2_timestamp, model2_dimensionKeyJoinedString

            DateTimeOffset? absulteExpireTime = modelDef.AbsoluteTimeRelativeToNow != null ? TimeUtil.UtcNow + modelDef.AbsoluteTimeRelativeToNow : null;
            long? absoluteExpireUnixSeconds = absulteExpireTime?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(modelDef.SlidingTime?.TotalSeconds);
            long? expireSeconds = GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds);

            redisValues.Add(absoluteExpireUnixSeconds ?? -1);
            redisValues.Add(expireSeconds ?? -1);
            //TODO: 考虑设置过期时间的限制？最大过期时间？现在是如果不设置absoluteExpire和slideseconds，那么就一直不过期

            redisValues.Add(models.Count());
            redisValues.Add(modelDef.AltKeyProperties.Count);

            foreach (TModel model in models)
            {
                string idRealKey = GetRealKey(modelDef.Name, modelDef.KeyProperty.GetValue(model)?.ToString()!);

                redisKeys.Add(idRealKey);

                StringBuilder joinedDimensinKeyBuilder = new StringBuilder();

                foreach (PropertyInfo property in modelDef.AltKeyProperties)
                {
                    string dimentionKey = GetModelDimensionKey(modelDef.Name, property.Name, property.GetValue(model)?.ToString()!);
                    redisKeys.Add(dimentionKey);
                    joinedDimensinKeyBuilder.Append(dimentionKey);
                    joinedDimensinKeyBuilder.Append(' ');
                }

                if (joinedDimensinKeyBuilder.Length > 0)
                {
                    joinedDimensinKeyBuilder.Remove(joinedDimensinKeyBuilder.Length - 1, 1);
                }

                byte[] data = SerializeUtil.Serialize(model);

                redisValues.Add(data);
                redisValues.Add(model.Timestamp);
                redisValues.Add(joinedDimensinKeyBuilder.ToString());
            }
        }

        private static (IList<TModel>?, bool) MapGetModelsRedisResult<TModel>(RedisResult result) where TModel : ITimestampModel, new()
        {
            if (result.IsNull)
            {
                return (null, false);
            }

            RedisResult[]? results = (RedisResult[])result!;

            if (results == null || results.Length == 0)
            {
                return (null, false);
            }

            List<TModel> models = new List<TModel>();

            foreach (RedisResult item in results)
            {
                TModel? model = SerializeUtil.Deserialize<TModel>((byte[])item!);

                //因为lua中已经检查过全部存在，所以这里都不为null
                models.Add(model!);
            }

            return (models, true);
        }
    }
}
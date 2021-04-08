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
    /// Entity in Redis:
    /// 
    ///                                 |------ abexp    :  value
    /// Id------------------------------|------ slidexp  :  value        //same as IDistributed way
    ///                                 |------ data     :  jsonString
    /// 
    /// 
    ///                                 |------- DimensionKeyValue_1   :  Id
    /// EntityName_DimensionKeyName-----|......
    ///                                 |------- DimensionKeyValue_n   :  Id
    ///                                 
    /// 所以EntityName_DimensionKeyName 这个key是一个索引key
    /// </summary>
    internal partial class RedisCache : RedisCacheBase, ICache
    {
        /// <summary>
        /// keys:id1, id2, id3
        /// argv:3(id_number), sldexp, nowInUnixSeconds
        /// </summary>
        public const string LUA_ENTITIES_GET_AND_REFRESH = @"
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
        /// ARGV:3(entity_count), sldexp, nowInUnixSeconds
        /// </summary>
        public const string LUA_ENTITIES_GET_AND_REFRESH_BY_DIMENSION = @"

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

        /// <summary>
        /// 返回0为未更新，返回1为更新
        /// keys: entity1_idKey, entity1_dimensionkey1, entity1_dimensionkey2, entity1_dimensionkey3, entity2_idKey, entity2_dimensionkey1, entity2_dimensionkey2, entity2_dimensionkey3
        /// argv: absexp_value, expire_value,2(entity_cout), 3(dimensionkey_count), entity1_data, entity1_version, entity1_dimensionKeyJoinedString, entity2_data, entity2_version, entity2_dimensionKeyJoinedString
        /// </summary>
        public const string LUA_ENTITIES_SET = @"
local entityNum = tonumber(ARGV[3])
local dimNum = tonumber(ARGV[4])
local rt={}
for j=1, entityNum do
    rt[j]=0
    local keyIndex= 1 + (j-1) *(dimNum+1)

    local minVersion = redis.call('get', '_minV'..KEYS[keyIndex])

    if ((not minVersion) or tonumber(minVersion)<= tonumber(ARGV[6+(j-1)*3])) then

        local cached=redis.call('hget', KEYS[keyIndex], 'version')
        if((not cached) or tonumber(cached)< tonumber(ARGV[6+(j-1) * 3])) then

            redis.call('hmset', KEYS[keyIndex],'absexp',ARGV[1],'data',ARGV[5+(j-1)*3], 'version', ARGV[6+(j-1)*3], 'dim', ARGV[7+(j-1)*3]) 

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

        /// <summary>
        /// keys: idKey1, idKey2, idKey3
        /// argv: 3(entity_num), invalidationKey_expire_seconds, updated_version_value1, updated_version_value2, updated_version_value3
        /// </summary>
        public const string LUA_ENTITIES_REMOVE = @" 
local entityNum = tonumber(ARGV[1])
for j=1, entityNum do

    redis.call('set', '_minV'..KEYS[j], ARGV[j+2], 'EX', ARGV[2])

    local data=redis.call('hget', KEYS[j], 'dim')

    redis.call('del', KEYS[j]) 

    if(data and data~='') then
        for i in string.gmatch(data, '%S+') do
            redis.call('del', i)
        end
    end
end
";

        /// <summary>
        /// keys:entity1_dimensionkey, entity2_dimensionkey, entity3_dimensionKey
        /// argv: 3(entity_count), invalidationKey_expire_seconds, updated_version_value1, updated_version_value2, updated_version_value3
        /// </summary>
        public const string LUA_ENTITIES_REMOVE_BY_DIMENSION = @"
local entityNum = tonumber(ARGV[1])

for j = 1, entityNum do
    local id = redis.call('get',KEYS[j])

    if (id) then

        redis.call('set', '_minV'..id, ARGV[j+2], 'EX', ARGV[2])

        local data= redis.call('hget',id, 'dim') 

        if (not data) then
            redis.call('del', KEYS[1])
        else
            redis.call('del',id)
    
            if (data~='') then
                for i in string.gmatch(data, '%S+') do
                    redis.call('del', i) 
                end
            end
        end
    end
end
";
        /// <summary>
        /// keys:entity1_dimensionkey, entity2_dimensionkey, entity3_dimensionKey
        /// argv: 3(entity_count)
        /// </summary>
        public const string LUA_ENTITIES_REMOVE_BY_DIMENSION_FORCED_NO_VERSION = @"
local entityNum = tonumber(ARGV[1])

for j = 1, entityNum do
    local id = redis.call('get',KEYS[j])

    if (id) then

       

        local data= redis.call('hget',id, 'dim') 

        if (not data) then
            redis.call('del', KEYS[1])
        else
            redis.call('del',id)
    
            if (data~='') then
                for i in string.gmatch(data, '%S+') do
                    redis.call('del', i) 
                end
            end
        end
    end
end
";
        /// <summary>
        /// keys: idKey1, idKey2, idKey3
        /// argv: 3(entity_num)
        /// </summary>
        public const string LUA_ENTITIES_REMOVE_FORECED_NO_VERSION = @" 
local entityNum = tonumber(ARGV[1])
for j=1, entityNum do

    

    local data=redis.call('hget', KEYS[j], 'dim')

    redis.call('del', KEYS[j]) 

    if(data and data~='') then
        for i in string.gmatch(data, '%S+') do
            redis.call('del', i)
        end
    end
end
";

        public RedisCache(IOptions<RedisCacheOptions> options, ILogger<RedisCache> logger) : base(options, logger)
        {
            _logger.LogInformation($"RedisCache初始化完成");
        }

        /// <summary>
        /// GetEntitiesAsync
        /// </summary>
        /// <param name="dimensionKeyName"></param>
        /// <param name="dimensionKeyValues"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public async Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIf.Null(dimensionKeyValues, nameof(dimensionKeyValues));
            ThrowIfNotCacheEnabled(entityDef);

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            byte[] loadedScript = AddGetEntitiesRedisInfo(dimensionKeyName, dimensionKeyValues, entityDef, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    loadedScript,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                return await MapGetEntitiesRedisResultAsync<TEntity>(result).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await GetEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析这个GetEntitiesAsync.情况1，程序中实体改了");

                try
                {
                    await ForcedRemoveEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token).ConfigureAwait(false);
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "在强制删除中出错，{TEntity}, dimKey:{dimensionKeyname} ", typeof(TEntity).Name, dimensionKeyName);
                }

                throw Exceptions.UnkownButDeleted(cause: "缓存中取值时，未知错误, 删除此项缓存", innerException: ex);
            }
        }

        /// <summary>
        /// SetEntitiesAsync
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public async Task<IEnumerable<bool>> SetEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIfNotCacheEnabled(entityDef);
            ThrowIf.NullOrEmpty(entities, nameof(entities));


            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            await AddSetEntitiesRedisInfoAsync(entities, entityDef, redisKeys, redisValues).ConfigureAwait(false);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesSetLua,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                List<bool> rts = new List<bool>();

                RedisResult[] results = (RedisResult[])result;

                for (int i = 0; i < results.Length; ++i)
                {
                    int rt = (int)results[i];

                    rts.Add(rt == 1);

                    if (rt == 8)
                    {
                        _logger.LogWarning("检测到，Cache Invalidation Concurrency冲突，已被阻止. {Entity}, {Id}", entityDef.Name, SerializeUtil.ToJson(entities.ElementAt(i)));
                    }
                    else if (rt == 9)
                    {
                        _logger.LogWarning("检测到，Cache Update Concurrency冲突，已被阻止. {Entity}, {Id}", entityDef.Name, SerializeUtil.ToJson(entities.ElementAt(i)));
                    }
                }

                return rts;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await SetEntitiesAsync<TEntity>(entities, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析这个");

                throw Exceptions.Unkown(redisKeys, redisValues, ex);
            }
        }

        /// <summary>
        /// RemoveEntitiesAsync
        /// </summary>
        /// <param name="dimensionKeyName"></param>
        /// <param name="dimensionKeyValues"></param>
        /// <param name="updatedVersions"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        public async Task RemoveEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, IEnumerable<int> updatedVersions, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            ThrowIfNotCacheEnabled(entityDef);
            ThrowIf.Null(dimensionKeyValues, nameof(dimensionKeyValues));

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            byte[] loadedScript = AddRemoveEntitiesRedisInfo<TEntity>(dimensionKeyName, dimensionKeyValues, updatedVersions, entityDef, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                await database.ScriptEvaluateAsync(loadedScript, redisKeys.ToArray(), redisValues.ToArray()).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await RemoveEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, updatedVersions, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析这个RemoveEntitiesAsync");

                throw Exceptions.Unkown(redisKeys, redisValues, ex);
            }
        }

        private async Task ForcedRemoveEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            ThrowIfNotCacheEnabled(entityDef);
            ThrowIf.Null(dimensionKeyValues, nameof(dimensionKeyValues));

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            byte[] loadedScript = AddForcedRemoveEntitiesRedisInfo<TEntity>(dimensionKeyName, dimensionKeyValues, entityDef, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                await database.ScriptEvaluateAsync(loadedScript, redisKeys.ToArray(), redisValues.ToArray()).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await ForcedRemoveEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析这个ForcedRemoveEntitiesAsync");

                throw Exceptions.Unkown(redisKeys, redisValues, ex);
            }
        }

        /// <summary>
        /// AddRemoveEntitiesRedisInfo
        /// </summary>
        /// <param name="dimensionKeyName"></param>
        /// <param name="dimensionKeyValues"></param>
        /// <param name="updatedVersions"></param>
        /// <param name="entityDef"></param>
        /// <param name="redisKeys"></param>
        /// <param name="redisValues"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        private byte[] AddRemoveEntitiesRedisInfo<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, IEnumerable<int> updatedVersions, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
        {
            byte[] loadedScript;

            if (entityDef.KeyProperty.Name == dimensionKeyName)
            {
                foreach (object dimensionKeyValue in dimensionKeyValues)
                {
                    redisKeys.Add(GetRealKey(entityDef.Name, dimensionKeyValue.ToString()!));
                }

                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesRemoveLua;
            }
            else
            {
                foreach (object dimensionKeyValue in dimensionKeyValues)
                {
                    redisKeys.Add(GetEntityDimensionKey(entityDef.Name, dimensionKeyName, dimensionKeyValue.ToString()));
                }

                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesRemoveByDimensionLua;
            }

            /// argv: 3(entity_count), invalidationKey_expire_seconds, updated_version_value1, updated_version_value2, updated_version_value3

            redisValues.Add(redisKeys.Count);
            redisValues.Add(_invalidationVersionExpirySeconds);

            foreach (int updatedVersion in updatedVersions)
            {
                redisValues.Add(updatedVersion);
            }

            return loadedScript;
        }

        private byte[] AddForcedRemoveEntitiesRedisInfo<TEntity>(string dimensionKeyName, IEnumerable dimensionKeyValues, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
        {
            byte[] loadedScript;

            if (entityDef.KeyProperty.Name == dimensionKeyName)
            {
                foreach (object dimensionKeyValue in dimensionKeyValues)
                {
                    redisKeys.Add(GetRealKey(entityDef.Name, dimensionKeyValue.ToString()!));
                }

                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesForcedRemoveLua;
            }
            else
            {
                foreach (object dimensionKeyValue in dimensionKeyValues)
                {
                    redisKeys.Add(GetEntityDimensionKey(entityDef.Name, dimensionKeyName, dimensionKeyValue.ToString()));
                }

                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesForcedRemoveByDimensionLua;
            }

            /// argv: 3(entity_count)

            redisValues.Add(redisKeys.Count);

            return loadedScript;
        }


        /// <summary>
        /// AddGetEntitiesRedisInfo
        /// </summary>
        /// <param name="dimensionKeyName"></param>
        /// <param name="dimensionKeyValues"></param>
        /// <param name="entityDef"></param>
        /// <param name="redisKeys"></param>
        /// <param name="redisValues"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        private byte[] AddGetEntitiesRedisInfo(string dimensionKeyName, IEnumerable dimensionKeyValues, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            byte[] loadedScript;

            if (entityDef.KeyProperty.Name == dimensionKeyName)
            {
                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesGetAndRefreshLua;

                foreach (object dk in dimensionKeyValues)
                {
                    redisKeys.Add(GetRealKey(entityDef.Name, dk.ToString()!));
                }
            }
            else
            {
                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesGetAndRefreshByDimensionLua;

                foreach (object dk in dimensionKeyValues)
                {
                    redisKeys.Add(GetEntityDimensionKey(entityDef.Name, dimensionKeyName, dk.ToString()!));
                }
            }

            redisValues.Add(redisKeys.Count);
            redisValues.Add(entityDef.SlidingTime?.TotalSeconds ?? -1);
            redisValues.Add(TimeUtil.UtcNowUnixTimeSeconds);

            return loadedScript;
        }

        private async Task AddSetEntitiesRedisInfoAsync<TEntity>(IEnumerable<TEntity> entities, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
        {
            /// keys: entity1_idKey, entity1_dimensionkey1, entity1_dimensionkey2, entity1_dimensionkey3, entity2_idKey, entity2_dimensionkey1, entity2_dimensionkey2, entity2_dimensionkey3
            /// argv: absexp_value, expire_value,2(entity_cout), 3(dimensionkey_count), entity1_data, entity1_version, entity1_dimensionKeyJoinedString, entity2_data, entity2_version, entity2_dimensionKeyJoinedString

            DateTimeOffset? absulteExpireTime = entityDef.AbsoluteTimeRelativeToNow != null ? TimeUtil.UtcNow + entityDef.AbsoluteTimeRelativeToNow : null;
            long? absoluteExpireUnixSeconds = absulteExpireTime?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(entityDef.SlidingTime?.TotalSeconds);
            long? expireSeconds = GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds);

            redisValues.Add(absoluteExpireUnixSeconds ?? -1);
            redisValues.Add(expireSeconds ?? -1);
            redisValues.Add(entities.Count());
            redisValues.Add(entityDef.Dimensions.Count);

            foreach (TEntity entity in entities)
            {
                string idRealKey = GetRealKey(entityDef.Name, entityDef.KeyProperty.GetValue(entity)?.ToString()!);

                redisKeys.Add(idRealKey);

                StringBuilder joinedDimensinKeyBuilder = new StringBuilder();

                foreach (PropertyInfo property in entityDef.Dimensions)
                {
                    string dimentionKey = GetEntityDimensionKey(entityDef.Name, property.Name, property.GetValue(entity)?.ToString()!);
                    redisKeys.Add(dimentionKey);
                    joinedDimensinKeyBuilder.Append(dimentionKey);
                    joinedDimensinKeyBuilder.Append(' ');
                }

                if (joinedDimensinKeyBuilder.Length > 0)
                {
                    joinedDimensinKeyBuilder.Remove(joinedDimensinKeyBuilder.Length - 1, 1);
                }

                byte[] data = await SerializeUtil.PackAsync(entity).ConfigureAwait(false);

                redisValues.Add(data);
                redisValues.Add(entity.Version);
                redisValues.Add(joinedDimensinKeyBuilder.ToString());
            }
        }

        private static async Task<(IEnumerable<TEntity>?, bool)> MapGetEntitiesRedisResultAsync<TEntity>(RedisResult result) where TEntity : Entity, new()
        {
            if (result.IsNull)
            {
                return (null, false);
            }

            RedisResult[]? results = (RedisResult[])result;

            if (results == null || results.Length == 0)
            {
                return (null, false);
            }

            List<TEntity> entities = new List<TEntity>();

            foreach (RedisResult item in results)
            {
                TEntity? entity = await SerializeUtil.UnPackAsync<TEntity>((byte[])item).ConfigureAwait(false);

                //因为lua中已经检查过全部存在，所以这里都不为null
                entities.Add(entity!);
            }

            return (entities, true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common.Entities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Cache
{
    internal partial class RedisCache : RedisCacheBase, ICache
    {
        /// <summary>
        /// keys:guid1, guid2, guid3
        /// argv:3(guid_number), sldexp, nowInUnixSeconds
        /// </summary>
        public const string _luaEntitiesGetAndRefresh = @"
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
                for i in string.gmatch(data[3], '%w+') do
                   redis.call('del', i) 
                end
            end
            return nil 
        end
    end

    if(ARGV[2]~='-1') then
        redis.call('expire', KEYS[j], ARGV[2])
    
        if (data[3]~='') then
            for k in string.gmatch(data[3], '%w+') do
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
        public const string _luaEntitiesGetAndRefreshByDimension = @"

local number = tonumber(ARGV[1])
local existCount = redis.call('exists', unpack(KEYS))
if (existCount ~= number) then
    return nil
end

local array={}

for j =1,number do
    local guid = redis.call('get',KEYS[j])

    local data= redis.call('hmget',guid, 'absexp','data','dim') 

    if (not data[1]) then
        redis.call('del', KEYS[j])
        return nil
    end

    if(data[1]~='-1') then
        local now = tonumber(ARGV[3])
        local absexp = tonumber(data[1])
        if(now>=absexp) then
            redis.call('del',guid)
    
            if (data[3]~='') then
                for i in string.gmatch(data[3], '%w+') do
                   redis.call('del', i) 
                end
            end
            return nil 
        end
    end

    if(ARGV[2]~='-1') then
        redis.call('expire', guid, ARGV[2])
    
        if (data[3]~='') then
            for k in string.gmatch(data[3], '%w+') do
               redis.call('expire', k, ARGV[2]) 
            end
        end
    end

    array[j]= data[2]
end
return array";

        /// <summary>
        /// 返回0为未更新，返回1为更新
        /// keys: entity1_guidKey, entity1_dimensionkey1, entity1_dimensionkey2, entity1_dimensionkey3, entity2_guidKey, entity2_dimensionkey1, entity2_dimensionkey2, entity2_dimensionkey3
        /// argv: absexp_value, expire_value,2(entity_cout), 3(dimensionkey_count), entity1_data, entity1_version, entity1_dimensionKeyJoinedString, entity2_data, entity2_version, entity2_dimensionKeyJoinedString
        /// </summary>
        public const string _luaEntitiesSet = @"
local entityNum = tonumber(ARGV[3])
local dimNum = tonumber(ARGV[4])
local rt={}
for j=1, entityNum do
    rt[j]=0
    local keyIndex= 1 + (j-1) *(dimNum+1)
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
    end
end
return rt";

        /// <summary>
        /// keys: guidKey1, guidKey2, guidKey3
        /// argv: 3(entity_num)
        /// </summary>
        public const string _luaEntitiesRemove = @" 
local entityNum = tonumber(ARGV[1])
for j=1, entityNum do

    local data=redis.call('hget', KEYS[j], 'dim')

    redis.call('del', KEYS[j]) 

    if(data and data~='') then
        for i in string.gmatch(data, '%w+') do
            redis.call('del', i)
        end
    end
end
";

        /// <summary>
        /// keys:entity1_dimensionkey, entity2_dimensionkey, entity3_dimensionKey
        /// argv: 3(entity_count)
        /// </summary>
        public const string _luaEntitiesRemoveByDimension = @"
local entityNum = tonumber(ARGV[1])

for j = 1, entityNum do
    local guid = redis.call('get',KEYS[j])

    if (guid) then

        local data= redis.call('hget',guid, 'dim') 

        if (not data) then
            redis.call('del', KEYS[1])
        else
            redis.call('del',guid)
    
            if (data~='') then
                for i in string.gmatch(data, '%w+') do
                    redis.call('del', i) 
                end
            end
        end
    end
end
";

        private readonly ILogger<RedisCache> _logger;

        public RedisCache(IOptions<RedisCacheOptions> options, ILogger<RedisCache> logger) : base(options)
        {
            _logger = logger;
        }

        public async Task<(IEnumerable<TEntity>?, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIf.NullOrEmpty(dimensionKeyValues, nameof(dimensionKeyValues));
            ThrowIfNotCacheEnabled(entityDef);
            ThrowIfNotBactchEnabled(entityDef);

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
        }

        public async Task<IEnumerable<bool>> SetEntitiesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIfNotCacheEnabled(entityDef);
            ThrowIfNotBactchEnabled(entityDef);
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

                foreach (RedisResult item in results)
                {
                    rts.Add((int)item == 1);
                }

                return rts;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await SetEntitiesAsync<TEntity>(entities, token).ConfigureAwait(false);
            }
        }


        public async Task RemoveEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            ThrowIfNotCacheEnabled(entityDef);
            ThrowIfNotBactchEnabled(entityDef);
            ThrowIf.NullOrEmpty(dimensionKeyValues, nameof(dimensionKeyValues));

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            byte[] loadedScript = AddRemoveEntitiesRedisInfo<TEntity>(dimensionKeyName, dimensionKeyValues, entityDef, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                await database.ScriptEvaluateAsync(loadedScript, redisKeys.ToArray(), redisValues.ToArray()).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await RemoveEntitiesAsync<TEntity>(dimensionKeyName, dimensionKeyValues, token).ConfigureAwait(false);
            }

        }

        private byte[] AddRemoveEntitiesRedisInfo<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
        {
            byte[] loadedScript;

            if (entityDef.GuidKeyProperty.Name == dimensionKeyName)
            {
                foreach (string dimensionKeyValue in dimensionKeyValues)
                {
                    redisKeys.Add(GetRealKey(dimensionKeyValue));
                }

                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesRemoveLua;
            }
            else
            {
                foreach (string dimensionKeyValue in dimensionKeyValues)
                {
                    redisKeys.Add(GetEntityDimensionKey(entityDef.Name, dimensionKeyName, dimensionKeyValue));
                }

                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesRemoveByDimensionLua;
            }

            redisValues.Add(dimensionKeyValues.Count());

            return loadedScript;
        }


        private byte[] AddGetEntitiesRedisInfo(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            byte[] loadedScript;

            if (entityDef.GuidKeyProperty.Name == dimensionKeyName)
            {
                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesGetAndRefreshLua;

                foreach (string dk in dimensionKeyValues)
                {
                    redisKeys.Add(GetRealKey(dk));
                }
            }
            else
            {
                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitiesGetAndRefreshByDimensionLua;

                foreach (string dk in dimensionKeyValues)
                {
                    redisKeys.Add(GetEntityDimensionKey(entityDef.Name, dimensionKeyName, dk));
                }
            }

            redisValues.Add(redisKeys.Count);
            redisValues.Add(entityDef.SlidingTime?.TotalSeconds ?? -1);
            redisValues.Add(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            return loadedScript;
        }
        private async Task AddSetEntitiesRedisInfoAsync<TEntity>(IEnumerable<TEntity> entities, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
        {
            /// keys: entity1_guidKey, entity1_dimensionkey1, entity1_dimensionkey2, entity1_dimensionkey3, entity2_guidKey, entity2_dimensionkey1, entity2_dimensionkey2, entity2_dimensionkey3
            /// argv: absexp_value, expire_value,2(entity_cout), 3(dimensionkey_count), entity1_data, entity1_version, entity1_dimensionKeyJoinedString, entity2_data, entity2_version, entity2_dimensionKeyJoinedString

            DateTimeOffset? absulteExpireTime = entityDef.AbsoluteTimeRelativeToNow != null ? DateTimeOffset.UtcNow + entityDef.AbsoluteTimeRelativeToNow : null;
            long? absoluteExpireUnixSeconds = absulteExpireTime?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(entityDef.SlidingTime?.TotalSeconds);
            long? expireSeconds = GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds);

            redisValues.Add(absoluteExpireUnixSeconds ?? -1);
            redisValues.Add(expireSeconds ?? -1);
            redisValues.Add(entities.Count());
            redisValues.Add(entityDef.Dimensions.Count);

            foreach (TEntity entity in entities)
            {
                string guidRealKey = GetRealKey(entityDef.GuidKeyProperty.GetValue(entity).ToString());

                redisKeys.Add(guidRealKey);

                StringBuilder joinedDimensinKeyBuilder = new StringBuilder();

                foreach (PropertyInfo property in entityDef.Dimensions)
                {
                    string dimentionKey = GetEntityDimensionKey(entityDef.Name, property.Name, property.GetValue(entity).ToString());
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

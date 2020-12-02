using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common.Entities;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Cache
{
    /// <summary>
    /// Entity in Redis:
    /// 
    ///                                 |------ abexp    :  value
    /// Guid----------------------------|------ slidexp  :  value        //same as IDistributed way
    ///                                 |------ data     :  jsonString
    /// 
    /// 
    ///                                 |------- DimensionKeyValue_1   :  Guid
    /// EntityName_DimensionKeyName-----|......
    ///                                 |------- DimensionKeyValue_n   :  Guid
    ///                                 
    /// 所以EntityName_DimensionKeyName 这个key是一个索引key
    /// </summary>
    //    internal partial class RedisCache
    //    {



    //        /// <summary>
    //        /// keys:guidkey
    //        /// argv:sldexp,nowInUnixSeconds
    //        /// </summary>
    //        public const string _luaEntityGetAndRefresh = @"
    //local data = redis.call('hmget', KEYS[1], 'absexp','data','dim')
    //-- 无数据直接返回
    //if (not data[1]) then
    //    return nil
    //end
    //-- 删除absexp过期的
    //if(data[1]~='-1') then
    //    local now = tonumber(ARGV[2])
    //    local absexp = tonumber(data[1])
    //    if(now>=absexp) then
    //        redis.call('del',KEYS[1])

    //        if (data[3]~='') then
    //            for i in string.gmatch(data[3], '%w+') do
    //               redis.call('del', i) 
    //            end
    //        end
    //        return nil 
    //    end
    //end
    //-- 更新expire
    //if(ARGV[1]~='-1') then
    //    redis.call('expire', KEYS[1], ARGV[1])

    //    if (data[3]~='') then
    //        for j in string.gmatch(data[3], '%w+') do
    //           redis.call('expire', j, ARGV[1]) 
    //        end
    //    end
    //end

    //return data[2]";

    //        /// <summary>
    //        /// KEYS:dimensionKey
    //        /// ARGV:sldexp, nowInUnixSeconds
    //        /// </summary>
    //        public const string _luaEntityGetAndRefreshByDimension = @"
    //local guid = redis.call('get',KEYS[1])

    //if (not guid) then
    //    return nil
    //end

    //local data= redis.call('hmget',guid, 'absexp','data','dim') 

    //if (not data[1]) then
    //    redis.call('del', KEYS[1])
    //    return nil
    //end

    //if(data[1]~='-1') then
    //    local now = tonumber(ARGV[2])
    //    local absexp = tonumber(data[1])
    //    if(now>=absexp) then
    //        redis.call('del',guid)

    //        if (data[3]~='') then
    //            for i in string.gmatch(data[3], '%w+') do
    //               redis.call('del', i) 
    //            end
    //        end
    //        return nil 
    //    end
    //end

    //if(ARGV[1]~='-1') then
    //    redis.call('expire', guid, ARGV[1])

    //    if (data[3]~='') then
    //        for j in string.gmatch(data[3], '%w+') do
    //           redis.call('expire', j, ARGV[1]) 
    //        end
    //    end
    //end

    //return data[2]";

    //        /// <summary>
    //        /// 说 存在 且 cached version 大于等于 new version，就不更新.返回9. 
    //        /// _minVersion 大于new version，不更新，返回8
    //        /// 成功返回1
    //        /// keys: guidKey, dimensionkey1, dimensionkey2, dimensionkey3
    //        /// argv: absexp_value, expire_value, data_value,version_value, dimensionKeyJoinedString, 3(dimensionkey_count)
    //        /// </summary>
    //        public const string _luaEntitySet = @"
    //local minVersion = redis.call('get', '_minV'+KEYS[1])
    //if (minVersion and tonumber(minVersion) > tonumber(ARGV[4])) then
    //    return 8
    //end

    //local cached=redis.call('hget', KEYS[1], 'version')
    //if(cached and tonumber(cached) >= tonumber(ARGV[4])) then
    //    return 9    
    //end

    //redis.call('hmset', KEYS[1],'absexp',ARGV[1],'data',ARGV[3], 'version', ARGV[4], 'dim', ARGV[5]) 

    //if(ARGV[2]~='-1') then 
    //    redis.call('expire',KEYS[1], ARGV[2]) 
    //end 

    //for i=2, ARGV[6]+1 do
    //    redis.call('set', KEYS[i], KEYS[1])
    //    if (ARGV[2]~='-1') then
    //        redis.call('expire', KEYS[i], ARGV[2])
    //    end
    //end

    //return 1";

    //        /// <summary>
    //        /// keys: guidKey
    //        /// argv: updated_version_value, invalidationKey_expire_seconds
    //        /// </summary>
    //        public const string _luaEntityRemove = @" 
    //redis.call('set', '_minV'+ KEYS[1], ARGV[1], 'EX', ARGV[2])

    //local dim=redis.call('hget', KEYS[1], 'dim')

    //redis.call('del', KEYS[1]) 

    //if (not dim) then
    //    return 1
    //end

    //if(dim~='') then
    //    for i in string.gmatch(dim, '%w+') do
    //        redis.call('del', i)
    //    end
    //end
    //return 1
    //";

    //        /// <summary>
    //        /// keys:dimensionKey
    //        /// argv:updated_version_value, invalidationKey_expire_seconds
    //        /// </summary>
    //        public const string _luaEntityRemoveByDimension = @"
    //local guid = redis.call('get',KEYS[1])

    //if (not guid) then
    //    return 8
    //end

    //redis.call('set', '_minV' + guid, ARGV[1], 'EX', ARGV[2])

    //local dim= redis.call('hget',guid, 'dim') 

    //if (not dim) then
    //    redis.call('del', KEYS[1])
    //    return 1
    //end

    //redis.call('del',guid)

    //if (dim~='') then
    //    for i in string.gmatch(dim, '%w+') do
    //        redis.call('del', i) 
    //    end
    //end
    //return 1";

    //        public async Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
    //        {
    //            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

    //            ThrowIfNotCacheEnabled(entityDef);

    //            List<RedisKey> redisKeys = new List<RedisKey>();
    //            List<RedisValue> redisValues = new List<RedisValue>();

    //            byte[] loadedScript = AddGetEntityRedisInfo<TEntity>(dimensionKeyName, dimensionKeyValue, entityDef, redisKeys, redisValues);

    //            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

    //            try
    //            {
    //                RedisResult result = await database.ScriptEvaluateAsync(
    //                    loadedScript,
    //                    redisKeys.ToArray(),
    //                    redisValues.ToArray()).ConfigureAwait(false);

    //                return await MapGetEntityRedisResultAsync<TEntity>(result).ConfigureAwait(false);
    //            }
    //            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
    //            {
    //                _logger.LogError(ex, "NOSCRIPT, will try again.");

    //                InitLoadedLuas();

    //                return await GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token).ConfigureAwait(false);
    //            }
    //        }

    //        public async Task<bool> SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
    //        {
    //            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

    //            ThrowIfNotCacheEnabled(entityDef);

    //            List<RedisKey> redisKeys = new List<RedisKey>();
    //            List<RedisValue> redisValues = new List<RedisValue>();

    //            await AddSetEntityRedisInfoAsync(entity, entityDef, redisKeys, redisValues).ConfigureAwait(false);

    //            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

    //            try
    //            {
    //                RedisResult result = await database.ScriptEvaluateAsync(
    //                    GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitySetLua,
    //                    redisKeys.ToArray(),
    //                    redisValues.ToArray()).ConfigureAwait(false);

    //                int rt = (int)result;

    //                if (rt == 1)
    //                {
    //                    return true;
    //                }
    //                else if (rt == 8)
    //                {
    //                    _logger.LogWarning($"检测到，Cache Invalidation Concurrency冲突，已被阻止. Entity:{entityDef.Name}, Guid:{entity.Guid}");
    //                }
    //                else if (rt == 9)
    //                {
    //                    _logger.LogWarning($"检测到，Cache Update Concurrency冲突，已被阻止. Entity:{entityDef.Name}, Guid:{entity.Guid}");
    //                }

    //                return false;
    //            }
    //            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
    //            {
    //                _logger.LogError(ex, "NOSCRIPT, will try again.");

    //                InitLoadedLuas();

    //                return await SetEntityAsync<TEntity>(entity, token).ConfigureAwait(false);
    //            }
    //        }

    //        public async Task RemoveEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, int updatedVersion, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
    //        {
    //            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

    //            ThrowIfNotCacheEnabled(entityDef);

    //            List<RedisKey> redisKeys = new List<RedisKey>();
    //            List<RedisValue> redisValues = new List<RedisValue>();

    //            byte[] loadedScript = AddRemoveEntityRedisInfo<TEntity>(dimensionKeyName, dimensionKeyValue, updatedVersion, entityDef, redisKeys, redisValues);

    //            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

    //            try
    //            {
    //                RedisResult result = await database.ScriptEvaluateAsync(
    //                    loadedScript,
    //                    redisKeys.ToArray(),
    //                    redisValues.ToArray()).ConfigureAwait(false);

    //                if ((int)result != 1)
    //                {
    //                    _logger.LogWarning($"Cache Remove NotFount. Entity:{nameof(TEntity)}, DimensionKeyName:{dimensionKeyName}, DimensionKeyValue:{dimensionKeyValue}");
    //                }
    //            }
    //            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
    //            {
    //                _logger.LogError(ex, "NOSCRIPT, will try again.");

    //                InitLoadedLuas();

    //                await RemoveEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, updatedVersion, token).ConfigureAwait(false);
    //            }
    //        }

    //        private byte[] AddGetEntityRedisInfo<TEntity>(string dimensionKeyName, string dimensionKeyValue, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
    //        {
    //            byte[] loadedScript;

    //            if (entityDef.GuidKeyProperty.Name == dimensionKeyName)
    //            {
    //                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityGetAndRefreshLua;
    //                redisKeys.Add(GetRealKey(dimensionKeyValue));
    //            }
    //            else
    //            {
    //                ThrowIfNotADimensionKeyName(dimensionKeyName, entityDef);

    //                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityGetAndRefreshByDimensionLua;
    //                redisKeys.Add(GetEntityDimensionKey(entityDef.Name, dimensionKeyName, dimensionKeyValue));
    //            }

    //            redisValues.Add(entityDef.SlidingTime?.TotalSeconds ?? -1);
    //            redisValues.Add(TimeUtil.UtcNowUnixTimeSeconds);

    //            return loadedScript;
    //        }

    //        private async Task AddSetEntityRedisInfoAsync<TEntity>(TEntity entity, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
    //        {
    //            /// keys: guidKey, dimensionkey1, dimensionkey2, dimensionkey3
    //            /// argv: absexp_value, expire_value, data_value,version_value, dimensionKeyJoinedString, 3(dimensionkey_count)


    //            string guidRealKey = GetRealKey(entityDef.GuidKeyProperty.GetValue(entity).ToString());

    //            redisKeys.Add(guidRealKey);

    //            StringBuilder joinedDimensinKeyBuilder = new StringBuilder();

    //            foreach (PropertyInfo property in entityDef.Dimensions)
    //            {
    //                string dimentionKey = GetEntityDimensionKey(entityDef.Name, property.Name, property.GetValue(entity).ToString());
    //                redisKeys.Add(dimentionKey);
    //                joinedDimensinKeyBuilder.Append(dimentionKey);
    //                joinedDimensinKeyBuilder.Append(' ');
    //            }

    //            if (joinedDimensinKeyBuilder.Length > 0)
    //            {
    //                joinedDimensinKeyBuilder.Remove(joinedDimensinKeyBuilder.Length - 1, 1);
    //            }

    //            DateTimeOffset? absulteExpireTime = entityDef.AbsoluteTimeRelativeToNow != null ? TimeUtil.UtcNow + entityDef.AbsoluteTimeRelativeToNow : null;
    //            long? absoluteExpireUnixSeconds = absulteExpireTime?.ToUnixTimeSeconds();
    //            long? slideSeconds = (long?)(entityDef.SlidingTime?.TotalSeconds);
    //            long? expireSeconds = GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds);

    //            byte[] data = await SerializeUtil.PackAsync(entity).ConfigureAwait(false);


    //            redisValues.Add(absoluteExpireUnixSeconds ?? -1);
    //            redisValues.Add(expireSeconds ?? -1);
    //            redisValues.Add(data);
    //            redisValues.Add(entity.Version);
    //            redisValues.Add(joinedDimensinKeyBuilder.ToString());
    //            redisValues.Add(entityDef.Dimensions.Count);
    //        }

    //        private byte[] AddRemoveEntityRedisInfo<TEntity>(string dimensionKeyName, string dimensionKeyValue, int updatedVersion, CacheEntityDef entityDef, List<RedisKey> redisKeys, List<RedisValue> redisValues) where TEntity : Entity, new()
    //        {
    //            byte[] loadedScript;

    //            if (entityDef.GuidKeyProperty.Name == dimensionKeyName)
    //            {
    //                redisKeys.Add(GetRealKey(dimensionKeyValue));
    //                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityRemoveLua;
    //            }
    //            else
    //            {
    //                redisKeys.Add(GetEntityDimensionKey(entityDef.Name, dimensionKeyName, dimensionKeyValue));
    //                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityRemoveByDimensionLua;
    //            }

    //            /// argv:updated_version_value, invalidationKey_expire_seconds

    //            redisValues.Add(updatedVersion);
    //            redisValues.Add(_invalidationVersionExpirySeconds);

    //            return loadedScript;
    //        }

    //        private static async Task<(TEntity?, bool)> MapGetEntityRedisResultAsync<TEntity>(RedisResult result) where TEntity : Entity, new()
    //        {
    //            if (result.IsNull)
    //            {
    //                return (null, false);
    //            }

    //            TEntity? entity = await SerializeUtil.UnPackAsync<TEntity>((byte[])result).ConfigureAwait(false);

    //            return (entity, true);
    //        }
    //    }
}

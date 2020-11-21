using HB.Framework.Common.Cache;
using HB.Framework.Common.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.Cache
{
    internal partial class RedisCache : ICache
    {
        private readonly ILogger _logger;
        private readonly RedisCacheOptions _options;
        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;
        private readonly IDictionary<string, LoadedLuas> _loadedLuaDict = new Dictionary<string, LoadedLuas>();

        public string DefaultInstanceName => _options.DefaultInstanceName ?? _options.ConnectionSettings[0].InstanceName;

        public RedisCache(IOptions<RedisCacheOptions> options, ILogger<RedisCache> logger)
        {
            _logger = logger;
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);

            InitLoadedLuas();
        }

        #region privates

        private string GetRealKey(string key)
        {
            return _options.ApplicationName + key;
        }

        /// <summary>
        /// 各服务器反复Load也没有关系
        /// </summary>
        private void InitLoadedLuas()
        {
            foreach (RedisInstanceSetting setting in _options.ConnectionSettings)
            {
                IServer server = RedisInstanceManager.GetServer(setting);
                LoadedLuas loadedLuas = new LoadedLuas
                {
                    LoadedSetLua = server.ScriptLoad(_luaSet),
                    LoadedGetAndRefreshLua = server.ScriptLoad(_luaGetAndRefresh),

                    LoadedEntityGetAndRefreshLua = server.ScriptLoad(_luaEntityGetAndRefresh),
                    LoadedEntityGetAndRefreshByDimensionLua = server.ScriptLoad(_luaEntityGetAndRefreshByDimension),
                    LoadedEntitySetLua = server.ScriptLoad(_luaEntitySet),
                    LoadedEntityRemoveLua = server.ScriptLoad(_luaEntityRemove),
                    LoadedEntityRemoveByDimensionLua = server.ScriptLoad(_luaEntityRemoveByDimension)
                };

                _loadedLuaDict[setting.InstanceName] = loadedLuas;
            }
        }

        private LoadedLuas GetDefaultLoadLuas()
        {
            return GetLoadedLuas(DefaultInstanceName);
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

        private async Task<IDatabase> GetDatabaseAsync(string? instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                instanceName = DefaultInstanceName;
            }

            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return await RedisInstanceManager.GetDatabaseAsync(setting).ConfigureAwait(false);
            }

            throw new CacheException(ErrorCode.CacheLoadedLuaNotFound, $"Can not found Such Redis Instance: {instanceName}");
        }

        private Task<IDatabase> GetDefaultDatabaseAsync()
        {
            return GetDatabaseAsync(DefaultInstanceName);
        }

        private IDatabase GetDatabase(string? instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                instanceName = DefaultInstanceName;
            }

            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return RedisInstanceManager.GetDatabase(setting);
            }

            throw new CacheException(ErrorCode.CacheLoadedLuaNotFound, $"Can not found Such Redis Instance: {instanceName}");
        }

        private IDatabase GetDefaultDatabase()
        {
            return GetDatabase(DefaultInstanceName);
        }

        public void Close()
        {
            _instanceSettingDict.ForEach(kv =>
            {
                RedisInstanceManager.Close(kv.Value);
            });
        }

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Entity

        private string GetDimensionKey(string entityName, string dimensionKeyName, string dimensionKeyValue)
        {
            return GetRealKey(entityName + dimensionKeyName + dimensionKeyValue);
        }

        /// <summary>
        /// keys:guidkey
        /// </summary>
        private const string _luaEntityGetAndRefresh = @"
local data = redis.call('hmget', KEYS[1], 'absexp', 'sldexp','data','dim')

if (not data) then
    return nil
end

local now = tonumber((redis.call('time'))[1]) 

data[1] = tonumber(data[1])
data[2] = tonumber(data[2])

if(data[1]~= -1 and now >=data[1]) then 
    redis.call('del',KEYS[1])
    
    if (data[4]~='') then
        for i in string.gmatch(data[4], '%w+') do
           redis.call('del', i) 
        end
    end
    return 8 
end 

local curexp=-1

if(data[1]==-1 and data[2]~=-1) then
    curexp= data[2]
else if(data[1]~=-1 and data[2]~=-1) then
    curexp = data[1]-now
    if (data[2]<curexp) then
        curexp = data[2]
    end
else if(data[1]~=-1 and data[2]==-1) then
    curexp = data[1]-now
end

if(curexp~=-1) then 
    redis.call('expire', KEYS[1], curexp)
    
    if (data[4]~='') then
        for i in string.gmatch(data[4], '%w+') do
           redis.call('expire', i, curexp) 
        end
    end
end 

return data";

        /// <summary>
        /// KEYS:dimensionKey
        /// </summary>
        private const string _luaEntityGetAndRefreshByDimension = @"
local guid = redis.call('get',KEYS[1])

if (not guid) then
    return nil
end

local data= redis.call('hmget',guid, 'absexp', 'sldexp','data','dim') 

if (not data) then
    redis.call('del', KEYS[1])
    return 9
end

local now = tonumber((redis.call('time'))[1]) 

data[1] = tonumber(data[1])
data[2] = tonumber(data[2])

if(data[1]~= -1 and now >=data[1]) then 
    redis.call('del',guid)
    
    if (data[4]~='') then
        for i in string.gmatch(data[4], '%w+') do
           redis.call('del', i) 
        end
    end
    return 8 
end 

local curexp=-1

if(data[1]~=-1 and data[2]~=-1) then 
    curexp=data[1]-now 
    
    if (data[2]<curexp) then 
        curexp=data[2] 
    end 
elseif (data[1]~=-1) then 
    curexp=data[1]-now 
elseif (data[2]~=-1) then 
    curexp=data[2] 
end 

if(curexp~=-1) then 
    redis.call('expire', guid, curexp)
    
    if (data[4]~='') then
        for i in string.gmatch(data[4], '%w+') do
           redis.call('expire', i, curexp) 
        end
    end
end 

return data";

        /// <summary>
        /// keys: guidKey, dimensionkey1, dimensionkey2, dimensionkey3
        /// argv: absexp_value, sldexp_value, expire_value, data_value, dimensionKeyJoinedString, 3(dimensionkey_count)
        /// </summary>
        private const string _luaEntitySet = @"
redis.call('hmset', KEYS[1],'absexp',ARGV[1],'sldexp',ARGV[2],'data',ARGV[4], 'dim', ARGV[5]) 

if(ARGV[3]~='-1') then 
    redis.call('expire',KEYS[1], ARGV[3]) 
end 

for i=2, ARGV[6]+1 do
    redis.call('set', KEYS[i], KEYS[1])
    if (ARGV[3]~='-1') then
        redis.call('expire', KEYS[i], ARGV[3])
    end
end";

        /// <summary>
        /// keys: guidKey
        /// argv: 
        /// </summary>
        private const string _luaEntityRemove = @" 
local data=redis.call('hget', KEYS[1], 'dim')

redis.call('del', KEYS[1]) 

if (not data) do
    return 1
end

if(data[1]~='') then
    for i in string.gmatch(data[1], '%w+') do
        redis.call('del', i)
    end
end";

        /// <summary>
        /// keys:dimensionKey
        /// </summary>
        private const string _luaEntityRemoveByDimension = @"
local guid = redis.call('get',KEYS[1])

if (not guid) then
    return nil
end

local data= redis.call('hmget',guid, 'absexp', 'sldexp','data','dim') 

if (not data) then
    redis.call('del', KEYS[1])
    return 9
end


redis.call('del',guid)
    
if (data[4]~='') then
    for i in string.gmatch(data[4], '%w+') do
        redis.call('del', i) 
    end
end
return 1";

        public async Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIfNotCacheEnabled(entityDef);

            byte[] loadedScript;
            string redisKey;

            if (entityDef.GuidKeyProperty.Name == dimensionKeyName)
            {
                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityGetAndRefreshLua;
                redisKey = GetRealKey(dimensionKeyValue);
            }
            else
            {
                ThrowIfNotADimensionKeyName(dimensionKeyName, entityDef);

                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityGetAndRefreshByDimensionLua;
                redisKey = GetDimensionKey(entityDef.Name, dimensionKeyName, dimensionKeyValue);
            }

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    loadedScript,
                    new RedisKey[] { redisKey },
                    null).ConfigureAwait(false);

                return await MapGetEntityRedisResultAsync<TEntity>(result).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await GetEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token).ConfigureAwait(false);
            }
        }

        public async Task SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIfNotCacheEnabled(entityDef);

            string guidRealKey = GetRealKey(entityDef.GuidKeyProperty.GetValue(entity).ToString());

            DateTimeOffset? absulteExpireTime = entityDef.AbsoluteTimeRelativeToNow != null ? DateTimeOffset.UtcNow + entityDef.AbsoluteTimeRelativeToNow : null;
            long? absoluteExpireUnixSeconds = absulteExpireTime?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(entityDef.SlidingTime?.TotalSeconds);
            long? expireSeconds = GetExpireSeconds(absoluteExpireUnixSeconds, slideSeconds);

            byte[] data = await SerializeUtil.PackAsync(entity).ConfigureAwait(false);

            List<RedisKey> redisKeys = new List<RedisKey> { guidRealKey };

            StringBuilder joinedDimensinKeyBuilder = new StringBuilder();

            foreach (PropertyInfo property in entityDef.Dimensions)
            {
                string dimentionKey = GetDimensionKey(entityDef.Name, property.Name, property.GetValue(entity).ToString());
                redisKeys.Add(dimentionKey);
                joinedDimensinKeyBuilder.Append(dimentionKey);
                joinedDimensinKeyBuilder.Append(' ');
            }

            if (joinedDimensinKeyBuilder.Length > 0)
            {
                joinedDimensinKeyBuilder.Remove(joinedDimensinKeyBuilder.Length - 1, 1);
            }

            List<RedisValue> redisValues = new List<RedisValue> {
                absoluteExpireUnixSeconds ?? -1,
                slideSeconds ?? -1,
                expireSeconds ?? -1,
                data,
                joinedDimensinKeyBuilder.ToString(),
                entityDef.Dimensions.Count };

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                await database.ScriptEvaluateAsync(
                    GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntitySetLua,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await SetEntityAsync<TEntity>(entity, token).ConfigureAwait(false);
            }
        }

        public async Task RemoveEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIfNotCacheEnabled(entityDef);

            string redisKey;
            byte[] loadedScript;
            if (entityDef.GuidKeyProperty.Name == dimensionKeyName)
            {
                redisKey = GetRealKey(dimensionKeyValue);
                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityRemoveLua;
            }
            else
            {
                redisKey = GetDimensionKey(entityDef.Name, dimensionKeyName, dimensionKeyValue);
                loadedScript = GetLoadedLuas(entityDef.CacheInstanceName!).LoadedEntityRemoveByDimensionLua;
            }

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            try
            {
                await database.ScriptEvaluateAsync(loadedScript, new RedisKey[] { redisKey }, null).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await RemoveEntityAsync<TEntity>(dimensionKeyName, dimensionKeyValue, token).ConfigureAwait(false);
            }
        }

        public bool IsEnabled<TEntity>() where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            return entityDef.IsCacheable;
        }



        #endregion

        #region Batch Entity

        public async Task<(IEnumerable<TEntity?>, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIfNotCacheEnabled(entityDef);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);

            ITransaction transaction = database.CreateTransaction();

            RedisResult redisResult = await transaction.ScriptEvaluateAsync("").ConfigureAwait(false);

            transaction
        }

        public async Task SetEntitiesAsync<TEntity>(IEnumerable<TEntity> entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            ThrowIfNotCacheEnabled(entityDef);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);
        }

        public async Task RemoveEntitiesAsync<TEntity>(IEnumerable<string> dimensionKeyNames, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            ThrowIfNotCacheEnabled(entityDef);

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);


        }

        #endregion

        #region Privates

        private static async Task<(TEntity?, bool)> MapGetEntityRedisResultAsync<TEntity>(RedisResult result) where TEntity : Entity, new()
        {
            if (result.IsNull)
            {
                return (null, false);
            }

            RedisResult[]? results = (RedisResult[])result;

            if (results == null)
            {
                return (null, false);
            }

            TEntity? entity = await SerializeUtil.UnPackAsync<TEntity>((byte[])results[2]).ConfigureAwait(false);

            return (entity, true);
        }

        private static void ThrowIfNotADimensionKeyName(string dimensionKeyName, CacheEntityDef entityDef)
        {
            if (!entityDef.Dimensions.Any(p => p.Name == dimensionKeyName))
            {
                throw new CacheException(ErrorCode.CacheNoSuchDimensionKey, $"{entityDef.Name}, {dimensionKeyName}");
            }
        }

        private static void ThrowIfNotCacheEnabled(CacheEntityDef entityDef)
        {
            if (!entityDef.IsCacheable)
            {
                throw new CacheException(ErrorCode.CacheNotEnabledForEntity, $"{entityDef.Name}");
            }
        }

        #endregion

    }
}

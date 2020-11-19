using HB.Framework.Common.Cache;
using HB.Framework.Common.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    LoadedGetAndRefreshLua = server.ScriptLoad(_luaGetAndRefresh)
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

        private string GetDimensionKey(string entityName, string dimensionKeyName)
        {
            return _options.ApplicationName + entityName + dimensionKeyName;
        }

        /// <summary>
        /// 返回9 说明，dimension失效，要删除所有dimension
        /// 返回8，说明，找到，但过期，删除所有dimension
        /// 返回data，data[4]=7表示需要更新其他dimension
        /// </summary>
        private const string _luaEntityGetAndRefresh = @"
local guid = redis.call('hget',KEYS[1], ARGV[1])

if (not guid) then
    return nil
end

local data= redis.call('hmget',guid, 'absexp', 'sldexp','data') 

if (not data) then
    redis.call('del', KEYS[1])
    return 9
end

local now = tonumber((redis.call('time'))[1]) 

if(data[1]~= -1 and now >=tonumber(data[1])) then 
    redis.call('del',KEYS[1])
    redis.call('del',guid)
    return 8 
end 

local curexp=-1 

if(data[1]~=-1 and data[2]~=-1) then 
    curexp=data[1]-now 
    
    if (tonumber(data[2])<curexp) then 
        curexp=data[2] 
    end 
elseif (data[1]~=-1) then 
    curexp=data[1]-now 
elseif (data[2]~=-1) then 
    curexp=data[2] 
end 

if(curexp~=-1) then 
    redis.call('expire', guid, curexp)
    redis.call('expire', KEYS[1], curexp) 
    data[4]= 7
end 

return data";

        public async Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            if (entityDef.GuidKeyProperty.Name == dimensionKeyName)
            {
                //mean dimensionKeyValue is a guid
                return await GetAsync<TEntity>(dimensionKeyValue, token).ConfigureAwait(false);
            }

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName).ConfigureAwait(false);


        }

        public async Task SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();

            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName ?? DefaultInstanceName).ConfigureAwait(false);
        }

        public async Task SetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, TEntity? entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName ?? DefaultInstanceName).ConfigureAwait(false);
        }

        public async Task RemoveEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName ?? DefaultInstanceName).ConfigureAwait(false);
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
            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName ?? DefaultInstanceName).ConfigureAwait(false);
        }

        public async Task SetEntitiesAsync<TEntity>(IEnumerable<TEntity> entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName ?? DefaultInstanceName).ConfigureAwait(false);
        }

        public async Task SetEntitiesAsync<TEntity>(IEnumerable<string> dimensionKeyNames, IEnumerable<string> dimensionKeyValues, IEnumerable<TEntity?> entities, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName ?? DefaultInstanceName).ConfigureAwait(false);
        }

        public async Task RemoveEntitiesAsync<TEntity>(IEnumerable<string> dimensionKeyNames, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<TEntity>();
            IDatabase database = await GetDatabaseAsync(entityDef.CacheInstanceName ?? DefaultInstanceName).ConfigureAwait(false);
        }

        #endregion

    }
}

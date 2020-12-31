using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Cache
{
    internal class RedisCacheBase
    {
        protected const int _invalidationVersionExpirySeconds = 60;

        private readonly RedisCacheOptions _options;
        protected readonly ILogger _logger;

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;
        private readonly IDictionary<string, LoadedLuas> _loadedLuaDict = new Dictionary<string, LoadedLuas>();

        public RedisCacheBase(IOptions<RedisCacheOptions> options, ILogger logger)
        {
            _options = options.Value;
            _logger = logger;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);

            InitLoadedLuas();
        }

        public string DefaultInstanceName => _options.DefaultInstanceName ?? _options.ConnectionSettings[0].InstanceName;

        public void Close()
        {
            foreach (var kv in _instanceSettingDict)
            {
                RedisInstanceManager.Close(kv.Value);
            }
        }

        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 各服务器反复Load也没有关系
        /// </summary>
        protected void InitLoadedLuas()
        {
            foreach (RedisInstanceSetting setting in _options.ConnectionSettings)
            {
                IServer server = RedisInstanceManager.GetServer(setting, _logger);
                LoadedLuas loadedLuas = new LoadedLuas
                {
                    LoadedSetWithTimestampLua = server.ScriptLoad(RedisCache._luaSetWithTimestamp),
                    LoadedRemoveWithTimestampLua = server.ScriptLoad(RedisCache._luaRemoveWithTimestamp),
                    LoadedGetAndRefreshLua = server.ScriptLoad(RedisCache._luaGetAndRefresh),

                    LoadedEntitiesGetAndRefreshLua = server.ScriptLoad(RedisCache._luaEntitiesGetAndRefresh),
                    LoadedEntitiesGetAndRefreshByDimensionLua = server.ScriptLoad(RedisCache._luaEntitiesGetAndRefreshByDimension),
                    LoadedEntitiesSetLua = server.ScriptLoad(RedisCache._luaEntitiesSet),
                    LoadedEntitiesRemoveLua = server.ScriptLoad(RedisCache._luaEntitiesRemove),
                    LoadedEntitiesRemoveByDimensionLua = server.ScriptLoad(RedisCache._luaEntitiesRemoveByDimension)
                };

                _loadedLuaDict[setting.InstanceName] = loadedLuas;
            }
        }

        protected LoadedLuas GetDefaultLoadLuas()
        {
            return GetLoadedLuas(DefaultInstanceName);
        }

        protected LoadedLuas GetLoadedLuas(string? instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                instanceName = DefaultInstanceName;
            }

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

        protected async Task<IDatabase> GetDatabaseAsync(string? instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                instanceName = DefaultInstanceName;
            }

            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return await RedisInstanceManager.GetDatabaseAsync(setting, _logger).ConfigureAwait(false);
            }

            throw new CacheException(ErrorCode.CacheLoadedLuaNotFound, $"Can not found Such Redis Instance: {instanceName}");
        }

        protected Task<IDatabase> GetDefaultDatabaseAsync()
        {
            return GetDatabaseAsync(DefaultInstanceName);
        }

        protected IDatabase GetDatabase(string? instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                instanceName = DefaultInstanceName;
            }

            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return RedisInstanceManager.GetDatabase(setting, _logger);
            }

            throw new CacheException(ErrorCode.CacheLoadedLuaNotFound, $"Can not found Such Redis Instance: {instanceName}");
        }

        protected IDatabase GetDefaultDatabase()
        {
            return GetDatabase(DefaultInstanceName);
        }

        protected string GetRealKey(string entityName, string key)
        {
            return _options.ApplicationName + entityName + key;
        }

        protected string GetEntityDimensionKey(string entityName, string dimensionKeyName, string dimensionKeyValue)
        {
            return GetRealKey(entityName, dimensionKeyName + dimensionKeyValue);
        }

        protected static void ThrowIfNotADimensionKeyName(string dimensionKeyName, CacheEntityDef entityDef)
        {
            if (!entityDef.Dimensions.Any(p => p.Name == dimensionKeyName))
            {
                throw new CacheException(ErrorCode.CacheNoSuchDimensionKey, $"{entityDef.Name}, {dimensionKeyName}");
            }
        }

        protected static void ThrowIfNotCacheEnabled(CacheEntityDef entityDef)
        {
            if (!entityDef.IsCacheable)
            {
                throw new CacheException(ErrorCode.CacheNotEnabledForEntity, $"{entityDef.Name}");
            }
        }

        protected static long? GetInitialExpireSeconds(long? absoluteExpireUnixSeconds, long? slideSeconds)
        {
            //参见Readme.txt
            if (slideSeconds != null)
            {
                return slideSeconds.Value;
            }

            if (absoluteExpireUnixSeconds != null)
            {
                return absoluteExpireUnixSeconds - TimeUtil.UtcNowUnixTimeSeconds;
            }

            return null;
        }
    }
}
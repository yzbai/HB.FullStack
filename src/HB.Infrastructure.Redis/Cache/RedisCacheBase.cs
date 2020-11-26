using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.Framework.Cache;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Cache
{
    internal class RedisCacheBase
    {
        private readonly RedisCacheOptions _options;
        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;
        private readonly IDictionary<string, LoadedLuas> _loadedLuaDict = new Dictionary<string, LoadedLuas>();

        public RedisCacheBase(IOptions<RedisCacheOptions> options)
        {
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);

            InitLoadedLuas();
        }

        public string DefaultInstanceName => _options.DefaultInstanceName ?? _options.ConnectionSettings[0].InstanceName;

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

        /// <summary>
        /// 各服务器反复Load也没有关系
        /// </summary>
        protected void InitLoadedLuas()
        {
            foreach (RedisInstanceSetting setting in _options.ConnectionSettings)
            {
                IServer server = RedisInstanceManager.GetServer(setting);
                LoadedLuas loadedLuas = new LoadedLuas();


                loadedLuas.LoadedSetLua = server.ScriptLoad(RedisCache._luaSet);
                loadedLuas.LoadedGetAndRefreshLua = server.ScriptLoad(RedisCache._luaGetAndRefresh);

                loadedLuas.LoadedEntityGetAndRefreshLua = server.ScriptLoad(RedisCache._luaEntityGetAndRefresh);
                loadedLuas.LoadedEntityGetAndRefreshByDimensionLua = server.ScriptLoad(RedisCache._luaEntityGetAndRefreshByDimension);
                loadedLuas.LoadedEntitySetLua = server.ScriptLoad(RedisCache._luaEntitySet);
                loadedLuas.LoadedEntityRemoveLua = server.ScriptLoad(RedisCache._luaEntityRemove);
                loadedLuas.LoadedEntityRemoveByDimensionLua = server.ScriptLoad(RedisCache._luaEntityRemoveByDimension);
                loadedLuas.LoadedEntitiesGetAndRefreshLua = server.ScriptLoad(RedisCache._luaEntitiesGetAndRefresh);
                loadedLuas.LoadedEntitiesGetAndRefreshByDimensionLua = server.ScriptLoad(RedisCache._luaEntitiesGetAndRefreshByDimension);
                loadedLuas.LoadedEntitiesSetLua = server.ScriptLoad(RedisCache._luaEntitiesSet);
                loadedLuas.LoadedEntitiesRemoveLua = server.ScriptLoad(RedisCache._luaEntitiesRemove);
                loadedLuas.LoadedEntitiesRemoveByDimensionLua = server.ScriptLoad(RedisCache._luaEntitiesRemoveByDimension);

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
                return await RedisInstanceManager.GetDatabaseAsync(setting).ConfigureAwait(false);
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
                return RedisInstanceManager.GetDatabase(setting);
            }

            throw new CacheException(ErrorCode.CacheLoadedLuaNotFound, $"Can not found Such Redis Instance: {instanceName}");
        }

        protected IDatabase GetDefaultDatabase()
        {
            return GetDatabase(DefaultInstanceName);
        }

        protected string GetRealKey(string key)
        {
            return _options.ApplicationName + key;
        }

        protected string GetEntityDimensionKey(string entityName, string dimensionKeyName, string dimensionKeyValue)
        {
            return GetRealKey(entityName + dimensionKeyName + dimensionKeyValue);
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

        protected static void ThrowIfNotBactchEnabled(CacheEntityDef entityDef)
        {
            if (!entityDef.IsBatchEnabled)
            {
                throw new CacheException(ErrorCode.CacheBatchNotEnabled, $"{entityDef.Name}");
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
                return absoluteExpireUnixSeconds - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            return null;
        }
    }
}
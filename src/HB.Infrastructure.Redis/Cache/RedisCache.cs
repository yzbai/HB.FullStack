using HB.Framework.Cache;
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

        public RedisCache(IOptions<RedisCacheOptions> options, ILogger<RedisCache> logger)
        {
            _logger = logger;
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
        private void InitLoadedLuas()
        {
            foreach (RedisInstanceSetting setting in _options.ConnectionSettings)
            {
                IServer server = RedisInstanceManager.GetServer(setting);
                LoadedLuas loadedLuas = new LoadedLuas();


                loadedLuas.LoadedSetLua = server.ScriptLoad(_luaSet);
                loadedLuas.LoadedGetAndRefreshLua = server.ScriptLoad(_luaGetAndRefresh);
                loadedLuas.LoadedEntityGetAndRefreshLua = server.ScriptLoad(_luaEntityGetAndRefresh);
                loadedLuas.LoadedEntityGetAndRefreshByDimensionLua = server.ScriptLoad(_luaEntityGetAndRefreshByDimension);
                loadedLuas.LoadedEntitySetLua = server.ScriptLoad(_luaEntitySet);
                loadedLuas.LoadedEntityRemoveLua = server.ScriptLoad(_luaEntityRemove);
                loadedLuas.LoadedEntityRemoveByDimensionLua = server.ScriptLoad(_luaEntityRemoveByDimension);
                loadedLuas.LoadedEntitiesGetAndRefreshLua = server.ScriptLoad(_luaEntitiesGetAndRefresh);
                loadedLuas.LoadedEntitiesGetAndRefreshByDimensionLua = server.ScriptLoad(_luaEntitiesGetAndRefreshByDimension);
                loadedLuas.LoadedEntitiesSetLua = server.ScriptLoad(_luaEntitiesSet);
                loadedLuas.LoadedEntitiesRemoveLua = server.ScriptLoad(_luaEntitiesRemove);
                loadedLuas.LoadedEntitiesRemoveByDimensionLua = server.ScriptLoad(_luaEntitiesRemoveByDimension);



                _loadedLuaDict[setting.InstanceName] = loadedLuas;
            }
        }

        private LoadedLuas GetDefaultLoadLuas()
        {
            return GetLoadedLuas(DefaultInstanceName);
        }

        private LoadedLuas GetLoadedLuas(string? instanceName)
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

        private string GetRealKey(string key)
        {
            return _options.ApplicationName + key;
        }

        private string GetEntityDimensionKey(string entityName, string dimensionKeyName, string dimensionKeyValue)
        {
            return GetRealKey(entityName + dimensionKeyName + dimensionKeyValue);
        }

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

            TEntity? entity = await SerializeUtil.UnPackAsync<TEntity>((byte[])results[1]).ConfigureAwait(false);

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

        private static void ThrowIfNotBactchEnabled(CacheEntityDef entityDef)
        {
            if (!entityDef.IsBatchEnabled)
            {
                throw new CacheException(ErrorCode.CacheBatchNotEnabled, $"{entityDef.Name}");
            }
        }

    }
}

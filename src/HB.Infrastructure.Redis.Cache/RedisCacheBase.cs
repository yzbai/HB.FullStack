﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.FullStack.Common.Cache.CacheModels;
using HB.Infrastructure.Redis.Shared;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Cache
{
    public class RedisCacheBase
    {
        protected const int INVALIDATION_VERSION_EXPIRY_SECONDS = 60;

        private readonly RedisCacheOptions _options;

        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;
        private readonly IDictionary<string, LoadedLuas> _loadedLuaDict = new Dictionary<string, LoadedLuas>();
        protected ILogger Logger { get; private set; }

        public RedisCacheBase(IOptions<RedisCacheOptions> options, ILogger logger)
        {
            _options = options.Value;
            Logger = logger;
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
                IServer server = RedisInstanceManager.GetServer(setting, Logger);
                LoadedLuas loadedLuas = new LoadedLuas
                {
                    LoadedCollectionGetAndRefreshWithTimestampLua = server.ScriptLoad(RedisCache.LUA_COLLECTION_GET_AND_REFRESH_WITH_TIMESTAMP),
                    LoadedCollectionRemoveItemWithTimestampLua = server.ScriptLoad(RedisCache.LUA_COLLECTION_REMOVE_ITEM_WITH_TIMESTAMP),
                    LoadedCollectionSetWithTimestampLua = server.ScriptLoad(RedisCache.LUA_COLLECTION_SET_WITH_TIMESTAMP),

                    LoadedSetWithTimestampLua = server.ScriptLoad(RedisCache.LUA_SET_WITH_TIMESTAMP),
                    LoadedRemoveWithTimestampLua = server.ScriptLoad(RedisCache.LUA_REMOVE_WITH_TIMESTAMP),
                    LoadedRemoveMultipleWithTimestampLua = server.ScriptLoad(RedisCache.LUA_REMOVE_MULTIPLE_WITH_TIMESTAMP),
                    LoadedGetAndRefreshLua = server.ScriptLoad(RedisCache.LUA_GET_AND_REFRESH_WITH_TIMESTAMP),

                    LoadedModelsGetAndRefreshLua = server.ScriptLoad(RedisCache.LUA_MODELS_GET_AND_REFRESH),
                    LoadedModelsGetAndRefreshByDimensionLua = server.ScriptLoad(RedisCache.LUA_MODELS_GET_AND_REFRESH_BY_DIMENSION),
                    LoadedModelsSetLua = server.ScriptLoad(RedisCache.LUA_MODELS_SET),
                    LoadedModelsRemoveLua = server.ScriptLoad(RedisCache.LUA_MODELS_REMOVE),
                    LoadedModelsRemoveByDimensionLua = server.ScriptLoad(RedisCache.LUA_MODELS_REMOVE_BY_DIMENSION),

                    LoadedModelsForcedRemoveLua = server.ScriptLoad(RedisCache.LUA_MODELS_REMOVE_FORECED_NO_VERSION),
                    LoadedModelsForcedRemoveByDimensionLua = server.ScriptLoad(RedisCache.LUA_MODELS_REMOVE_BY_DIMENSION_FORCED_NO_VERSION),
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

            if (_loadedLuaDict.TryGetValue(instanceName, out LoadedLuas? loadedLuas))
            {
                return loadedLuas;
            }

            InitLoadedLuas();

            if (_loadedLuaDict.TryGetValue(instanceName, out LoadedLuas? loadedLuas2))
            {
                return loadedLuas2;
            }

            throw CacheExceptions.CacheLoadedLuaNotFound(cacheInstanceName: instanceName);
        }

        protected async Task<IDatabase> GetDatabaseAsync(string? instanceName)
        {
            instanceName ??= DefaultInstanceName;

            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting? setting))
            {
                return await RedisInstanceManager.GetDatabaseAsync(setting, Logger).ConfigureAwait(false);
            }

            throw CacheExceptions.InstanceNotFound(instanceName);
        }

        protected Task<IDatabase> GetDefaultDatabaseAsync()
        {
            return GetDatabaseAsync(DefaultInstanceName);
        }

        protected IDatabase GetDatabase(string? instanceName)
        {
            instanceName ??= DefaultInstanceName;

            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting? setting))
            {
                return RedisInstanceManager.GetDatabase(setting, Logger);
            }

            throw CacheExceptions.InstanceNotFound(instanceName);
        }

        protected IDatabase GetDefaultDatabase()
        {
            return GetDatabase(DefaultInstanceName);
        }

        protected string GetRealKey(string modelName, string key)
        {
            return _options.ApplicationName + modelName + key;
        }

        protected string GetModelDimensionKey(string modelName, string dimensionKeyName, string dimensionKeyValue)
        {
            return GetRealKey(modelName, dimensionKeyName + dimensionKeyValue);
        }

        protected static void ThrowIfNotADimensionKeyName(string dimensionKeyName, CacheModelDef modelDef)
        {
            if (!modelDef.Dimensions.Any(p => p.Name == dimensionKeyName))
            {
                throw CacheExceptions.NoSuchDimensionKey(typeName: modelDef.Name, dimensionKeyName: dimensionKeyName);
            }
        }

        /// <summary>
        /// ThrowIfNotCacheEnabled
        /// </summary>
        /// <param name="modelDef"></param>

        protected static void ThrowIfNotCacheEnabled(CacheModelDef modelDef)
        {
            if (!modelDef.IsCacheable)
            {
                throw CacheExceptions.NotEnabledForModel(modelDef.Name);
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
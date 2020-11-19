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
    internal partial class RedisCache : ICache, IDisposable
    {
        private readonly ILogger _logger;
        private readonly RedisCacheOptions _options;
        private readonly IDictionary<string, RedisInstanceSetting> _instanceSettingDict;

        public string DefaultInstanceName => _options.DefaultInstanceName ?? _options.ConnectionSettings[0].InstanceName;

        public RedisCache(IOptions<RedisCacheOptions> options, ILogger<RedisCache> logger)
        {
            _logger = logger;
            _options = options.Value;
            _instanceSettingDict = _options.ConnectionSettings.ToDictionary(s => s.InstanceName);
        }

        #region privates
        private async Task<IDatabase> GetDatabaseAsync(string instanceName)
        {
            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return await RedisInstanceManager.GetDatabaseAsync(setting, _logger).ConfigureAwait(false);
            }

            throw new CacheException(ErrorCode.CacheNotFoundInstance, $"Can not found Such Redis Instance: {instanceName}");
        }

        private Task<IDatabase> GetDefaultDatabaseAsync()
        {
            return GetDatabaseAsync(_options.DefaultInstanceName ?? _options.ConnectionSettings[0].InstanceName);
        }

        private IDatabase GetDatabase(string instanceName)
        {
            if (_instanceSettingDict.TryGetValue(instanceName, out RedisInstanceSetting setting))
            {
                return RedisInstanceManager.GetDatabase(setting, _logger);
            }

            throw new CacheException(ErrorCode.CacheNotFoundInstance, $"Can not found Such Redis Instance: {instanceName}");
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

        public Task<(TEntity?, bool)> GetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        public Task SetEntityAsync<TEntity>(TEntity entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        public Task SetEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, TEntity? entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        public Task RemoveEntityAsync<TEntity>(string dimensionKeyName, string dimensionKeyValue, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled<TEntity>() where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Batch Entity

        public Task<(IEnumerable<TEntity?>, bool)> GetEntitiesAsync<TEntity>(string dimensionKeyName, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        public Task SetEntitiesAsync<TEntity>(IEnumerable<TEntity> entity, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        public Task SetEntitiesAsync<TEntity>(IEnumerable<string> dimensionKeyNames, IEnumerable<string> dimensionKeyValues, IEnumerable<TEntity?> entities, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        public Task RemoveEntitiesAsync<TEntity>(IEnumerable<string> dimensionKeyNames, IEnumerable<string> dimensionKeyValues, CancellationToken token = default(CancellationToken)) where TEntity : Entity, new()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}

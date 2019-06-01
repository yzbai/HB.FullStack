using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    internal class RedisInstanceManager : IRedisInstanceManager
    {      
        private readonly ILogger _logger;

        //instanceName : RedisConnection
        private readonly Dictionary<string, RedisConnection> _connectionDict;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        private readonly RedisOptions _options;

        public RedisInstanceManager(IOptions<RedisOptions> options, ILogger<RedisInstanceManager> logger)
        {
            _logger = logger;
            _options = options.Value;

            _connectionDict = new Dictionary<string, RedisConnection>();
        }

        public IDatabase GetDatabase(string instanceName)
        {
            if (!_connectionDict.ContainsKey(instanceName))
            {
                RedisInstanceSetting setting = _options.GetInstanceSetting(instanceName);

                if (setting == null)
                {
                    return null;
                }

                _connectionDict[instanceName] = new RedisConnection(setting.ConnectionString);
            }

            RedisConnection redisWrapper = _connectionDict[instanceName];

            if (redisWrapper.Connection != null && !redisWrapper.Connection.IsConnected)
            {
                Thread.Sleep(5000);
            }

            if (redisWrapper.Connection == null || !redisWrapper.Connection.IsConnected || redisWrapper.Database == null)
            {
                _connectionLock.Wait();

                //TODO: add polly here,
                //TODO: add heath check
                //TODO: add Event Listening
                //TODO: add More Log here
                //TODO: dig into ConfigurationOptions when create ConnectionMultiplexer

                try
                {
                    redisWrapper.Connection?.Dispose();

                    ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(redisWrapper.ConnectionString);

                    //TODO: add into configure file
                    configurationOptions.AbortOnConnectFail = false;
                    configurationOptions.KeepAlive = 60;
                    configurationOptions.ConnectTimeout = 10 * 1000;
                    configurationOptions.ResponseTimeout = 100 * 1000;
                    configurationOptions.SyncTimeout = 100 * 1000;

                    //TODO: add detailed ConfigurationOptions Settings, like abortOnConnectionFailed, Should Retry Policy, etc.;

                    redisWrapper.Connection = ConnectionMultiplexer.Connect(configurationOptions);
                    redisWrapper.Database = redisWrapper.Connection.GetDatabase(0);
                    //redisWrapper.Connection.ConnectionFailed += Connection_ConnectionFailed;

                    _logger.LogInformation($"Redis KVStoreEngine Connection ReConnected : {instanceName}");
                }
                finally
                {
                    _connectionLock.Release();
                }
            }

            return redisWrapper.Database;
        }

        public RedisInstanceSetting GetInstanceSetting(string instanceName)
        {
            return _options.GetInstanceSetting(instanceName);
        }

        #region Dispose Support

        private readonly object _disposeLocker = new object();
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free managed

                lock (_disposeLocker)
                {
                    if (_connectionLock != null)
                    {
                        _connectionLock.Dispose();
                    }

                    if (_connectionDict != null)
                    {
                        foreach (KeyValuePair<string, RedisConnection> pair in _connectionDict)
                        {
                            pair.Value?.Connection?.Close();
                        }
                    }
                }
            }

            // Free unmanaged


            _disposed = true;
        }


        ~RedisInstanceManager()
        {
            Dispose(false);
        }

        #endregion
    }
}

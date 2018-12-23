using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    public class RedisConnectionManager : IRedisConnectionManager
    {      
        private ILogger _logger;
        private Dictionary<string, RedisConnection> _connectionDict;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        protected RedisOptions Options { get; private set; }

        public RedisConnectionManager(RedisOptions options, ILogger<RedisConnectionManager> logger)
        {
            _logger = logger;
            Options = options;
            _connectionDict = new Dictionary<string, RedisConnection>();
        }

        public IDatabase GetDatabase(string instanceName, int dbIndex, bool isMaster)
        {
            string key = string.Format(GlobalSettings.Culture, "{0}:{1}:{2}", instanceName, dbIndex, isMaster ? 1 : 0);

            if (!_connectionDict.ContainsKey(key))
            {
                IEnumerable<RedisConnectionSetting> rss = Options.ConnectionSettings.Where(rs => rs.InstanceName.Equals(instanceName, GlobalSettings.Comparison) && rs.IsMaster == isMaster);

                if (rss.Count() == 0 && isMaster == false)
                {
                    rss = Options.ConnectionSettings.Where(rs => rs.InstanceName.Equals(instanceName, GlobalSettings.Comparison) && rs.IsMaster);
                }

                if (rss.Count() == 0)
                {
                    return null;
                }

                _connectionDict[key] = new RedisConnection(rss.ElementAt(0).ConnectionString);
            }

            RedisConnection redisWrapper = _connectionDict[key];

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
                    //TODO: add detailed ConfigurationOptions Settings, like abortOnConnectionFailed, Should Retry Policy, etc.;

                    redisWrapper.Connection = ConnectionMultiplexer.Connect(configurationOptions);
                    redisWrapper.Database = redisWrapper.Connection.GetDatabase(dbIndex);
                    //redisWrapper.Connection.ConnectionFailed += Connection_ConnectionFailed;

                    _logger.LogInformation("Redis KVStoreEngine Connection ReConnected : " + key);
                }
                finally
                {
                    _connectionLock.Release();
                }
            }

            return redisWrapper.Database;
        }

        public IDatabase GetReadDatabase(string instanceName, int dbIndex)
        {
            return GetDatabase(instanceName, dbIndex, false);
        }

        public IDatabase GetWriteDatabase(string instanceName, int dbIndex)
        {
            return GetDatabase(instanceName, dbIndex, true);
        }

        private void Connection_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            
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

        ~RedisConnectionManager()
        {
            Dispose(false);
        }

        #endregion
    }
}

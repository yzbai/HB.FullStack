using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    internal class RedisWrapper
    {
        public string ConnectionString { get; set; }
        public ConnectionMultiplexer Connection { get; set; }
        public IDatabase Database { get; set; }

        public RedisWrapper(string connectionString)
        {
            ConnectionString = connectionString;
            Connection = null;
            Database = null;
        }
    }

    public class RedisEngineBase : IDisposable
    {
        private bool _disposed = false;
        private ILogger _logger;
        private Dictionary<string, RedisWrapper> _connectionDict;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly object _closeLocker = new object();

        protected RedisEngineOptions Options { get; private set; }

        public RedisEngineBase(IApplicationLifetime applicationLifetime, RedisEngineOptions options, ILogger logger)
        {
            _logger = logger;
            Options = options;
            _connectionDict = new Dictionary<string, RedisWrapper>();

            applicationLifetime.ApplicationStopped.Register(() => {
                Dispose();
            });
        }

        protected IDatabase GetDatabase(string dbName, int dbIndex, bool isMaster)
        {
            string key = string.Format("{0}:{1}:{2}", dbName, dbIndex, isMaster ? 1 : 0);

            if (!_connectionDict.ContainsKey(key))
            {
                IEnumerable<RedisConnectionSetting> rss = Options.ConnectionSettings.Where(rs => rs.Name.Equals(dbName) && rs.IsMaster == isMaster);

                if (rss.Count() == 0 && isMaster == false)
                {
                    rss = Options.ConnectionSettings.Where(rs => rs.Name.Equals(dbName) && rs.IsMaster);
                }

                if (rss.Count() == 0)
                {
                    return null;
                }

                _connectionDict[key] = new RedisWrapper(rss.ElementAt(0).ConnectionString);
            }

            RedisWrapper redisWrapper = _connectionDict[key];

            if (redisWrapper.Connection != null && !redisWrapper.Connection.IsConnected)
            {
                Thread.Sleep(3000);
            }

            if (redisWrapper.Connection == null || !redisWrapper.Connection.IsConnected || redisWrapper.Database == null)
            {
                _connectionLock.Wait();
                try
                {
                    redisWrapper.Connection?.Dispose();
                    redisWrapper.Connection = ConnectionMultiplexer.Connect(redisWrapper.ConnectionString);
                    redisWrapper.Database = redisWrapper.Connection.GetDatabase(dbIndex);

                    _logger.LogInformation("Redis KVStoreEngine Connection ReConnected : " + key);
                }
                finally
                {
                    _connectionLock.Release();
                }
            }

            return redisWrapper.Database;
        }

        protected IDatabase GetReadDatabase(string dbName, int dbIndex)
        {
            return GetDatabase(dbName, dbIndex, false);
        }

        protected IDatabase GetWriteDatabase(string dbName, int dbIndex)
        {
            return GetDatabase(dbName, dbIndex, true);
        }

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
            }

            // Free unmanaged
            lock (_closeLocker)
            {
                if (_connectionDict != null)
                {
                    foreach (KeyValuePair<string, RedisWrapper> pair in _connectionDict)
                    {
                        pair.Value?.Connection?.Dispose();
                    }
                }
            }

            _disposed = true;
        }

        ~RedisEngineBase()
        {
            Dispose(false);
        }
    }
}

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
        private ILogger _logger;
        private Dictionary<string, RedisWrapper> _connectionDict;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        protected RedisEngineOptions Options { get; private set; }

        public RedisEngineBase(RedisEngineOptions options, ILogger logger)
        {
            _logger = logger;
            Options = options;
            _connectionDict = new Dictionary<string, RedisWrapper>();
        }

        protected IDatabase GetDatabase(string dbName, int dbIndex, bool isMaster)
        {
            string key = string.Format(GlobalSettings.Culture, "{0}:{1}:{2}", dbName, dbIndex, isMaster ? 1 : 0);

            if (!_connectionDict.ContainsKey(key))
            {
                IEnumerable<RedisConnectionSetting> rss = Options.ConnectionSettings.Where(rs => rs.InstanceName.Equals(dbName, GlobalSettings.Comparison) && rs.IsMaster == isMaster);

                if (rss.Count() == 0 && isMaster == false)
                {
                    rss = Options.ConnectionSettings.Where(rs => rs.InstanceName.Equals(dbName, GlobalSettings.Comparison) && rs.IsMaster);
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
                    redisWrapper.Connection = ConnectionMultiplexer.Connect(redisWrapper.ConnectionString);
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

        private void Connection_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            
        }

        protected IDatabase GetReadDatabase(string dbName, int dbIndex)
        {
            return GetDatabase(dbName, dbIndex, false);
        }

        protected IDatabase GetWriteDatabase(string dbName, int dbIndex)
        {
            return GetDatabase(dbName, dbIndex, true);
        }

        protected IDatabase GetQueueDatabase()
        {
            RedisConnectionSetting connectionSetting = Options.GetQueueConnectionSetting();

            if (connectionSetting == null)
            {
                string msg = "没有支持Queue的Redis实例";
                Exception ex = new Exception(msg);

                _logger.LogCritical(ex, msg);

                throw ex;
            }

            return GetDatabase(connectionSetting.InstanceName, 0, true);

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
                        foreach (KeyValuePair<string, RedisWrapper> pair in _connectionDict)
                        {
                            pair.Value?.Connection?.Dispose();
                        }
                    }
                }
            }

            // Free unmanaged


            _disposed = true;
        }

        ~RedisEngineBase()
        {
            Dispose(false);
        }

        #endregion
    }
}

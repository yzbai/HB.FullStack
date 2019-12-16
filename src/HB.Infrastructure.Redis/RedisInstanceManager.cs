using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis
{
    internal static class RedisInstanceManager
    {      
        private static Dictionary<string, RedisConnection> _connectionDict = new Dictionary<string, RedisConnection>();      //instanceName : RedisConnection
        private static readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private static readonly object _closeLocker = new object();

        public static IDatabase GetDatabase(RedisInstanceSetting setting, ILogger logger)
        {
            if (setting == null)
            {
                return null;
            }

            if (!_connectionDict.ContainsKey(setting.InstanceName))
            {
                _connectionDict[setting.InstanceName] = new RedisConnection(setting.ConnectionString);
            }

            RedisConnection redisWrapper = _connectionDict[setting.InstanceName];

            if (redisWrapper.Connection != null && !redisWrapper.Connection.IsConnected)
            {
                //TODO: Check this
                //Thread.Sleep(5000);
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
                    //configurationOptions.ResponseTimeout = 100 * 1000;
                    configurationOptions.SyncTimeout = 100 * 1000;

                    //TODO: add detailed ConfigurationOptions Settings, like abortOnConnectionFailed, Should Retry Policy, etc.;

                    ReConnectPolicy(logger, redisWrapper.ConnectionString).Execute(async ()=> {
                        redisWrapper.Connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions).ConfigureAwait(false);
                        redisWrapper.Database = redisWrapper.Connection.GetDatabase(0);
                        //redisWrapper.Connection.ConnectionFailed += Connection_ConnectionFailed;

                        logger.LogInformation($"Redis KVStoreEngine Connection ReConnected : {setting.InstanceName}");
                    });
                }
                finally
                {
                    _connectionLock.Release();
                }
            }

            return redisWrapper.Database;
        }

        private static RetryPolicy ReConnectPolicy(ILogger logger, string connectionString)
        {
            //TODO: move this settings to options
            return Policy
                        .Handle<RedisConnectionException>()
                        .WaitAndRetry(
                            3,
                            count => TimeSpan.FromSeconds(5 + count * 2),
                            (exception, timeSpan, retryCount, context) => {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                logger.LogError(exception, $"Redis Connection lost. Try {retryCount}th times. Wait For {timeSpan.TotalSeconds} seconds. Redis Can not connect {connectionString}");
                            });
        }

        public static void Close(RedisInstanceSetting setting)
        {
            lock(_closeLocker)
            {
                if (_connectionDict.ContainsKey(setting.InstanceName))
                {
                    _connectionDict[setting.InstanceName].Connection?.Close();

                    _connectionDict.Remove(setting.InstanceName);
                }
            }
        }

        public static void CloseAll()
        {
            lock(_closeLocker)
            {
                _connectionDict.ForEach(kv => {
                    kv.Value?.Connection?.Close();
                });

                _connectionDict = new Dictionary<string, RedisConnection>();
            }
        }
    }
}

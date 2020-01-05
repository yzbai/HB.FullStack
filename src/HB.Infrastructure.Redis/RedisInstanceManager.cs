using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public static async Task<IDatabase> GetDatabaseAsync(RedisInstanceSetting setting, ILogger logger)
        {
            ThrowIf.Null(setting, nameof(setting));

            if (!_connectionDict.ContainsKey(setting.InstanceName))
            {
                _connectionDict[setting.InstanceName] = new RedisConnection(setting.ConnectionString);
            }

            RedisConnection redisWrapper = _connectionDict[setting.InstanceName];

            while (redisWrapper.Connection != null && redisWrapper.Connection.IsConnecting)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }

            if (redisWrapper.Connection == null || !redisWrapper.Connection.IsConnected || redisWrapper.Database == null)
            {
                try
                {
                    _connectionLock.Wait();

                    redisWrapper.Connection?.Dispose();

                    ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(redisWrapper.ConnectionString);

                    //configurationOptions.AbortOnConnectFail = false;
                    //configurationOptions.KeepAlive = 60;
                    //configurationOptions.ConnectTimeout = 10 * 1000;
                    //configurationOptions.SyncTimeout = 100 * 1000;

                    await ReConnectPolicyAsync(logger, redisWrapper.ConnectionString).ExecuteAsync(async ()=> {
                        redisWrapper.Connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions).ConfigureAwait(false);
                        redisWrapper.Database = redisWrapper.Connection.GetDatabase(setting.DatabaseNumber);
                        //redisWrapper.Connection.ConnectionFailed += Connection_ConnectionFailed;

                        logger.LogInformation($"Redis KVStoreEngine Connection ReConnected : {setting.InstanceName}");
                    }).ConfigureAwait(false);
                }
                finally
                {
                    _connectionLock.Release();
                }
            }

            return redisWrapper.Database;
        }

        private static AsyncRetryPolicy ReConnectPolicyAsync(ILogger logger, string connectionString)
        {
            //TODO: move this settings to options
            return Policy
                        .Handle<RedisConnectionException>()
                        .WaitAndRetryForeverAsync(
                            count => TimeSpan.FromSeconds(5 + count * 2),
                            (exception, retryCount, timeSpan) => {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                logger.LogCritical(exception, $"Redis Connection lost. Try {retryCount}th times. Wait For {timeSpan.TotalSeconds} seconds. Redis Can not connect {connectionString}");
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

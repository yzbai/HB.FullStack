#nullable enable

using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis
{
    internal static class RedisInstanceManager
    {
        private static ConcurrentDictionary<string, ConnectionMultiplexer> _connectionDict = new ConcurrentDictionary<string, ConnectionMultiplexer>();      //instanceName : RedisConnection
        private static readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public static async Task<IDatabase> GetDatabaseAsync(RedisInstanceSetting setting, ILogger logger)
        {
            if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer cached))
            {
                return cached.GetDatabase(setting.DatabaseNumber);
            }

            try
            {
                _connectionLock.Wait();

                //Double check
                if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer cached2))
                {
                    return cached2.GetDatabase(setting.DatabaseNumber);
                }

                ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(setting.ConnectionString);

                //TODO: 调查参数
                //configurationOptions.KeepAlive = 30;
                //configurationOptions.ConnectTimeout = 10 * 1000;
                //configurationOptions.SyncTimeout = 100 * 1000;
                //configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);

                IDatabase database = null!;

                await ReConnectPolicyAsync(logger, setting.ConnectionString).ExecuteAsync(async () =>
                {
                    ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions).ConfigureAwait(false);

                    _connectionDict[setting.InstanceName] = connection;

                    database = connection.GetDatabase(setting.DatabaseNumber);

                    logger.LogInformation($"Redis 链接建立 Connected : {setting.InstanceName}");
                }).ConfigureAwait(false);

                _connectionLock.Release();

                return database;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _connectionLock.Release();

                logger.LogCritical(ex, $"Redis Database获取失败.尝试重新获取。 SettingName:{setting.InstanceName}, Connection:{setting.ConnectionString}");

                Close(setting);

                await Task.Delay(1000).ConfigureAwait(false);

                return await GetDatabaseAsync(setting, logger).ConfigureAwait(false);
            }
        }

        private static AsyncRetryPolicy ReConnectPolicyAsync(ILogger logger, string connectionString)
        {
            //TODO: move this settings to options
            return Policy
                        .Handle<RedisConnectionException>()
                        .WaitAndRetryForeverAsync(
                            count => TimeSpan.FromSeconds(5),
                            (exception, retryCount, timeSpan) =>
                            {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                logger.LogCritical(exception, $"Redis 建立链接时失败 Connection lost. Try {retryCount}th times. Wait For {timeSpan.TotalSeconds} seconds. Redis Can not connect {connectionString}");
                            });
        }

        public static void Close(RedisInstanceSetting setting)
        {
            if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer connection))
            {
                connection.Dispose();
                _connectionDict.TryRemove(setting.InstanceName, out ConnectionMultiplexer _);
            }
        }

        public static void CloseAll()
        {
            _connectionDict.ForEach(kv =>
            {
                kv.Value?.Dispose();
                _connectionDict.TryRemove(kv.Key, out ConnectionMultiplexer _);
            });

            _connectionDict = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        }
    }
}

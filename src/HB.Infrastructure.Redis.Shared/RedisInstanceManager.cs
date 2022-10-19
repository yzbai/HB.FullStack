
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Shared
{
    public static class RedisInstanceManager
    {
        private static ConcurrentDictionary<string, ConnectionMultiplexer> _connectionDict = new ConcurrentDictionary<string, ConnectionMultiplexer>();      //instanceName : RedisConnection
        private static readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        private static ConnectionMultiplexer GetConnectionMultiplexer(RedisInstanceSetting setting, ILogger logger)
        {
            if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer? cached))
            {
                return cached;
            }

            try
            {
                _connectionLock.Wait();

                //Double check
                if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer? cached2))
                {
                    return cached2;
                }

                ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(setting.ConnectionString);

                //TODO: 调查参数
                //configurationOptions.KeepAlive = 30;
                //configurationOptions.ConnectTimeout = 10 * 1000;
                //configurationOptions.SyncTimeout = 100 * 1000;
                //configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);

                ConnectionMultiplexer connection = null!;

                ReConnectPolicy(setting.ConnectionString, logger).Execute(() =>
                {
                    connection = ConnectionMultiplexer.Connect(configurationOptions);

                    _connectionDict[setting.InstanceName] = connection;

                    SetConnectionEvents(connection, logger);

                    logger.LogInformation("Redis 链接建立 Connected : {CacheInstanceName}", setting.InstanceName);
                });

                _connectionLock.Release();

                return connection;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _connectionLock.Release();

                logger.LogCritical(ex, "Redis Database获取失败.尝试重新获取。 SettingName:{CacheInstanceName}, Connection:{ConnectionString}",
                    setting.InstanceName, setting.ConnectionString);

                Close(setting);

                Thread.Sleep(5000);

                return GetConnectionMultiplexer(setting, logger);
            }
        }

        private static void SetConnectionEvents(ConnectionMultiplexer connection, ILogger logger)
        {
            connection.ErrorMessage += (sender, e) =>
            {
                logger.LogError("Redis Connection ErrorMessage. Message:{Message}, Endpoint:{EndPoint}", e.Message, e.EndPoint);
            };

            connection.InternalError += (sernder, e) =>
            {
                logger.LogError(e.Exception, "Redis Connection InternalError. ConnectionType: {ConnectionType}, Endpoint:{EndPoint}, Origin:{Origin}",
                    e.ConnectionType, e.EndPoint, e.Origin);
            };
        }

        private static async Task<ConnectionMultiplexer> GetConnectionMultiplexerAsync(RedisInstanceSetting setting, ILogger logger)
        {
            if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer? cached))
            {
                return cached;
            }

            try
            {
                await _connectionLock.WaitAsync().ConfigureAwait(false);

                //Double check
                if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer? cached2))
                {
                    return cached2;
                }

                ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(setting.ConnectionString);

                //TODO: 调查参数
                //configurationOptions.KeepAlive = 30;
                //configurationOptions.ConnectTimeout = 10 * 1000;
                //configurationOptions.SyncTimeout = 100 * 1000;
                //configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);

                ConnectionMultiplexer connection = null!;

                await AsyncReConnectPolicy(setting.ConnectionString, logger).ExecuteAsync(async () =>
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions).ConfigureAwait(false);

                    _connectionDict[setting.InstanceName] = connection;

                    SetConnectionEvents(connection, logger);

                    logger.LogInformation("Redis 链接建立 Connected : {CacheInstanceName}", setting.InstanceName);
                }).ConfigureAwait(false);

                _connectionLock.Release();

                return connection;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _connectionLock.Release();

                logger.LogCritical(ex, "Redis Database获取失败.尝试重新获取。 SettingName:{CacheInstanceName}, Connection:{ConnectionString}",
                    setting.InstanceName, setting.ConnectionString);

                Close(setting);

                await Task.Delay(5000).ConfigureAwait(false);

                return await GetConnectionMultiplexerAsync(setting, logger).ConfigureAwait(false);
            }
        }

        public static IDatabase GetDatabase(RedisInstanceSetting setting, ILogger logger)
        {
            //StackExchange.Redis会在GetDatabase缓存，所以这里我们不用缓存IDatabase
            return GetConnectionMultiplexer(setting, logger).GetDatabase(setting.DatabaseNumber);
        }

        public static async Task<IDatabase> GetDatabaseAsync(RedisInstanceSetting setting, ILogger logger)
        {
            ConnectionMultiplexer connection = await GetConnectionMultiplexerAsync(setting, logger).ConfigureAwait(false);

            return connection.GetDatabase(setting.DatabaseNumber);
        }

        public static IServer GetServer(RedisInstanceSetting setting, ILogger logger)
        {
            ConnectionMultiplexer connection = GetConnectionMultiplexer(setting, logger);

            return connection.GetServer(setting.ServerEndPoint);
        }

        public static async Task<IServer> GetServerAsync(RedisInstanceSetting setting, ILogger logger)
        {
            ConnectionMultiplexer connection = await GetConnectionMultiplexerAsync(setting, logger).ConfigureAwait(false);

            return connection.GetServer(setting.ServerEndPoint);
        }

        private static AsyncRetryPolicy AsyncReConnectPolicy(string connectionString, ILogger logger)
        {
            //TODO: move this settings to options
            return Policy
                        .Handle<RedisConnectionException>()
                        .WaitAndRetryForeverAsync(
                            count => TimeSpan.FromSeconds(5),
                            (exception, retryCount, timeSpan) =>
                            {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                logger.LogCritical(exception, "Redis 建立链接时失败 Connection lost. Try {RetryCount}th times. Wait For {WaitTime} seconds. Redis Can not connect {ConnectionString}",
                                    retryCount, timeSpan.TotalSeconds, connectionString);
                            });
        }

        private static RetryPolicy ReConnectPolicy(string connectionString, ILogger logger)
        {
            //TODO: move this settings to options
            return Policy
                        .Handle<RedisConnectionException>()
                        .WaitAndRetryForever(
                            count => TimeSpan.FromSeconds(5),
                            (exception, retryCount, timeSpan) =>
                            {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                logger.LogCritical(exception, "Redis 建立链接时失败 Connection lost. Try {RetryCount}th times. Wait For {WaitTime} seconds. Redis Can not connect {ConnectionString}",
                                    retryCount, timeSpan.TotalSeconds, connectionString);
                            });
        }

        public static void Close(RedisInstanceSetting setting)
        {
            if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer? connection))
            {
                connection.Dispose();
                _connectionDict.TryRemove(setting.InstanceName, out ConnectionMultiplexer _);
            }
        }

        public static void CloseAll()
        {
            foreach (var kv in _connectionDict)
            {
                kv.Value?.Dispose();
                _connectionDict.TryRemove(kv.Key, out ConnectionMultiplexer _);
            }

            _connectionDict = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        }
    }
}
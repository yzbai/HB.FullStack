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

        public static ConnectionMultiplexer GetConnectionMultiplexer(RedisInstanceSetting setting)
        {
            if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer cached))
            {
                return cached;
            }

            try
            {
                _connectionLock.Wait();

                //Double check
                if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer cached2))
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

                ReConnectPolicy(setting.ConnectionString).Execute(() =>
                {
                    connection = ConnectionMultiplexer.Connect(configurationOptions);

                    _connectionDict[setting.InstanceName] = connection;

                    SetConnectionEvents(connection);

                    GlobalSettings.Logger.LogInformation($"Redis 链接建立 Connected : {setting.InstanceName}");

                });

                _connectionLock.Release();

                return connection;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _connectionLock.Release();

                GlobalSettings.Logger.LogCritical(ex, $"Redis Database获取失败.尝试重新获取。 SettingName:{setting.InstanceName}, Connection:{setting.ConnectionString}");

                Close(setting);

                Thread.Sleep(5000);

                return GetConnectionMultiplexer(setting);
            }
        }

        private static void SetConnectionEvents(ConnectionMultiplexer connection)
        {
            connection.ErrorMessage += (sender, e) =>
            {
                GlobalSettings.Logger.LogError($"message:{e.Message}, endpoint:{e.EndPoint}");

            };

            connection.InternalError += (sernder, e) =>
            {
                GlobalSettings.Logger.LogError(e.Exception, $"{e.ConnectionType}, {e.EndPoint}, {e.Origin}");
            };
        }

        public static async Task<ConnectionMultiplexer> GetConnectionMultiplexerAsync(RedisInstanceSetting setting)
        {
            if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer cached))
            {
                return cached;
            }

            try
            {
                await _connectionLock.WaitAsync().ConfigureAwait(false);

                //Double check
                if (_connectionDict.TryGetValue(setting.InstanceName, out ConnectionMultiplexer cached2))
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

                await AsyncReConnectPolicy(setting.ConnectionString).ExecuteAsync(async () =>
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(configurationOptions).ConfigureAwait(false);

                    _connectionDict[setting.InstanceName] = connection;

                    SetConnectionEvents(connection);

                    GlobalSettings.Logger.LogInformation($"Redis 链接建立 Connected : {setting.InstanceName}");
                }).ConfigureAwait(false);

                _connectionLock.Release();

                return connection;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _connectionLock.Release();

                GlobalSettings.Logger.LogCritical(ex, $"Redis Database获取失败.尝试重新获取。 SettingName:{setting.InstanceName}, Connection:{setting.ConnectionString}");

                Close(setting);

                await Task.Delay(5000).ConfigureAwait(false);

                return await GetConnectionMultiplexerAsync(setting).ConfigureAwait(false);
            }
        }

        public static IDatabase GetDatabase(RedisInstanceSetting setting)
        {
            //StackExchange.Redis会在GetDatabase缓存，所以这里我们不用缓存IDatabase
            return GetConnectionMultiplexer(setting).GetDatabase(setting.DatabaseNumber);
        }

        public static async Task<IDatabase> GetDatabaseAsync(RedisInstanceSetting setting)
        {
            ConnectionMultiplexer connection = await GetConnectionMultiplexerAsync(setting).ConfigureAwait(false);

            return connection.GetDatabase(setting.DatabaseNumber);
        }

        public static IServer GetServer(RedisInstanceSetting setting)
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(setting.ConnectionString);

            ConnectionMultiplexer connection = GetConnectionMultiplexer(setting);

            return connection.GetServer(options.EndPoints[0]);
        }

        public static async Task<IServer> GetServerAsync(RedisInstanceSetting setting)
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(setting.ConnectionString);

            ConnectionMultiplexer connection = await GetConnectionMultiplexerAsync(setting).ConfigureAwait(false);

            return connection.GetServer(options.EndPoints[0]);
        }

        private static AsyncRetryPolicy AsyncReConnectPolicy(string connectionString)
        {
            //TODO: move this settings to options
            return Policy
                        .Handle<RedisConnectionException>()
                        .WaitAndRetryForeverAsync(
                            count => TimeSpan.FromSeconds(5),
                            (exception, retryCount, timeSpan) =>
                            {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                GlobalSettings.Logger.LogCritical(exception, $"Redis 建立链接时失败 Connection lost. Try {retryCount}th times. Wait For {timeSpan.TotalSeconds} seconds. Redis Can not connect {connectionString}");
                            });
        }

        private static RetryPolicy ReConnectPolicy(string connectionString)
        {
            //TODO: move this settings to options
            return Policy
                        .Handle<RedisConnectionException>()
                        .WaitAndRetryForever(
                            count => TimeSpan.FromSeconds(5),
                            (exception, retryCount, timeSpan) =>
                            {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                GlobalSettings.Logger.LogCritical(exception, $"Redis 建立链接时失败 Connection lost. Try {retryCount}th times. Wait For {timeSpan.TotalSeconds} seconds. Redis Can not connect {connectionString}");
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

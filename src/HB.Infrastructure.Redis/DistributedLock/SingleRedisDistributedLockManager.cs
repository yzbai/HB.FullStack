using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Cache;
using HB.Framework.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.DistributedLock
{
    internal class SingleRedisDistributedLockManager : IDistributedLockManager
    {

        //TODO: 清理Log


        /// <summary>
        /// keys: resource1, resource2, resource3
        /// argv: 3(resource_count), expire_milliseconds, resource1_value, resource2_value, resource3_value
        /// </summary>
        private const string _luaLock = @"
if(redis.call('exists', unpack(KEYS)) ~= 0) then
	return 0
end

local count = tonumber(ARGV[1])

for i = 1,count do
    redis.call('set', KEYS[i], ARGV[i + 2], 'PX', ARGV[2])
end

return 1";

        /// <summary>
        /// keys: resource1,resource2,resource3
        /// argv:3(resource_count), resource1_value, resource2_value, resource3_value
        /// </summary>
        private const string _luaUnlock = @"
local count = tonumber(ARGV[1])
local ok = 1
for i = 1, count do
	if (redis.call('get', KEYS[i]) == ARGV[i+1]) then
		if (redis.pcall('del', KEYS[i]) ~= 1) then
            ok = 0
        end
	end
end
return ok
";

        /// <summary>
        /// keys:resource1,resource2,resource3
        /// argv:3(resource_count),expire_milliseconds, resource1_value, resource2_value, resource3_value
        /// </summary>
        private const string _luaExtend = @"
local count = tonumber(ARGV[1])

for i =1, count do
    if (redis.call('get', KEYS[i]) ~= ARGV[i+2]) then
        return 0
    end
end

for i =1, count do
    redis.call('set', KEYS[i], ARGV[i+2], 'PX', ARGV[2])
end
return 1";

        private readonly TimeSpan minimumExpiryTime = TimeSpan.FromMilliseconds(10);
        private readonly TimeSpan minimumRetryTime = TimeSpan.FromMilliseconds(10);

        private readonly SingleRedisDistributedLockOptions _options;
        private readonly ILogger _logger;

        private static LoadedLuas _loadedLuas = null!;

        public SingleRedisDistributedLockManager(IOptions<SingleRedisDistributedLockOptions> options, ILogger<SingleRedisDistributedLockManager> logger)
        {
            _options = options.Value;
            _logger = logger;

            InitLoadedLuas(_options.ConnectionSetting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="expiryTime">对资源的最大占用时间，应该大于TimeSpan.Zero, null表示使用默认</param>
        /// <param name="waitTime">如果资源被占用，你愿意等多久，TimeSpan.Zero表明不愿意等。null表示使用默认等待时间</param>
        /// <param name="retryInterval">等待时不断尝试获取资源 的 等待间隔，应该大于TimeSpan.Zero, null 表示使用默认时间</param>
        /// <returns></returns>
        public async Task<IDistributedLock> LockAsync(IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan? waitTime, TimeSpan? retryInterval, CancellationToken? cancellationToken = null)
        {
            if (expiryTime < minimumExpiryTime)
            {
                _logger.LogWarning($"Expiry time {expiryTime.TotalMilliseconds}ms too low, setting to {minimumExpiryTime.TotalMilliseconds}ms");
                expiryTime = minimumExpiryTime;
            }

            if (retryInterval != null && retryInterval.Value < minimumRetryTime)
            {
                _logger.LogWarning($"Retry time {retryInterval.Value.TotalMilliseconds}ms too low, setting to {minimumRetryTime.TotalMilliseconds}ms");
                retryInterval = minimumRetryTime;
            }

            RedisLock redisLock = new RedisLock(
                _options,
                resources,
                expiryTime,
                waitTime ?? TimeSpan.FromMilliseconds(_options.DefaultWaitMilliseconds),
                retryInterval ?? TimeSpan.FromMilliseconds(_options.DefaultRetryIntervalMilliseconds),
                cancellationToken);

            await StartAsync(redisLock, _logger).ConfigureAwait(false);

            return redisLock;
        }

        private static async Task StartAsync(RedisLock redisLock, ILogger logger)
        {
            if (redisLock.WaitTime != TimeSpan.Zero)
            {
                redisLock.Status = DistributedLockStatus.Waiting;

                Stopwatch stopwatch = Stopwatch.StartNew();

                while (!redisLock.IsAcquired && stopwatch.Elapsed <= redisLock.WaitTime)
                {
                    logger.LogDebug($"锁在等待... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");

                    redisLock.Status = await AcquireResourceAsync(redisLock, logger).ConfigureAwait(false);

                    if (!redisLock.IsAcquired)
                    {
                        logger.LogDebug($"锁继续等待... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
                        if (redisLock.CancellationToken == null)
                        {
                            await Task.Delay((int)redisLock.RetryTime.TotalMilliseconds).ConfigureAwait(false);
                        }
                        else
                        {
                            await Task.Delay((int)redisLock.RetryTime.TotalMilliseconds, redisLock.CancellationToken.Value).ConfigureAwait(false);
                        }
                    }
                }

                if (!redisLock.IsAcquired)
                {
                    logger.LogDebug($"锁等待超时... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
                    redisLock.Status = DistributedLockStatus.Expired;
                }

                stopwatch.Stop();
            }
            else
            {
                //不等待
                redisLock.Status = await AcquireResourceAsync(redisLock, logger).ConfigureAwait(false);
            }

            if (redisLock.IsAcquired)
            {
                logger.LogDebug($"锁获取成功... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
                StartAutoExtendTimer(redisLock, logger);
            }
            else
            {
                logger.LogDebug($"锁获取失败... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
            }
        }

        private static async Task<DistributedLockStatus> AcquireResourceAsync(RedisLock redisLock, ILogger logger)
        {
            if (redisLock.CancellationToken.HasValue && redisLock.CancellationToken.Value.IsCancellationRequested)
            {
                redisLock.Status = DistributedLockStatus.Cancelled;
                redisLock.CancellationToken.Value.ThrowIfCancellationRequested();
            }

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            AddAcquireOrExtendRedisInfo(redisLock, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(redisLock.Options.ConnectionSetting).ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    _loadedLuas.LoadedLockLua,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                int rt = (int)result;

                return rt == 1 ? DistributedLockStatus.Acquired : DistributedLockStatus.Failed;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas(redisLock.Options.ConnectionSetting);

                return await AcquireResourceAsync(redisLock, logger).ConfigureAwait(false);
            }
        }

        private static void StartAutoExtendTimer(RedisLock redisLock, ILogger logger)
        {
            var interval = redisLock.ExpiryTime.TotalMilliseconds / 2;

            redisLock.KeepAliveTimer = new Timer(
                state => { ExtendLockLifetime(redisLock, logger); },
                null,
                (int)interval,
                (int)interval);
        }

        private static void ExtendLockLifetime(RedisLock redisLock, ILogger logger)
        {
            logger.LogDebug($"锁在自动延期... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");

            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            AddAcquireOrExtendRedisInfo(redisLock, redisKeys, redisValues);

            IDatabase database = GetDatabase(redisLock.Options.ConnectionSetting);

            try
            {
                RedisResult result = database.ScriptEvaluate(
                    _loadedLuas.LoadedExtendLua,
                    redisKeys.ToArray(),
                    redisValues.ToArray());

                int rt = (int)result;

                if (rt != 1)
                {
                    logger.LogError("RedisLock Extend Failed.");
                    return;
                }

                redisLock.ExtendCount++;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas(redisLock.Options.ConnectionSetting);

                ExtendLockLifetime(redisLock, logger);
            }
        }

        internal static async Task ReleaseResourceAsync(RedisLock redisLock)
        {
            List<RedisKey> redisKeys = new List<RedisKey>();
            List<RedisValue> redisValues = new List<RedisValue>();

            AddReleaseResourceRedisInfo(redisLock, redisKeys, redisValues);

            IDatabase database = await GetDatabaseAsync(redisLock.Options.ConnectionSetting).ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    _loadedLuas.LoadedUnLockLua,
                    redisKeys.ToArray(),
                    redisValues.ToArray()).ConfigureAwait(false);

                int rt = (int)result;

                if (rt == 1)
                {
                    GlobalSettings.Logger.LogDebug($"锁已经解锁... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
                }
                else
                {
                    ThrowIfUnlockFailed(redisLock);
                }
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                InitLoadedLuas(redisLock.Options.ConnectionSetting);

                await ReleaseResourceAsync(redisLock).ConfigureAwait(false);
            }
            catch
            {
                GlobalSettings.Logger.LogDebug($"锁解锁失败... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
                throw;
            }
        }

        private static void ThrowIfUnlockFailed(RedisLock redisLock)
        {
            throw new FrameworkException(ErrorCode.DistributedLockUnLockFailed, $"锁解锁失败... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
        }

        private static void AddReleaseResourceRedisInfo(RedisLock redisLock, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys: resource1,resource2,resource3
            /// argv:3(resource_count), resource1_value, resource2_value, resource3_value

            foreach (string item in redisLock.Resources)
            {
                redisKeys.Add(item);
            }

            redisValues.Add(redisKeys.Count);

            foreach (string item in redisLock.ResourceValues)
            {
                redisValues.Add(item);
            }
        }

        private static void AddAcquireOrExtendRedisInfo(RedisLock redisLock, List<RedisKey> redisKeys, List<RedisValue> redisValues)
        {
            /// keys: resource1, resource2, resource3
            /// argv: 3(resource_count), expire_milliseconds, resource1_value, resource2_value, resource3_value

            foreach (string item in redisLock.Resources)
            {
                redisKeys.Add(item);
            }

            redisValues.Add(redisKeys.Count);
            redisValues.Add((int)redisLock.ExpiryTime.TotalMilliseconds);

            foreach (string item in redisLock.ResourceValues)
            {
                redisValues.Add(item);
            }
        }

        internal static void InitLoadedLuas(RedisInstanceSetting redisInstanceSetting)
        {
            IServer server = RedisInstanceManager.GetServer(redisInstanceSetting);

            _loadedLuas = new LoadedLuas();

            _loadedLuas.LoadedLockLua = server.ScriptLoad(_luaLock);
            _loadedLuas.LoadedUnLockLua = server.ScriptLoad(_luaUnlock);
            _loadedLuas.LoadedExtendLua = server.ScriptLoad(_luaExtend);
        }

        internal static async Task<IDatabase> GetDatabaseAsync(RedisInstanceSetting redisInstanceSetting)
        {
            return await RedisInstanceManager.GetDatabaseAsync(redisInstanceSetting).ConfigureAwait(false);
        }

        internal static IDatabase GetDatabase(RedisInstanceSetting redisInstanceSetting)
        {
            return RedisInstanceManager.GetDatabase(redisInstanceSetting);
        }
    }
}

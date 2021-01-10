using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Lock;
using HB.FullStack.Lock.Distributed;
using HB.Infrastructure.Redis.Shared;

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

		private readonly TimeSpan _minimumExpiryTime = TimeSpan.FromMilliseconds(10);
		private readonly TimeSpan _minimumRetryTime = TimeSpan.FromMilliseconds(10);

		private readonly SingleRedisDistributedLockOptions _options;
		private readonly ILogger _logger;

		private static LoadedLuas _loadedLuas = null!;

		public SingleRedisDistributedLockManager(IOptions<SingleRedisDistributedLockOptions> options, ILogger<SingleRedisDistributedLockManager> logger)
		{
			_options = options.Value;
			_logger = logger;

			InitLoadedLuas(_options.ConnectionSetting, _logger);

			_logger.LogInformation($"SingleRedisDistributedLockManager初始化完成");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resources"></param>
		/// <param name="expiryTime">对资源的最大占用时间，应该大于TimeSpan.Zero, null表示使用默认</param>
		/// <param name="waitTime">如果资源被占用，你愿意等多久，TimeSpan.Zero表明不愿意等。null表示使用默认等待时间</param>
		/// <param name="retryInterval">等待时不断尝试获取资源 的 等待间隔，应该大于TimeSpan.Zero, null 表示使用默认时间</param>
		/// <returns></returns>
		public async Task<IDistributedLock> LockAsync(IEnumerable<string> resources, TimeSpan expiryTime, TimeSpan? waitTime, TimeSpan? retryInterval, bool notUnlockWhenDispose = false, CancellationToken? cancellationToken = null)
		{
			if (expiryTime < _minimumExpiryTime)
			{
				_logger.LogWarning($"Expiry time {expiryTime.TotalMilliseconds}ms too low, setting to {_minimumExpiryTime.TotalMilliseconds}ms");
				expiryTime = _minimumExpiryTime;
			}

			if (retryInterval != null && retryInterval.Value < _minimumRetryTime)
			{
				_logger.LogWarning($"Retry time {retryInterval.Value.TotalMilliseconds}ms too low, setting to {_minimumRetryTime.TotalMilliseconds}ms");
				retryInterval = _minimumRetryTime;
			}

			RedisLock redisLock = new RedisLock(
				_options,
				_logger,
				resources,
				expiryTime,
				waitTime ?? TimeSpan.FromMilliseconds(_options.DefaultWaitMilliseconds),
				retryInterval ?? TimeSpan.FromMilliseconds(_options.DefaultRetryIntervalMilliseconds),
				notUnlockWhenDispose,
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

			AddAcquireOrExtendRedisInfo(redisLock, redisKeys, redisValues, logger);

			IDatabase database = await GetDatabaseAsync(redisLock.Options.ConnectionSetting, logger).ConfigureAwait(false);

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

				InitLoadedLuas(redisLock.Options.ConnectionSetting, logger);

				return await AcquireResourceAsync(redisLock, logger).ConfigureAwait(false);
			}
		}

		private static void StartAutoExtendTimer(RedisLock redisLock, ILogger logger)
		{
			long interval = (long)redisLock.ExpiryTime.TotalMilliseconds / 2;

			redisLock.KeepAliveTimer = new Timer(
				state => { ExtendLockLifetime(redisLock, logger); },
				null,
				interval,
				interval);
		}

		private static void ExtendLockLifetime(RedisLock redisLock, ILogger logger)
		{
			if (redisLock.Status != DistributedLockStatus.Acquired)
			{
				logger.LogDebug($"锁已不是获取状态，停止自动延期... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}, Status:{redisLock.Status}");
				return;
			}

			logger.LogDebug($"锁在自动延期... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");

			List<RedisKey> redisKeys = new List<RedisKey>();
			List<RedisValue> redisValues = new List<RedisValue>();

			AddAcquireOrExtendRedisInfo(redisLock, redisKeys, redisValues, logger);

			IDatabase database = GetDatabase(redisLock.Options.ConnectionSetting, logger);

			try
			{
				RedisResult result = database.ScriptEvaluate(
					_loadedLuas.LoadedExtendLua,
					redisKeys.ToArray(),
					redisValues.ToArray());

				int rt = (int)result;

				if (rt != 1)
				{
					logger.LogError($"RedisLock 延期 失败. Resources:{redisLock.Resources.ToJoinedString(",")}, Status:{redisLock.Status}");
					return;
				}

				redisLock.ExtendCount++;
			}
			catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
			{
				logger.LogError(ex, "NOSCRIPT, will try again.");

				InitLoadedLuas(redisLock.Options.ConnectionSetting, logger);

				ExtendLockLifetime(redisLock, logger);
			}
		}

		private static void StopKeepAliveTimer(RedisLock redisLock, ILogger logger)
		{
			if (redisLock.KeepAliveTimer != null)
			{
				lock (redisLock.StopKeepAliveTimerLockObj)
				{
					if (redisLock.KeepAliveTimer != null)
					{
						redisLock.KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
						redisLock.KeepAliveTimer.Dispose();
						redisLock.KeepAliveTimer = null;

						logger.LogDebug($"锁停止自动延期，Resources:{redisLock.Resources.ToJoinedString(",")}");
					}
				}
			}
		}

		/// <summary>
		/// ReleaseResourceAsync
		/// </summary>
		/// <param name="redisLock"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		/// <exception cref="LockException"></exception>
		internal static async Task ReleaseResourceAsync(RedisLock redisLock, ILogger logger)
		{
			StopKeepAliveTimer(redisLock, logger);

			if (redisLock.NotUnlockWhenDispose)
			{
				logger.LogDebug($"自动延期停止,但锁等他自己过期... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
				return;
			}

			List<RedisKey> redisKeys = new List<RedisKey>();
			List<RedisValue> redisValues = new List<RedisValue>();

			AddReleaseResourceRedisInfo(redisLock, redisKeys, redisValues);

			IDatabase database = await GetDatabaseAsync(redisLock.Options.ConnectionSetting, logger).ConfigureAwait(false);

			try
			{
				RedisResult result = await database.ScriptEvaluateAsync(
					_loadedLuas.LoadedUnLockLua,
					redisKeys.ToArray(),
					redisValues.ToArray()).ConfigureAwait(false);

				int rt = (int)result;

				if (rt == 1)
				{
					logger.LogDebug($"锁已经解锁... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
				}
				else
				{
					ThrowIfUnlockFailed(redisLock);
				}
			}
			catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
			{
				InitLoadedLuas(redisLock.Options.ConnectionSetting, logger);

				await ReleaseResourceAsync(redisLock, logger).ConfigureAwait(false);
			}
			catch(Exception ex)
			{
				string message = $"锁解锁失败... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}";
				logger.LogDebug(message);

				throw new LockException(LockErrorCode.DistributedLockUnLockFailed, message, ex);
			}
		}

		/// <summary>
		/// ThrowIfUnlockFailed
		/// </summary>
		/// <param name="redisLock"></param>
		/// <exception cref="LockException"></exception>
		private static void ThrowIfUnlockFailed(RedisLock redisLock)
		{
			throw new LockException(LockErrorCode.DistributedLockUnLockFailed, $"锁解锁失败... ThreadID: {Thread.CurrentThread.ManagedThreadId}, Resources:{redisLock.Resources.ToJoinedString(",")}");
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

		private static void AddAcquireOrExtendRedisInfo(RedisLock redisLock, List<RedisKey> redisKeys, List<RedisValue> redisValues, ILogger logger)
		{
			/// keys: resource1, resource2, resource3
			/// argv: 3(resource_count), expire_milliseconds, resource1_value, resource2_value, resource3_value

			//有可能到这里，dispose了，redisLock.Resources都为空


			try
			{
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
			catch (NullReferenceException ex)
			{
				logger.LogError(ex, $"在试图延长锁的时候，ResourceValues被清空. Resources:{redisLock.Resources.ToJoinedString(",")}, Status:{redisLock.Status}");
			}
		}

		internal static void InitLoadedLuas(RedisInstanceSetting redisInstanceSetting, ILogger logger)
		{
			IServer server = RedisInstanceManager.GetServer(redisInstanceSetting, logger);

			_loadedLuas = new LoadedLuas
			{
				LoadedLockLua = server.ScriptLoad(_luaLock),
				LoadedUnLockLua = server.ScriptLoad(_luaUnlock),
				LoadedExtendLua = server.ScriptLoad(_luaExtend)
			};
		}

		internal static async Task<IDatabase> GetDatabaseAsync(RedisInstanceSetting redisInstanceSetting, ILogger logger)
		{
			return await RedisInstanceManager.GetDatabaseAsync(redisInstanceSetting, logger).ConfigureAwait(false);
		}

		internal static IDatabase GetDatabase(RedisInstanceSetting redisInstanceSetting, ILogger logger)
		{
			return RedisInstanceManager.GetDatabase(redisInstanceSetting, logger);
		}
	}
}

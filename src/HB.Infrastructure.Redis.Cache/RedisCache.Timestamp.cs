using HB.FullStack.Cache;
using HB.FullStack.Common;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.Cache
{
    /// <summary>
    /// key-value的Timestamp构型
    /// </summary>
    public partial class RedisCache
    {
        // KEYS[1] = = key
        // ARGV[1] = absolute-expiration - unix time seconds as long (null for none)
        // ARGV[2] = sliding-expiration - seconds  as long (null for none)
        // ARGV[3] = ttl seconds 当前过期要设置的过期时间，由上面两个推算
        // ARGV[4] = data - byte[]
        // ARGV[5] = utcTicks
        // this order should not change LUA script depends on it
        public const string LUA_SET_WITH_TIMESTAMP = @"
local minTimestamp = redis.call('get', '_minTS'..KEYS[1])

if(minTimestamp and tonumber(minTimestamp)>tonumber(ARGV[5])) then
    return 8
end

local cachedTimestamp = redis.call('hget', KEYS[1], 'timestamp')
if(cachedTimestamp and tonumber(cachedTimestamp)>tonumber(ARGV[5])) then
    return 9
end

redis.call('hmset', KEYS[1],'absexp',ARGV[1],'sldexp',ARGV[2],'data',ARGV[4], 'timestamp', ARGV[5])

if(ARGV[3]~='-1') then
    redis.call('expire',KEYS[1], ARGV[3])
end

return 1";

        /// <summary>
        /// keys: key
        /// argv:utcTicks, invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_REMOVE_WITH_TIMESTAMP = @"
redis.call('set', '_minTS'..KEYS[1], ARGV[1], 'EX', ARGV[2])
return redis.call('del', KEYS[1])
";

        /// <summary>
        /// keys: key1, key2, key3
        /// argv: key_count, utcTicks, invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_REMOVE_MULTIPLE_WITH_TIMESTAMP = @"
local number=tonumber(ARGV[1])
for i=1,number do
    redis.call('set', '_minTS'..KEYS[i], ARGV[2], 'EX', ARGV[3])
end
return redis.call('del', unpack(KEYS))";

        /// <summary>
        /// keys:key
        /// argv:utcTicks
        /// </summary>
        public const string LUA_GET_AND_REFRESH_WITH_TIMESTAMP = @"
local data= redis.call('hmget',KEYS[1], 'absexp', 'sldexp','data')

if (not data[3]) then
    return nil
end

if(data[1]~='-1') then
    local now = tonumber(ARGV[1])
    local absexp = tonumber(data[1])
    if(now>=absexp) then
        redis.call('del', KEYS[1])
        return nil
    end
end

if(data[2]~='-1') then
    redis.call('expire', KEYS[1], data[2])
end

return data[3]";

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    GetDefaultLoadLuas().LoadedGetAndRefreshLua,
                    new RedisKey[] { GetRealKey("", key) },
                    new RedisValue[] { TimeUtil.UtcNowUnixTimeSeconds }).ConfigureAwait(false);

                return (byte[]?)result;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(GetAsync));

                InitLoadedLuas();

                return await GetAsync(key, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogCacheGetError(key, ex);

                AggregateException? aggregateException = null;

                try
                {
                    await RemoveAsync(key, TimeUtil.UtcNowTicks, token).ConfigureAwait(false);
                }
                catch (Exception ex2)
                {
                    aggregateException = new AggregateException(ex, ex2);
                }

                throw (Exception?)aggregateException ?? CacheExceptions.GetError(key, ex);
            }
        }

        /// <summary>
        /// SetAsync
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="utcTicks"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        public async Task<bool> SetAsync(string key, byte[] value, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                options.AbsoluteExpiration = TimeUtil.UtcNow + options.AbsoluteExpirationRelativeToNow;
            }

            long? absoluteExpireUnixSeconds = options.AbsoluteExpiration?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(options.SlidingExpiration?.TotalSeconds);

            try
            {
                RedisResult redisResult = await database.ScriptEvaluateAsync(GetDefaultLoadLuas().LoadedSetWithTimestampLua, new RedisKey[] { GetRealKey("", key) },
                    new RedisValue[]
                    {
                        absoluteExpireUnixSeconds??-1,
                        slideSeconds??-1,
                        GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds)??-1,
                        value,
                        utcTicks.Ticks
                    }).ConfigureAwait(false);

                int rt = (int)redisResult;

                if (rt == 1)
                {
                    return true;
                }
                else if (rt == 8)
                {
                    Logger.LogCacheInvalidationConcurrencyWithTimestamp(key, utcTicks, options);
                }
                else if (rt == 9)
                {
                    Logger.LogCacheUpdateTimestampConcurrency(key, utcTicks, options);
                }

                return false;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(SetAsync));

                InitLoadedLuas();

                return await SetAsync(key, value, utcTicks, options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.SetError(key, utcTicks, options, ex);
            }
        }

        /// <summary>
        /// 返回是否找到了
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timestampInUnixMilliseconds"></param>
        /// <param name="token"></param>
        /// <returns></returns>

        public async Task<bool> RemoveAsync(string key, UtcNowTicks utcTicks, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            try
            {
                RedisResult redisResult = await database.ScriptEvaluateAsync(
                    GetDefaultLoadLuas().LoadedRemoveWithTimestampLua,
                    new RedisKey[] { GetRealKey("", key) },
                    new RedisValue[]
                    {
                        utcTicks.Ticks,
                        INVALIDATION_VERSION_EXPIRY_SECONDS
                    }).ConfigureAwait(false);

                return (int)redisResult == 1;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(RemoveAsync));

                InitLoadedLuas();

                return await RemoveAsync(key, utcTicks, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.RemoveError(key, utcTicks, ex);
            }
        }

        public async Task<bool> RemoveAsync(string[] keys, UtcNowTicks utcTicks, CancellationToken token = default)
        {
            //TODO: 测试这个
            if (keys.IsNullOrEmpty())
            {
                return true;
            }

            token.ThrowIfCancellationRequested();

            //划分组 100个一组
            int groupLength = 100;

            IEnumerable<string[]> groups = keys.Chunk(groupLength);//PartitionToGroup(keys, groupLength);

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            int deletedSum = 0;

            try
            {
                foreach (string[] group in groups)
                {
                    RedisResult redisResult = await database.ScriptEvaluateAsync(
                        GetDefaultLoadLuas().LoadedRemoveMultipleWithTimestampLua,
                        group.Select(key => (RedisKey)GetRealKey("", key)).ToArray(),
                        new RedisValue[]
                        {
                            group.Length,
                            utcTicks.Ticks,
                            INVALIDATION_VERSION_EXPIRY_SECONDS
                        }).ConfigureAwait(false);

                    deletedSum += (int)redisResult;
                }

                return deletedSum == keys.Length;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(RemoveAsync));

                InitLoadedLuas();

                return await RemoveAsync(keys, utcTicks, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.RemoveMultipleError(keys, utcTicks, ex);
            }
        }

        public Task RemoveByKeyPrefixAsync(string keyPrefix, UtcNowTicks utcTikcs, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
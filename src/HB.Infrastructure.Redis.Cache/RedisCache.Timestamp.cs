﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

using StackExchange.Redis;

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
        // ARGV[5] = timestamp
        // this order should not change LUA script depends on it
        public const string LUA_SET_WITH_TIMESTAMP = @"
local minTimestamp = redis.call('get', '_minTS'..KEYS[1])

if(minTimestamp and tonumber(minTimestamp)>=tonumber(ARGV[5])) then
    return 8
end

local cachedTimestamp = redis.call('hget', KEYS[1], 'timestamp')
if(cachedTimestamp and tonumber(cachedTimestamp)>=tonumber(ARGV[5])) then
    return 9
end

redis.call('hmset', KEYS[1],'absexp',ARGV[1],'sldexp',ARGV[2],'data',ARGV[4], 'timestamp', ARGV[5])

if(ARGV[3]~='-1') then
    redis.call('expire',KEYS[1], ARGV[3])
end

return 1";

        /// <summary>
        /// keys: key
        /// argv:invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_REMOVE_2 = @"
local timestamp = redis.call('hget',KEYS[1], 'timestamp')
if (timestamp) then
    redis.call('set', '_minTS'..KEYS[1], timestamp, 'EX', ARGV[1])
end
return redis.call('del', KEYS[1])
";

        /// <summary>
        /// keys: key1, key2, key3
        /// argv: key_count, invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_REMOVE_MULTIPLE_2 = @"
local number=tonumber(ARGV[1])
for i=1,number do
    local timestamp = redis.call('hget', KEYS[i], 'timestamp')
    if(timestamp) then
        redis.call('set', '_minTS'..KEYS[i], timestamp, 'EX', ARGV[2])
    end
end
return redis.call('del', unpack(KEYS))";

        /// <summary>
        /// keys:key
        /// argv:UtcNowUnixTimeSeconds(用来比较刷新过期时间)
        /// </summary>
        public const string LUA_GET_AND_REFRESH = @"
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
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
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
                    await RemoveAsync(key, token).ConfigureAwait(false);
                }
                catch (Exception ex2)
                {
                    aggregateException = new AggregateException(ex, ex2);
                }

                throw (Exception?)aggregateException ?? CacheExceptions.GetError(key, ex);
            }
        }

        /// <summary>
        /// timestamp即ICacheModel.Timestamp
        /// </summary>
        public async Task<bool> SetAsync(string key, byte[] value, long timestamp, DistributedCacheEntryOptions options, CancellationToken token = default)
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
                RedisResult redisResult = await database.ScriptEvaluateAsync(GetDefaultLoadLuas().LoadedSetWithTimestampLua,
                    new RedisKey[] { GetRealKey("", key) },
                    new RedisValue[]
                    {
                        absoluteExpireUnixSeconds??-1,
                        slideSeconds??-1,
                        GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds)??-1,
                        value,
                        timestamp
                    }).ConfigureAwait(false);

                int rt = (int)redisResult;

                if (rt == 1)
                {
                    return true;
                }
                else if (rt == 8)
                {
                    Logger.LogCacheInvalidationConcurrencyWithTimestamp(key, timestamp, options);
                }
                else if (rt == 9)
                {
                    Logger.LogCacheUpdateTimestampConcurrency(key, timestamp, options);
                }

                return false;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(SetAsync));

                InitLoadedLuas();

                return await SetAsync(key, value, timestamp, options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.SetError(key, timestamp, options, ex);
            }
        }

        /// <summary>
        /// 返回是否找到了
        /// </summary>
        public async Task<bool> RemoveAsync(string key, CancellationToken token = default)
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
                    new RedisValue[] { MININAL_TIMESTAMP_LOCK_EXPIRY_SECONDS }
                    ).ConfigureAwait(false);

                return (int)redisResult == 1;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(RemoveAsync));

                InitLoadedLuas();

                return await RemoveAsync(key, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.RemoveError(key, ex);
            }
        }

        public async Task<bool> RemoveAsync(string[] keys, CancellationToken token = default)
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
                            MININAL_TIMESTAMP_LOCK_EXPIRY_SECONDS
                        }).ConfigureAwait(false);

                    deletedSum += (int)redisResult;
                }

                return deletedSum == keys.Length;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(RemoveAsync));

                InitLoadedLuas();

                return await RemoveAsync(keys, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.RemoveMultipleError(keys, ex);
            }
        }

        public Task RemoveByKeyPrefixAsync(string keyPrefix, long timestamp, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
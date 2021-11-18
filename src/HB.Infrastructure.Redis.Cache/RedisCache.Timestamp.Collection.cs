using Microsoft.Extensions.Caching.Distributed;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.Cache
{
    /// <summary>
    /// Collection构型。
    /// 一个Collection里，可以放各种条目，但没有自己的过期时间。
    /// 过期时间是整个Collection的。
    /// </summary>
    public partial class RedisCache
    {
        // KEYS = Collectionkey, key1, key2, key3

        // ARGV[1] = absolute-expiration - unix time seconds as long (null for none)
        // ARGV[2] = sliding-expiration - seconds  as long (null for none)
        // ARGV[3] = ttl seconds 当前过期要设置的过期时间，由上面两个推算
        // ARGV[4] = utcNowTicks
        // ARGV[5] = 3 (数据的个数)

        // ARGV[6] = key1_data
        // ARGV[7] = key2_data
        // ARGV[8] = key3_data

        // this order should not change LUA script depends on it
        public const string LUA_COLLECTION_SET_WITH_TIMESTAMP = @"
if(redis.call('exists', KEYS[1]) ~= 1) then
    redis.call('hmset', KEYS[1], '__absexp__', ARGV[1],'__sldexp__',ARGV[2])
    if(ARGV[3] ~='-1') then
        redis.call('expire', KEYS[1], ARGV[3])
    end
end

local minTS = '__minTS__'..KEYS[1]
local error = 0
local errorSum = 0
local dataNum = tonumber(ARGV[5])
for j=1, dataNum do
    local minTimestamp = redis.call('get', minTS..KEYS[j+1])

    if(minTimestamp and tonumber(minTimestamp)>tonumber(ARGV[4])) then
        error = 8
    end

    if(error == 0) then
        local cachedTimestamp = redis.call('hget', KEYS[1], KEYS[j+1]..'__ts__')
        if(cachedTimestamp and tonumber(cachedTimestamp) > tonumber(ARGV[4])) then
            error = 90000
        end
    end

    if(error == 0) then
        redis.call('hmset', KEYS[1], KEYS[j+1], ARGV[5+j], KEYS[j+1]..'__ts__', ARGV[4])
    else
        errorSum= errorSum+ errror
    end
end

return errorSum";

        /// <summary>
        /// keys: CollectionKey, key1, key2, key3
        /// argv: key_count, utcTicks, invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_COLLECTION_REMOVE_ITEM_WITH_TIMESTAMP = @"
local minTS = '__minTS__'..KEYS[1]
local number=tonumber(ARGV[1])
local delSum = 0
for j=1,number do
    redis.call('set', minTS..KEYS[j+1], ARGV[2], 'EX', ARGV[3])
    delSum = delSum + redis.call('hdel', KEYS[1], KEYS[j+1], KEYS[j+1]..'__ts__')
end

return delSum";

        /// <summary>
        /// keys:CollectionKey, key1
        /// argv:utcTicks
        /// </summary>
        public const string LUA_COLLECTION_GET_AND_REFRESH_WITH_TIMESTAMP = @"
local data= redis.call('hmget',KEYS[1], '__absexp__', '__sldexp__',KEYS[2])

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
        public async Task<byte[]?> GetFromCollectionAsync(string collectionKey, string itemKey, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(
                    GetDefaultLoadLuas().LoadedCollectionGetAndRefreshWithTimestampLua,
                    new RedisKey[] { GetRealKey("", collectionKey), itemKey },
                    new RedisValue[] { TimeUtil.UtcNowUnixTimeSeconds }).ConfigureAwait(false);

                return (byte[]?)result;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogLuaScriptNotLoaded(null, null, nameof(GetFromCollectionAsync));

                InitLoadedLuas();

                return await GetFromCollectionAsync(collectionKey, itemKey, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogCacheCollectionGetError(collectionKey, itemKey, ex);

                AggregateException? aggregateException = null;

                try
                {
                    await RemoveCollectionAsync(collectionKey, token).ConfigureAwait(false);
                }
                catch (Exception ex2)
                {
                    aggregateException = new AggregateException(ex, ex2);
                }

                throw (Exception?)aggregateException ?? CacheExceptions.GetError(collectionKey, ex);
            }
        }

        public async Task<bool> SetToCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, IEnumerable<byte[]> itemValues, UtcNowTicks utcTicks, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (itemKeys.Count() != itemValues.Count())
            {
                throw new ArgumentException("In SetToCollection, itemKeys count should equal vlaues count");
            }

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                options.AbsoluteExpiration = TimeUtil.UtcNow + options.AbsoluteExpirationRelativeToNow;
            }

            long? absoluteExpireUnixSeconds = options.AbsoluteExpiration?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(options.SlidingExpiration?.TotalSeconds);

            try
            {
                //Prepare RedisKeys
                int itemCount = itemKeys.Count();
                RedisKey[] redisKeys = new RedisKey[itemCount + 1];

                redisKeys[0] = GetRealKey("", collectionKey);

                for (int i = 0; i < itemCount; ++i)
                {
                    redisKeys[i + 1] = itemKeys.ElementAt(i);
                }

                //Prepare RedisValues
                RedisValue[] redisValues = new RedisValue[itemCount * 2 + 4];

                redisValues[0] = absoluteExpireUnixSeconds ?? -1;
                redisValues[1] = slideSeconds ?? -1;
                redisValues[2] = GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds) ?? -1;
                redisValues[3] = utcTicks.Ticks;
                redisValues[4] = itemCount;

                for (int i = 0; i < itemCount; ++i)
                {
                    redisValues[i + 5] = itemValues.ElementAt(i);
                }

                //Do
                RedisResult redisResult = await database.ScriptEvaluateAsync(
                    GetDefaultLoadLuas().LoadedCollectionSetWithTimestampLua,
                    redisKeys,
                    redisValues).ConfigureAwait(false);

                int rt = (int)redisResult;

                if (rt == 0)
                {
                    return true;
                }
                else if (rt < 90000)
                {
                    _logger.LogCacheInvalidationConcurrencyWithTimestamp(collectionKey, utcTicks, options);
                }
                else
                {
                    _logger.LogCacheUpdateTimestampConcurrency(collectionKey, utcTicks, options);
                }

                return false;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogLuaScriptNotLoaded(null, null, nameof(SetToCollectionAsync));

                InitLoadedLuas();

                return await SetToCollectionAsync(collectionKey, itemKeys, itemValues, utcTicks, options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.SetError(collectionKey, utcTicks, options, ex);
            }
        }

        public async Task<bool> RemoveFromCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, UtcNowTicks utcTicks, CancellationToken token = default)
        {
            //TODO: 测试这个

            token.ThrowIfCancellationRequested();

            //划分组 100个一组
            int groupLength = 100;

            IEnumerable<string[]> groups = itemKeys.Chunk(groupLength);//PartitionToGroup(keys, groupLength);

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            int deletedSum = 0;

            try
            {
                foreach (string[] group in groups)
                {
                    //Prepare RedisKeys
                    RedisKey[] redisKeys = new RedisKey[group.Length + 1];

                    redisKeys[0] = GetRealKey("", collectionKey);

                    for (int i = 0; i < itemKeys.Count(); ++i)
                    {
                        redisKeys[i + 1] = itemKeys.ElementAt(i);
                    }

                    //Prepare RedisValues
                    RedisValue[] redisValues = new RedisValue[]
                    {
                        group.Length,
                        utcTicks.Ticks,
                        INVALIDATION_VERSION_EXPIRY_SECONDS
                    };

                    RedisResult redisResult = await database.ScriptEvaluateAsync(
                        GetDefaultLoadLuas().LoadedCollectionRemoveItemWithTimestampLua,
                        redisKeys,
                        redisValues).ConfigureAwait(false);

                    deletedSum += (int)redisResult;
                }

                return deletedSum / 2 == itemKeys.Count();
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogLuaScriptNotLoaded(null, null, nameof(RemoveFromCollectionAsync));

                InitLoadedLuas();

                return await RemoveFromCollectionAsync(collectionKey, itemKeys, utcTicks, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.RemoveMultipleError(collectionKey, itemKeys, utcTicks, ex);
            }
        }

        public async Task<bool> RemoveCollectionAsync(string collectionKey, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            return await database.KeyDeleteAsync(collectionKey).ConfigureAwait(false);
        }
    }
}
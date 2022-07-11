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
    /// 构造：
    /// CollectionKey ------------| __absexp__
    ///               ------------| __sldexp__  
    ///               ------------| key1
    ///               ------------| key2
    ///               ------------| key3
    ///               ------------| key1__ts__
    ///               ------------| key2__ts__
    ///               ------------| key3__ts__
    /// 
    /// __minTS__CollectionKeykey1 -------- key1_minTimestamp
    /// __minTS__CollectionKeykey2 -------- key2_minTimestamp
    /// __minTS__CollectionKeykey3 -------- key3_minTimestamp
    /// </summary>
    public partial class RedisCache
    {
        // KEYS = Collectionkey, key1, key2, key3

        // ARGV[1] = absolute-expiration - unix time seconds as long (null for none)
        // ARGV[2] = sliding-expiration - seconds  as long (null for none)
        // ARGV[3] = ttl seconds 当前过期要设置的过期时间，由上面两个推算
        // ARGV[4] = 3 (数据的个数)

        // ARGV[5] = key1_data
        // ARGV[6] = key2_data
        // ARGV[7] = key3_data

        // ARGV[8] = key1_timestamp
        // ARGV[9] = key2_timestamp
        // ARGV[10] = key3_timestamp

        // this order should not change LUA script depends on it
        public const string LUA_COLLECTION_SET_WITH_TIMESTAMP_2 = @"
if(redis.call('exists', KEYS[1]) ~= 1) then
    redis.call('hmset', KEYS[1], '__absexp__', ARGV[1],'__sldexp__',ARGV[2])
    if(ARGV[3] ~='-1') then
        redis.call('expire', KEYS[1], ARGV[3])
    end
end

local minTS = '__minTS__'..KEYS[1]
local dataNum = tonumber(ARGV[4])

for j=1, dataNum do
    local minTimestamp = redis.call('get', minTS..KEYS[j+1])
    if( minTimestamp and tonumber(minTimestamp)>=tonumber(ARGV[j+7])) then
        return 8
    end

    local cachedTimestamp = redis.call('hget', KEYS[1], KEYS[j+1], KEYS[j+1]..'__ts__')
    if(cachedTimestamp and tonumber(cachedTimestamp) >= tonumber(ARGV[j+7])) then
        return 9
    end
end

for j=1, dataNum do
    redis.call('hmset', KEYS[1], KEYS[j+1], ARGV[j+4], KEYS[j+1]..'__ts__', ARGV[j+7])
end
return 1";

        /// <summary>
        /// keys: CollectionKey, key1, key2, key3
        /// argv: key_count, invalidationKey_expire_seconds
        /// </summary>
        public const string LUA_COLLECTION_REMOVE_ITEMS_WITH_TIMESTAMP_2 = @"
local minTS = '__minTS__'..KEYS[1]
local number=tonumber(ARGV[1])

for j=1,number do
    local cachedTimestamp = redis.call('hget', KEYS[1], KEYS[j+1], KEYS[j+1]..'__ts__')
    redis.call('set', minTS..KEYS[j+1], cachedTimestamp, 'EX', ARGV[2])
    redis.call('hdel', KEYS[1], KEYS[j+1], KEYS[j+1]..'__ts__')
end

return 1";

        /// <summary>
        /// keys:CollectionKey, key1
        /// argv:UtcNowUnixTimeSeconds(now) 为了刷新
        /// </summary>
        public const string LUA_COLLECTION_GET_AND_REFRESH_WITH_TIMESTAMP_2 = @"
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
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(GetFromCollectionAsync));

                InitLoadedLuas();

                return await GetFromCollectionAsync(collectionKey, itemKey, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogCacheCollectionGetError(collectionKey, itemKey, ex);

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

        public async Task<bool> SetToCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, IEnumerable<byte[]> itemValues, IEnumerable<long> timestamps, DistributedCacheEntryOptions options, CancellationToken token = default)
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
                List<RedisValue> redisValues = new List<RedisValue>(itemCount * 2 + 4)
                {
                    [0] = absoluteExpireUnixSeconds ?? -1,
                    [1] = slideSeconds ?? -1,
                    [2] = GetInitialExpireSeconds(absoluteExpireUnixSeconds, slideSeconds) ?? -1,
                    [3] = itemCount
                };

                foreach (var itemValue in itemValues)
                {
                    redisValues.Add(itemValue);
                }

                foreach (var timestamp in timestamps)
                {
                    redisValues.Add(timestamp);
                }

                //Do
                RedisResult redisResult = await database.ScriptEvaluateAsync(
                    GetDefaultLoadLuas().LoadedCollectionSetWithTimestampLua,
                    redisKeys,
                    redisValues.ToArray()).ConfigureAwait(false);

                int rt = (int)redisResult;

                if (rt == 1)
                {
                    return true;
                }
                else if (rt == 8)
                {
                    Logger.LogCacheInvalidationConcurrencyWithTimestamp(collectionKey, -1, options);
                }
                else
                {
                    Logger.LogCacheUpdateTimestampConcurrency(collectionKey, -1, options);
                }

                return false;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(SetToCollectionAsync));

                InitLoadedLuas();

                return await SetToCollectionAsync(collectionKey, itemKeys, itemValues, timestamps, options, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.SetError(collectionKey, -1, options, ex);
            }
        }

        public async Task RemoveFromCollectionAsync(string collectionKey, IEnumerable<string> itemKeys, CancellationToken token = default)
        {
            //TODO: 测试这个

            token.ThrowIfCancellationRequested();

            //划分组 100个一组

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);
            try
            {
                //Prepare RedisKeys
                int itemCount = itemKeys.Count();
                List<RedisKey> redisKeys = new List<RedisKey>(itemCount + 1);
                redisKeys[0] = GetRealKey("", collectionKey);

                foreach (var itemKey in itemKeys)
                {
                    redisKeys.Add(itemKey);
                }

                //Prepare RedisValues
                RedisValue[] redisValues = new RedisValue[]
                {
                        itemCount,
                        MININAL_TIMESTAMP_LOCK_EXPIRY_SECONDS
                };

                //Do
                _ = await database.ScriptEvaluateAsync(
                    GetDefaultLoadLuas().LoadedCollectionRemoveItemWithTimestampLua,
                    redisKeys.ToArray(),
                    redisValues).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.Ordinal))
            {
                Logger.LogLuaScriptNotLoaded(null, null, nameof(RemoveFromCollectionAsync));

                InitLoadedLuas();

                await RemoveFromCollectionAsync(collectionKey, itemKeys, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw CacheExceptions.RemoveMultipleError(collectionKey, itemKeys, ex);
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
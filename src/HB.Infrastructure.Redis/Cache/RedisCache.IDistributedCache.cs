using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.Cache
{
    internal partial class RedisCache
    {
        // KEYS[1] = = key
        // ARGV[1] = absolute-expiration - unix time seconds as long (null for none)
        // ARGV[2] = sliding-expiration - seconds  as long (null for none)
        // ARGV[3] = ttl seconds 当前过期要设置的过期时间，由上面两个推算
        // ARGV[4] = data - byte[]
        // this order should not change LUA script depends on it
        private const string _luaSet = @"
redis.call('hmset', KEYS[1],'absexp',ARGV[1],'sldexp',ARGV[2],'data',ARGV[4]) 

if(ARGV[3]~='-1') then 
    redis.call('expire',KEYS[1], ARGV[3]) 
end

return 1";

        private const string _luaGetAndRefresh = @"
local data= redis.call('hmget',KEYS[1], 'absexp', 'sldexp','data') 

if (not data) then
    return nil
end

local now = tonumber((redis.call('time'))[1]) 

data[1] = tonumber(data[1])
data[2] = tonumber(data[2])

if(data[1]~= -1 and now >=data[1]) then 
    redis.call('del',KEYS[1]) 
    return nil 
end 

local curexp=-1 

if(data[1]~=-1 and data[2]~=-1) then 
    curexp=data[1]-now 
    if (data[2]<curexp) then 
        curexp=data[2] 
    end 
elseif (data[1]~=-1) then 
    curexp=data[1]-now 
elseif (data[2]~=-1) then 
    curexp=data[2] 
end

if(curexp~=-1) then 
    redis.call('expire', KEYS[1], curexp) 
end 

return data";

        public byte[]? Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetAndRefresh(key);
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, token: token).ConfigureAwait(false);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            IDatabase database = GetDefaultDatabase();

            options.Check();

            long? absoluteExpireUnixSeconds = options.AbsoluteExpiration?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(options.SlidingExpiration?.TotalSeconds);

            try
            {
                _ = database.ScriptEvaluate(GetDefaultLoadLuas().LoadedSetLua, new RedisKey[] { GetRealKey(key) },
                    new RedisValue[]
                    {
                        absoluteExpireUnixSeconds??-1,
                        slideSeconds??-1,
                        GetExpireSeconds(absoluteExpireUnixSeconds, slideSeconds)??-1,
                        value
                    });
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                Set(key, value, options);
            }
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            options.Check();

            long? absoluteExpireUnixSeconds = options.AbsoluteExpiration?.ToUnixTimeSeconds();
            long? slideSeconds = (long?)(options.SlidingExpiration?.TotalSeconds);

            try
            {
                await database.ScriptEvaluateAsync(GetDefaultLoadLuas().LoadedSetLua, new RedisKey[] { GetRealKey(key) },
                    new RedisValue[]
                    {
                        absoluteExpireUnixSeconds??-1,
                        slideSeconds??-1,
                        GetExpireSeconds(absoluteExpireUnixSeconds, slideSeconds)??-1,
                        value
                    }).ConfigureAwait(false);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                await SetAsync(key, value, options, token).ConfigureAwait(false);
            }
        }

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            GetAndRefresh(key);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, token: token).ConfigureAwait(false);
        }

        private async Task<byte[]?> GetAndRefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            try
            {
                RedisResult result = await database.ScriptEvaluateAsync(GetDefaultLoadLuas().LoadedGetAndRefreshLua, new RedisKey[] { GetRealKey(key) }).ConfigureAwait(false);

                return (byte[]?)result;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return await GetAndRefreshAsync(key, token).ConfigureAwait(false);
            }
        }

        private byte[]? GetAndRefresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IDatabase database = GetDefaultDatabase();

            try
            {
                RedisResult result = database.ScriptEvaluate(GetDefaultLoadLuas().LoadedGetAndRefreshLua, new RedisKey[] { GetRealKey(key) });

                return (byte[]?)result;
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT", StringComparison.InvariantCulture))
            {
                _logger.LogError(ex, "NOSCRIPT, will try again.");

                InitLoadedLuas();

                return GetAndRefresh(key);
            }
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IDatabase database = GetDefaultDatabase();

            database.KeyDelete(GetRealKey(key));
            // TODO: Error handling
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            await database.KeyDeleteAsync(GetRealKey(key)).ConfigureAwait(false);
            // TODO: Error handling
        }

        private static long? GetExpireSeconds(long? absoluteExpUnixSeconds, long? slidingSeconds)
        {
            #region 算法1 
            //考虑到slidingSeconds之后已经超过now，所以取小。实际没事，在get时，会检查是否过期
            //但如果slidingSeconds过长，则会长时间存放不能及时销毁

            long nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (absoluteExpUnixSeconds.HasValue && absoluteExpUnixSeconds <= nowUnixSeconds)
            {
                return 0;
            }
            else if (absoluteExpUnixSeconds.HasValue && slidingSeconds.HasValue)
            {
                return Math.Min(absoluteExpUnixSeconds.Value - nowUnixSeconds, slidingSeconds.Value);
            }
            else if (absoluteExpUnixSeconds.HasValue)
            {
                return absoluteExpUnixSeconds.Value - nowUnixSeconds;
            }
            else if (slidingSeconds.HasValue)
            {
                return slidingSeconds.Value;
            }
            else
            {
                return null;
            }

            #endregion

            //如果临到期，会到存放不到一个SlidingSeconds的时间
            //与算法1 有所取舍

            //if (slidingSeconds != null)
            //{
            //    return slidingSeconds;
            //}

            //if (absoluteExpUnixSeconds == null)
            //{
            //    return null;
            //}

            //return absoluteExpUnixSeconds - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }


    }
}

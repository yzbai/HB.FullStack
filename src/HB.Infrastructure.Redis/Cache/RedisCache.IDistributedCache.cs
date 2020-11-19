using Microsoft.Extensions.Caching.Distributed;
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
        private const string _luaSet = @"redis.call('hmset', KEYS[1],'absexp',ARGV[1],'sldexp',ARGV[2],'data',ARGV[4]) if(ARGV[3]~=-1) then redis.call('expire',KEYS[1], ARGV[3]) end return 1";
        private const string _luaGetAndRefresh = @"local now = tonumber((redis.call('time'))[1]) local data= redis.call('hmget',KEYS[1], 'absexp', 'sldexp','data') if(data[1]~= -1 and now >=tonumber(data[1])) then redis.call('del',KEYS[1]) return nil end local curexp=-1 if(data[1]~=-1 and data[2]~=-1) then curexp=data[1]-now if (tonumber(data[2])<curexp) then curexp=data[2] end elseif (data[1]~=-1) then curexp=data[1]-now elseif (data[2]~=-1) then curexp=data[2] end if(curexp~=-1) then redis.call('expire', KEYS[1], curexp) end return data";

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

            _ = database.ScriptEvaluate(GetDefaultLoadLuas().LoadedSetLua, new RedisKey[] { DefaultInstanceName + key },
                new RedisValue[]
                {
                        absoluteExpireUnixSeconds??-1,
                        slideSeconds??-1,
                        GetExpireSeconds(absoluteExpireUnixSeconds, slideSeconds)??-1,
                        value
                });

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


            await database.ScriptEvaluateAsync(GetDefaultLoadLuas().LoadedSetLua, new RedisKey[] { DefaultInstanceName + key },
                new RedisValue[]
                {
                        absoluteExpireUnixSeconds??-1,
                        slideSeconds??-1,
                        GetExpireSeconds(absoluteExpireUnixSeconds, slideSeconds)??-1,
                        value
                }).ConfigureAwait(false);

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

        private byte[]? GetAndRefresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IDatabase database = GetDefaultDatabase();

            database

            database.ScriptEvaluate

            RedisValue[] results = await database.ScriptEvaluate()

            if (getData)
            {
                results = HashMemberGet(database, DefaultInstanceName + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = HashMemberGet(database, DefaultInstanceName + key, AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                Refresh(database, key, absExpr, sldExpr);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        private async Task<byte[]?> GetAndRefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            if (getData)
            {
                results = await HashMemberGetAsync(database, DefaultInstanceName + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey).ConfigureAwait(false);
            }
            else
            {
                results = await HashMemberGetAsync(database, DefaultInstanceName + key, AbsoluteExpirationKey, SlidingExpirationKey).ConfigureAwait(false);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                await RefreshAsync(database, key, absExpr, sldExpr, token).ConfigureAwait(false);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IDatabase database = GetDefaultDatabase();

            database.KeyDelete(DefaultInstanceName + key);
            // TODO: Error handling
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IDatabase database = await GetDefaultDatabaseAsync().ConfigureAwait(false);

            await database.KeyDeleteAsync(DefaultInstanceName + key).ConfigureAwait(false);
            // TODO: Error handling
        }

        private static void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        private void Refresh(IDatabase database, string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                database.KeyExpire(DefaultInstanceName + key, expr);
                // TODO: Error handling
            }
        }

        private async Task RefreshAsync(IDatabase database, string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                await database.KeyExpireAsync(DefaultInstanceName + key, expr).ConfigureAwait(false);
                // TODO: Error handling
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="absoluteExpUnixSeconds"></param>
        /// <param name="slidingSeconds"></param>
        /// <returns></returns>
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

        private const string HmGetScript = (@"return redis.call('HMGET', KEYS[1], unpack(ARGV))");

        internal static RedisValue[] HashMemberGet(IDatabase cache, string key, params string[] members)
        {
            var result = cache.ScriptEvaluate(
                HmGetScript,
                new RedisKey[] { key },
                GetRedisMembers(members));

            // TODO: Error checking?
            return (RedisValue[])result;
        }

        internal static async Task<RedisValue[]> HashMemberGetAsync(
            IDatabase cache,
            string key,
            params string[] members)
        {

            var result = await cache.ScriptEvaluateAsync(
                HmGetScript,
                new RedisKey[] { key },
                GetRedisMembers(members)).ConfigureAwait(false);

            // TODO: Error checking?
            return (RedisValue[])result;
        }

        private static RedisValue[] GetRedisMembers(params string[] members)
        {
            var redisMembers = new RedisValue[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                redisMembers[i] = (RedisValue)members[i];
            }

            return redisMembers;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.Cache.Test
{

    //public class CachedBooksByGuid : CachedItem

    public class TimestampCacheTest : IClassFixture<ServiceFixture_MySql>
    {
        private readonly ICache _cache;
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly int _databaseNumber;
        private readonly ITestOutputHelper _outputHelper;
        private readonly string _applicationName;

        public TimestampCacheTest(ServiceFixture_MySql serviceFixture, ITestOutputHelper outputHelper)
        {
            _cache = serviceFixture.ServiceProvider.GetRequiredService<ICache>();
            _redisConnection = ConnectionMultiplexer.Connect(serviceFixture.Configuration["RedisCache:ConnectionSettings:0:ConnectionString"]);
            _databaseNumber = Convert.ToInt32(ConnectionMultiplexer.Connect(serviceFixture.Configuration["RedisCache:ConnectionSettings:0:DatabaseNumber"]));
            _applicationName = serviceFixture.Configuration["RedisCache:ApplicationName"];

            _outputHelper = outputHelper;
        }


        [Theory]
        [InlineData(50, 40)]
        [InlineData(null, 20)]
        [InlineData(20, null)]
        [InlineData(null, null)]
        public async Task CacheTimestamp_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();
            entryOptions.AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entryOptions.SlidingExpiration = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase(_databaseNumber);

            List<Book> books = Mocker.MockMany();

            string key = "BooksBy" + SecurityUtil.CreateUniqueToken();

            await _cache.SetAsync<List<Book>>(key, books, TimeUtil.UtcNowTicks, entryOptions).ConfigureAwait(false);
            List<Book>? cached2 = await _cache.GetAsync<List<Book>>(key).ConfigureAwait(false);

            Assert.True(cached2 != null);
            Assert.True(cached2!.Count == books.Count);

            await _cache.RemoveAsync(key, TimeUtil.UtcNowTicks).ConfigureAwait(false);


            Assert.False(await database.KeyExistsAsync(key));
        }

        [Theory]
        [InlineData(19, 15)]
        public async Task CacheTimestamp_Abs_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();
            entryOptions.AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entryOptions.SlidingExpiration = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase(_databaseNumber);

            Book book = Mocker.MockOne();

            //typeof(Book).GetProperty("Guid").SetValue(book, "123");
            //book.Guid = "12345";
            //book.Name = "abc";
            //book.BookID = 222;


            await _cache.SetAsync(book.Guid, book, TimeUtil.UtcNowTicks, entryOptions).ConfigureAwait(false);

            Assert.True(database.KeyExists(_applicationName + book.Guid));


            await Task.Delay(10 * 1000);

            Book? cached3 = await _cache.GetAsync<Book>(book.Guid).ConfigureAwait(false);

            Assert.True(cached3 != null);

            Assert.True(SerializeUtil.ToJson(book) == SerializeUtil.ToJson(cached3!));


            await Task.Delay(10 * 1000);

            Book? cached4 = await _cache.GetAsync<Book>(book.Guid);

            Assert.False(cached4 != null);

        }

        [Theory]
        [InlineData(50, 40)]
        [InlineData(null, 20)]
        [InlineData(20, null)]
        [InlineData(null, null)]
        public async Task CacheTimestamp_Timestamp_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();
            entryOptions.AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entryOptions.SlidingExpiration = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase(_databaseNumber);

            Book book = Mocker.MockOne();

            UtcNowTicks utcNowTicks = TimeUtil.UtcNowTicks;
            UtcNowTicks utcNowTicks2 = TimeUtil.UtcNowTicks;
            UtcNowTicks utcNowTicks3 = TimeUtil.UtcNowTicks;

            utcNowTicks2.Ticks -= 10000;
            utcNowTicks3.Ticks += 10000;

            string oldName = book.Name;
            await _cache.SetAsync(book.Guid, book, utcNowTicks, entryOptions).ConfigureAwait(false);

            book.Name += "22222";

            await _cache.SetAsync(book.Guid, book, utcNowTicks2, entryOptions).ConfigureAwait(false);

            Book cached = await _cache.GetAsync<Book>(book.Guid);

            Assert.True(cached.Name == oldName);

            await _cache.SetAsync(book.Guid, book, utcNowTicks3, entryOptions).ConfigureAwait(false);

            Book cached2 = await _cache.GetAsync<Book>(book.Guid);

            Assert.True(cached2.Name == book.Name);
        }
    }
}

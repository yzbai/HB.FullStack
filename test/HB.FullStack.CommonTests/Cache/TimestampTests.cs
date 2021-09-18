using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.DatabaseTests.Data;
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
        private readonly HB.FullStack.Database.IDatabase _mysql;

        public TimestampCacheTest(ServiceFixture_MySql serviceFixture, ITestOutputHelper outputHelper)
        {
            _cache = serviceFixture.ServiceProvider.GetRequiredService<ICache>();
            _redisConnection = ConnectionMultiplexer.Connect(serviceFixture.Configuration["RedisCache:ConnectionSettings:0:ConnectionString"]);
            _databaseNumber = Convert.ToInt32(serviceFixture.Configuration["RedisCache:ConnectionSettings:0:DatabaseNumber"]);
            _applicationName = serviceFixture.Configuration["RedisCache:ApplicationName"];

            _mysql = serviceFixture.ServiceProvider.GetRequiredService<HB.FullStack.Database.IDatabase>();

            _outputHelper = outputHelper;
        }


        /// <summary>
        /// CacheTimestamp_TestAsync
        /// </summary>
        /// <param name="absoluteSecondsRelativeToNow"></param>
        /// <param name="slidingSeconds"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
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

            await AddToDatabaeAsync(books).ConfigureAwait(false);

            string key = "BooksBy" + SecurityUtil.CreateUniqueToken();

            await _cache.SetAsync<List<Book>>(key, books, TimeUtil.UtcNowTicks, entryOptions).ConfigureAwait(false);
            List<Book>? cached2 = await _cache.GetAsync<List<Book>>(key).ConfigureAwait(false);

            Assert.True(cached2 != null);
            Assert.True(cached2!.Count == books.Count);

            await _cache.RemoveAsync(key, TimeUtil.UtcNowTicks).ConfigureAwait(false);


            Assert.False(await database.KeyExistsAsync(key));
        }

        /// <summary>
        /// CacheTimestamp_Abs_TestAsync
        /// </summary>
        /// <param name="absoluteSecondsRelativeToNow"></param>
        /// <param name="slidingSeconds"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
        [Theory]
        [InlineData(19, 15)]
        public async Task CacheTimestamp_Abs_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();
            entryOptions.AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entryOptions.SlidingExpiration = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase(_databaseNumber);

            Book book = Mocker.MockOne();

            await AddToDatabaeAsync(new Book[] { book }).ConfigureAwait(false);

            //typeof(Book).GetProperty("Guid").SetValue(book, "123");
            //book.Guid = "12345";
            //book.Name = "abc";
            //book.BookID = 222;


            await _cache.SetAsync(nameof(Book) + book.Id.ToString(), book, TimeUtil.UtcNowTicks, entryOptions).ConfigureAwait(false);

            Assert.True(database.KeyExists(_applicationName + nameof(Book) + book.Id.ToString()));


            await Task.Delay(10 * 1000);

            Book? cached3 = await _cache.GetAsync<Book>(nameof(Book) + book.Id.ToString()).ConfigureAwait(false);

            Assert.True(cached3 != null);

            Assert.True(SerializeUtil.ToJson(book) == SerializeUtil.ToJson(cached3!));


            await Task.Delay(10 * 1000);

            Book? cached4 = await _cache.GetAsync<Book>(nameof(Book) + book.Id.ToString());

            Assert.False(cached4 != null);

        }

        /// <summary>
        /// CacheTimestamp_Timestamp_TestAsync
        /// </summary>
        /// <param name="absoluteSecondsRelativeToNow"></param>
        /// <param name="slidingSeconds"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
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

            await AddToDatabaeAsync(new Book[] { book }).ConfigureAwait(false);

            UtcNowTicks utcNowTicks = TimeUtil.UtcNowTicks;
            UtcNowTicks utcNowTicks2 = TimeUtil.UtcNowTicks;
            UtcNowTicks utcNowTicks3 = TimeUtil.UtcNowTicks;

            utcNowTicks2.Ticks -= 10000;
            utcNowTicks3.Ticks += 10000;

            string oldName = book.Name;
            await _cache.SetAsync(nameof(Book) + book.Id.ToString(), book, utcNowTicks, entryOptions).ConfigureAwait(false);

            book.Name += "22222";

            await _cache.SetAsync(nameof(Book) + book.Id.ToString(), book, utcNowTicks2, entryOptions).ConfigureAwait(false);

            Book? cached = await _cache.GetAsync<Book>(nameof(Book) + book.Id.ToString());

            Assert.True(cached?.Name == oldName);

            await _cache.SetAsync(nameof(Book) + book.Id.ToString(), book, utcNowTicks3, entryOptions).ConfigureAwait(false);

            Book? cached2 = await _cache.GetAsync<Book>(nameof(Book) + book.Id.ToString());

            Assert.True(cached2?.Name == book.Name);
        }

        /// <summary>
        /// AddToDatabaeAsync
        /// </summary>
        /// <param name="books"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private async Task AddToDatabaeAsync(IEnumerable<Book> books)
        {
            await _mysql.BatchAddAsync(books, "", GetFakeTransactionContext()).ConfigureAwait(false);
        }

        private static Database.TransactionContext GetFakeTransactionContext()
        {
            return new Database.TransactionContext(null!, Database.TransactionStatus.InTransaction, null!);
        }
    }
}

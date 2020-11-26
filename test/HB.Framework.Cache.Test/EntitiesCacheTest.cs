using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;

using Xunit;
using Xunit.Abstractions;

namespace HB.Framework.Cache.Test
{
    public class EntitiesCacheTest : IClassFixture<ServiceFixture>
    {
        private readonly ICache _cache;
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly ITestOutputHelper _outputHelper;

        public EntitiesCacheTest(ServiceFixture serviceFixture, ITestOutputHelper outputHelper)
        {
            _cache = serviceFixture.Cache;
            _redisConnection = serviceFixture.RedisConnection;

            _outputHelper = outputHelper;
        }


        [Theory]
        [InlineData(50, 40)]
        [InlineData(null, 20)]
        [InlineData(20, null)]
        [InlineData(null, null)]
        public async Task CacheEntities_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            Stopwatch stopwatch = new Stopwatch();
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase();

            List<Book> books = Mocker.MockMany();

            IEnumerable<string> bookIds = books.Select(b => b.BookID.ToString());
            IEnumerable<string> bookNames = books.Select(b => b.Name);
            IEnumerable<string> guids = books.Select(b => b.Guid);
            List<RedisKey> guidRedisKeys = new List<RedisKey>();
            List<RedisKey> bookIdRedisKeys = new List<RedisKey>();
            List<RedisKey> bookNameRedisKeys = new List<RedisKey>();

            foreach (Book book in books)
            {
                guidRedisKeys.Add(ServiceFixture.ApplicationName + book.Guid);
                bookIdRedisKeys.Add(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID);
                bookNameRedisKeys.Add(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name);
            }

            (IEnumerable<Book>? cached, bool exists) = await _cache.GetEntitiesAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);
            (IEnumerable<Book>? cached2, bool exists2) = await _cache.GetEntitiesAsync<Book>(nameof(Book.Guid), guids).ConfigureAwait(false);

            Assert.True(exists == false && cached == null);
            Assert.True(exists2 == false && cached2 == null);

            await _cache.RemoveEntitiesAsync<Book>(nameof(Book.Guid), guids).ConfigureAwait(false);
            await _cache.RemoveEntitiesAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);

            stopwatch.Reset();
            stopwatch.Start();
            await _cache.SetEntitiesAsync(books).ConfigureAwait(false);
            stopwatch.Stop();

            _outputHelper.WriteLine($"Set 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

            Assert.True(guidRedisKeys.Count == database.KeyExists(guidRedisKeys.ToArray()));
            Assert.True(guidRedisKeys.Count == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.True(guidRedisKeys.Count == database.KeyExists(bookNameRedisKeys.ToArray()));


            stopwatch.Reset();
            stopwatch.Start();
            (IEnumerable<Book>? cached3, bool exists3) = await _cache.GetEntitiesAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);
            stopwatch.Stop();
            _outputHelper.WriteLine($"Get 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

            Assert.True(exists3);
            Assert.True(cached3!.Count() == books.Count);
            Assert.True(SerializeUtil.ToJson(books[0]) == SerializeUtil.ToJson(cached3!.ElementAt(0)));

            (IEnumerable<Book>? cached4, bool exists4) = await _cache.GetEntitiesAsync<Book>(nameof(Book.Guid), guids);

            Assert.True(exists4);
            Assert.True(cached4!.Count() == books.Count);
            Assert.True(SerializeUtil.ToJson(books[0]) == SerializeUtil.ToJson(cached4!.ElementAt(0)));

            stopwatch.Reset();
            stopwatch.Start();
            await _cache.RemoveEntitiesAsync<Book>(nameof(Book.Guid), guids).ConfigureAwait(false);
            stopwatch.Stop();
            _outputHelper.WriteLine($"Delete 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");


            Assert.True(0 == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.True(0 == database.KeyExists(bookNameRedisKeys.ToArray()));
            Assert.True(0 == database.KeyExists(guidRedisKeys.ToArray()));

            await _cache.SetEntitiesAsync<Book>(books).ConfigureAwait(false);

            Assert.True(guidRedisKeys.Count == database.KeyExists(guidRedisKeys.ToArray()));
            Assert.True(guidRedisKeys.Count == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.True(guidRedisKeys.Count == database.KeyExists(bookNameRedisKeys.ToArray()));

            await _cache.RemoveEntitiesAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);

            Assert.True(0 == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.True(0 == database.KeyExists(bookNameRedisKeys.ToArray()));
            Assert.True(0 == database.KeyExists(guidRedisKeys.ToArray()));
        }

        [Theory]
        [InlineData(19, 15)]
        public async Task CacheEntities_Abs_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase();

            Book book = Mocker.MockOne();

            //typeof(Book).GetProperty("Guid").SetValue(book, "123");
            //book.Guid = "12345";
            //book.Name = "abc";
            //book.BookID = 222;


            await _cache.SetEntityAsync(book).ConfigureAwait(false);

            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + book.Guid));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));


            await Task.Delay(10 * 1000);

            (Book? cached3, bool exists3) = await _cache.GetEntityAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);

            Assert.True(exists3);

            Assert.True(SerializeUtil.ToJson(book) == SerializeUtil.ToJson(cached3!));


            await Task.Delay(10 * 1000);

            (Book? cached4, bool exists4) = await _cache.GetEntityAsync<Book>(book);

            Assert.False(exists4);

            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + book.Guid));
            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));
        }

        [Theory]
        [InlineData(50, 40)]
        [InlineData(null, 20)]
        [InlineData(20, null)]
        [InlineData(null, null)]
        public async Task CacheEntities_Version_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase();

            IList<Book> books = Mocker.MockMany();

            await _cache.RemoveEntitiesAsync<Book>("Guid", books.Select(b => b.Guid)).ConfigureAwait(false);

            IEnumerable<bool> oks = await _cache.SetEntitiesAsync(books).ConfigureAwait(false);

            Assert.True(oks.All(b => b));

            IEnumerable<bool> oks1 = await _cache.SetEntitiesAsync(books).ConfigureAwait(false);

            Assert.True(oks1.All(b => !b));

            typeof(Book).GetProperty("Version")!.SetValue(books[0], books[0].Version + 1);

            IEnumerable<bool> oks2 = await _cache.SetEntitiesAsync(books).ConfigureAwait(false);

            Assert.True(oks2.ElementAt(0));

            Assert.True(oks2.Count(b => b) == 1);
        }
    }
}

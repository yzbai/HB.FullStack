using System;
using System.Threading.Tasks;

using StackExchange.Redis;

using Xunit;

namespace HB.Framework.Cache.Test
{

    public class EntityCacheTest : IClassFixture<ServiceFixture>
    {
        private readonly ICache _cache;
        private readonly ConnectionMultiplexer _redisConnection;

        public EntityCacheTest(ServiceFixture serviceFixture)
        {
            _cache = serviceFixture.Cache;
            _redisConnection = serviceFixture.RedisConnection;
        }


        [Theory]
        [InlineData(50, 40)]
        [InlineData(null, 20)]
        [InlineData(20, null)]
        [InlineData(null, null)]
        public async Task CacheEntity_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase();

            Book book = Mocker.MockOne();

            //book.Guid = "12345";
            //book.Name = "abc";
            //book.BookID = 222;

            (Book? cached, bool exists) = await _cache.GetEntityAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);
            (Book? cached2, bool exists2) = await _cache.GetEntityAsync<Book>(nameof(Book.Guid), book.Guid).ConfigureAwait(false);

            Assert.True(exists == false && cached == null);
            Assert.True(exists2 == false && cached2 == null);

            await _cache.RemoveEntityAsync<Book>(nameof(Book.Guid), book.Guid.ToString()).ConfigureAwait(false);
            await _cache.RemoveEntityAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);


            await _cache.SetEntityAsync(book).ConfigureAwait(false);

            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + book.Guid));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));


            (Book? cached3, bool exists3) = await _cache.GetEntityAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);

            Assert.True(exists3);

            Assert.True(SerializeUtil.ToJson(book) == SerializeUtil.ToJson(cached3!));

            (Book? cached4, bool exists4) = await _cache.GetEntityAsync<Book>(book);

            Assert.True(exists4);

            Assert.True(SerializeUtil.ToJson(book) == SerializeUtil.ToJson(cached4!));

            await _cache.RemoveEntityAsync<Book>(nameof(Book.Guid), book.Guid.ToString()).ConfigureAwait(false);

            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + book.Guid));
            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));

            await _cache.SetEntityAsync<Book>(book).ConfigureAwait(false);

            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + book.Guid));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));

            await _cache.RemoveEntityAsync<Book>(book).ConfigureAwait(false);

            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + book.Guid));
            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.False(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));
        }

        [Theory]
        [InlineData(9, 7)]
        public async Task CacheEntity_Abs_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase();

            Book book = Mocker.MockOne();

            //book.Guid = "12345";
            //book.Name = "abc";
            //book.BookID = 222;


            await _cache.SetEntityAsync(book).ConfigureAwait(false);

            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + book.Guid));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.True(database.KeyExists(ServiceFixture.ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));


            await Task.Delay(5 * 1000);

            (Book? cached3, bool exists3) = await _cache.GetEntityAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);

            Assert.True(exists3);

            Assert.True(SerializeUtil.ToJson(book) == SerializeUtil.ToJson(cached3!));

            await Task.Delay(5 * 1000);

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
        public async Task CacheEntity_Version_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = _redisConnection.GetDatabase();

            Book book = Mocker.MockOne();

            await _cache.RemoveEntityAsync<Book>(book).ConfigureAwait(false);

            bool ok = await _cache.SetEntityAsync(book).ConfigureAwait(false);

            Assert.True(ok);

            bool ok1 = await _cache.SetEntityAsync(book).ConfigureAwait(false);

            Assert.False(ok1);

            typeof(Book).GetProperty("Version")!.SetValue(book, book.Version + 1);

            bool ok2 = await _cache.SetEntityAsync(book).ConfigureAwait(false);

            Assert.True(ok2);
        }
    }
}

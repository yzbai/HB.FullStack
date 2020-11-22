using System;
using System.Threading.Tasks;
using HB.Framework.Common.Entities;
using Xunit;

namespace HB.Framework.Cache.Test
{

    [CacheEntity(IsBatchEnabled = true/*, MaxAliveSeconds = 5 * 60, SlidingSeconds = 60*/)]
    public class Book : Entity
    {
        [CacheDifferentDimensionKey]
        public string Name { get; set; } = null!;

        [CacheDifferentDimensionKey]
        public long BookID { get; set; }

        public string? Publisher { get; set; }

        public double Price { get; set; }
    }

    public static class Mocker
    {
        private static readonly Random _random = new Random();
        public static Book MockOne()
        {
            return new Book
            {
                Name = SecurityUtil.CreateUniqueToken(),
                BookID = DateTimeOffset.UtcNow.Ticks,
                Publisher = _random.Next().ToString(),
                Price = _random.NextDouble() * 1000
            };
        }
    }

    public class EntityCacheTest : IClassFixture<ServiceFixture>
    {
        private readonly ICache _cache;

        public EntityCacheTest(ServiceFixture serviceFixture)
        {
            _cache = serviceFixture.Cache;
        }


        [Fact]
        public async Task CacheEntityDef_TestAsync()
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            Book book = Mocker.MockOne();

            await _cache.SetEntityAsync(book).ConfigureAwait(false);

            (Book? cached, bool exists) = await _cache.GetEntityAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);

            (Book? cached2, bool exists2) = await _cache.GetEntityAsync<Book>(book);

            await _cache.RemoveEntityAsync<Book>(nameof(Book.Guid), book.Guid.ToString()).ConfigureAwait(false);

            Assert.True(exists2);
        }
    }
}

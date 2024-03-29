﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.BaseTest;
using HB.FullStack.BaseTest.Data.Sqlites;
using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackExchange.Redis;

namespace HB.FullStack.CacheTests
{
    [TestClass]
    public class TimestampCacheTest : BaseTestClass
    {
        [TestMethod]
        [DataRow(50, 40)]
        [DataRow(null, 20)]
        [DataRow(20, null)]
        [DataRow(null, null)]
        public async Task CacheTimestamp_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value),
                SlidingExpiration = slidingSeconds == null ? null : TimeSpan.FromSeconds(slidingSeconds.Value)
            };

            IDatabase database = RedisConnection.GetDatabase(RedisDbNumber);

            List<Book> books = Mocker.MockMany();

            await AddToDatabaeAsync(books).ConfigureAwait(false);

            string key = "BooksBy" + SecurityUtil.CreateUniqueToken();

            await Cache.SetAsync(key, books, TimeUtil.Timestamp, entryOptions).ConfigureAwait(false);
            List<Book>? cached2 = await Cache.GetAsync<List<Book>>(key).ConfigureAwait(false);

            Assert.IsTrue(cached2 != null);
            Assert.IsTrue(cached2!.Count == books.Count);

            await Cache.RemoveAsync(key).ConfigureAwait(false);

            Assert.IsFalse(await database.KeyExistsAsync(key));
        }

        [TestMethod]
        [DataRow(19, 15)]
        public async Task CacheTimestamp_Abs_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value),
                SlidingExpiration = slidingSeconds == null ? null : TimeSpan.FromSeconds(slidingSeconds.Value)
            };

            IDatabase database = RedisConnection.GetDatabase(RedisDbNumber);

            Book book = Mocker.MockOne();

            await AddToDatabaeAsync(new Book[] { book }).ConfigureAwait(false);

            //typeof(Book).GetProperty("Guid").SetValue(book, "123");
            //book.Guid = "12345";
            //book.Code = "abc";
            //book.BookID = 222;

            await Cache.SetAsync(nameof(Book) + book.Id.ToString(), book, TimeUtil.Timestamp, entryOptions).ConfigureAwait(false);

            Assert.IsTrue(database.KeyExists(ApplicationName + nameof(Book) + book.Id.ToString()));

            await Task.Delay(10 * 1000);

            Book? cached3 = await Cache.GetAsync<Book>(nameof(Book) + book.Id.ToString()).ConfigureAwait(false);

            Assert.IsTrue(cached3 != null);

            Assert.IsTrue(SerializeUtil.ToJson(book) == SerializeUtil.ToJson(cached3!));

            await Task.Delay(10 * 1000);

            Book? cached4 = await Cache.GetAsync<Book>(nameof(Book) + book.Id.ToString());

            Assert.IsFalse(cached4 != null);
        }

        [TestMethod]
        [DataRow(50, 40)]
        [DataRow(null, 20)]
        [DataRow(20, null)]
        [DataRow(null, null)]
        public async Task CacheTimestamp_Timestamp_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value),
                SlidingExpiration = slidingSeconds == null ? null : TimeSpan.FromSeconds(slidingSeconds.Value)
            };

            IDatabase database = RedisConnection.GetDatabase(RedisDbNumber);

            Book book = Mocker.MockOne();

            await AddToDatabaeAsync(new Book[] { book }).ConfigureAwait(false);

            long utcNowTicks = TimeUtil.Timestamp;
            long utcNowTicks2 = TimeUtil.Timestamp;
            long utcNowTicks3 = TimeUtil.Timestamp;

            utcNowTicks2 -= 10000;
            utcNowTicks3 += 10000;

            string oldName = book.Name;
            await Cache.SetAsync(nameof(Book) + book.Id.ToString(), book, utcNowTicks, entryOptions).ConfigureAwait(false);

            book.Name += "22222";

            await Cache.SetAsync(nameof(Book) + book.Id.ToString(), book, utcNowTicks2, entryOptions).ConfigureAwait(false);

            Book? cached = await Cache.GetAsync<Book>(nameof(Book) + book.Id.ToString());

            Assert.IsTrue(cached?.Name == oldName);

            await Cache.SetAsync(nameof(Book) + book.Id.ToString(), book, utcNowTicks3, entryOptions).ConfigureAwait(false);

            Book? cached2 = await Cache.GetAsync<Book>(nameof(Book) + book.Id.ToString());

            Assert.IsTrue(cached2?.Name == book.Name);
        }

        private async Task AddToDatabaeAsync(IEnumerable<Book> books)
        {
            var transContext = await Trans.BeginTransactionAsync<Book>();

            try
            {
                await Db.AddAsync(books, "", transContext).ConfigureAwait(false);
                await transContext.CommitAsync();
            }
            catch
            {
                await transContext.RollbackAsync();
                throw;
            }
        }

    
    }
}
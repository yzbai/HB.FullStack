﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackExchange.Redis;

namespace HB.FullStack.CacheTests
{
    [TestClass]
    public class EntitiesCacheTest : BaseTestClass
    {
        [TestMethod]
        [DataRow(50, 40)]
        [DataRow(null, 20)]
        [DataRow(20, null)]
        [DataRow(null, null)]
        public async Task CacheEntities_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            Stopwatch stopwatch = new Stopwatch();
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = RedisConnection.GetDatabase(DatabaseNumber);

            List<Book> books = Mocker.MockMany();

            await AddToDatabaeAsync(books).ConfigureAwait(false);

            IEnumerable<string> bookIds = books.Select(b => b.BookID.ToString());
            IEnumerable<string> bookNames = books.Select(b => b.Name);
            IEnumerable<string> ids = books.Select(b => b.Id.ToString());
            List<RedisKey> idRedisKeys = new List<RedisKey>();
            List<RedisKey> bookIdRedisKeys = new List<RedisKey>();
            List<RedisKey> bookNameRedisKeys = new List<RedisKey>();

            foreach (Book book in books)
            {
                idRedisKeys.Add(ApplicationName + nameof(Book) + book.Id);
                bookIdRedisKeys.Add(ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID);
                bookNameRedisKeys.Add(ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name);
            }

            (IEnumerable<Book>? cached, bool exists) = await Cache.GetEntitiesAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);
            (IEnumerable<Book>? cached2, bool exists2) = await Cache.GetEntitiesAsync<Book>(nameof(Book.Id), ids).ConfigureAwait(false);

            Assert.IsTrue(exists == false && cached == null);
            Assert.IsTrue(exists2 == false && cached2 == null);

            await Cache.RemoveEntitiesAsync<Book>(nameof(Book.Id), ids, books.Select(b => b.Version)).ConfigureAwait(false);
            await Cache.RemoveEntitiesAsync<Book>(nameof(Book.Name), bookNames, books.Select(b => b.Version)).ConfigureAwait(false);

            stopwatch.Reset();
            stopwatch.Start();
            await Cache.SetEntitiesAsync(books).ConfigureAwait(false);
            stopwatch.Stop();

            Console.WriteLine($"Set 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

            Assert.IsTrue(idRedisKeys.Count == database.KeyExists(idRedisKeys.ToArray()));
            Assert.IsTrue(idRedisKeys.Count == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.IsTrue(idRedisKeys.Count == database.KeyExists(bookNameRedisKeys.ToArray()));

            stopwatch.Reset();
            stopwatch.Start();
            (IEnumerable<Book>? cached3, bool exists3) = await Cache.GetEntitiesAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);
            stopwatch.Stop();
            Console.WriteLine($"Get 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

            Assert.IsTrue(exists3);
            Assert.IsTrue(cached3!.Count() == books.Count);
            Assert.IsTrue(SerializeUtil.ToJson(books[0]) == SerializeUtil.ToJson(cached3!.ElementAt(0)));

            (IEnumerable<Book>? cached4, bool exists4) = await Cache.GetEntitiesAsync<Book>(nameof(Book.Id), ids);

            Assert.IsTrue(exists4);
            Assert.IsTrue(cached4!.Count() == books.Count);
            Assert.IsTrue(SerializeUtil.ToJson(books[0]) == SerializeUtil.ToJson(cached4!.ElementAt(0)));

            stopwatch.Reset();
            stopwatch.Start();
            await Cache.RemoveEntitiesAsync<Book>(nameof(Book.Id), ids, books.Select(b => b.Version)).ConfigureAwait(false);
            stopwatch.Stop();
            Console.WriteLine($"Delete 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

            Assert.IsTrue(0 == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.IsTrue(0 == database.KeyExists(bookNameRedisKeys.ToArray()));
            Assert.IsTrue(0 == database.KeyExists(idRedisKeys.ToArray()));

            await Cache.SetEntitiesAsync<Book>(books).ConfigureAwait(false);

            Assert.IsTrue(idRedisKeys.Count == database.KeyExists(idRedisKeys.ToArray()));
            Assert.IsTrue(idRedisKeys.Count == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.IsTrue(idRedisKeys.Count == database.KeyExists(bookNameRedisKeys.ToArray()));

            await Cache.RemoveEntitiesAsync<Book>(nameof(Book.Name), bookNames, books.Select(b => b.Version)).ConfigureAwait(false);

            Assert.IsTrue(0 == database.KeyExists(bookIdRedisKeys.ToArray()));
            Assert.IsTrue(0 == database.KeyExists(bookNameRedisKeys.ToArray()));
            Assert.IsTrue(0 == database.KeyExists(idRedisKeys.ToArray()));
        }

        [TestMethod]
        [DataRow(19, 15)]
        public async Task CacheEntities_Abs_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = RedisConnection.GetDatabase(DatabaseNumber);

            Book book = Mocker.MockOne();

            await AddToDatabaeAsync(new Book[] { book }).ConfigureAwait(false);

            //typeof(Book).GetProperty("Guid").SetValue(book, "123");
            //book.Guid = "12345";
            //book.Name = "abc";
            //book.BookID = 222;

            await Cache.SetEntityAsync(book).ConfigureAwait(false);

            Assert.IsTrue(database.KeyExists(ApplicationName + nameof(Book) + book.Id));
            Assert.IsTrue(database.KeyExists(ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.IsTrue(database.KeyExists(ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));

            await Task.Delay(10 * 1000);

            (Book? cached3, bool exists3) = await Cache.GetEntityAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);

            Assert.IsTrue(exists3);

            string json1 = SerializeUtil.ToJson(book);
            string json2 = SerializeUtil.ToJson(cached3!);

            Assert.AreEqual(json1, json2);

            await Task.Delay(10 * 1000);

            (Book? cached4, bool exists4) = await Cache.GetEntityAsync<Book>(book);

            Assert.IsFalse(exists4);

            Assert.IsFalse(database.KeyExists(ApplicationName + nameof(Book) + book.Id));
            Assert.IsFalse(database.KeyExists(ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
            Assert.IsFalse(database.KeyExists(ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));
        }

        [TestMethod]
        [DataRow(50, 40)]
        [DataRow(null, 20)]
        [DataRow(20, null)]
        [DataRow(null, null)]
        public async Task CacheEntities_Version_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            CacheEntityDef entityDef = CacheEntityDefFactory.Get<Book>();

            entityDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entityDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            IDatabase database = RedisConnection.GetDatabase(DatabaseNumber);

            IList<Book> books = Mocker.MockMany();

            await AddToDatabaeAsync(books).ConfigureAwait(false);

            await Cache.RemoveEntitiesAsync<Book>("Id", books.Select(b => b.Id.ToString()), books.Select(b => b.Version)).ConfigureAwait(false);

            IEnumerable<bool> oks = await Cache.SetEntitiesAsync(books).ConfigureAwait(false);

            Assert.IsTrue(oks.All(b => b));

            IEnumerable<bool> oks1 = await Cache.SetEntitiesAsync(books).ConfigureAwait(false);

            Assert.IsTrue(oks1.All(b => !b));

            typeof(Book).GetProperty("Version")!.SetValue(books[0], books[0].Version + 1);

            IEnumerable<bool> oks2 = await Cache.SetEntitiesAsync(books).ConfigureAwait(false);

            Assert.IsTrue(oks2.ElementAt(0));

            Assert.IsTrue(oks2.Count(b => b) == 1);
        }

        private static async Task AddToDatabaeAsync(IEnumerable<Book> books)
        {
            await Db.BatchAddAsync(books, "", GetFakeTransactionContext()).ConfigureAwait(false);
        }

        private static Database.TransactionContext GetFakeTransactionContext()
        {
            return new Database.TransactionContext(null!, Database.TransactionStatus.InTransaction, null!);
        }
    }
}
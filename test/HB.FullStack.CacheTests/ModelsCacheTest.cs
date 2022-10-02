using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Cache;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackExchange.Redis;

namespace HB.FullStack.CacheTests
{

	[TestClass]
	public class ModelsCacheTest : BaseTestClass
	{
		[TestMethod]
		[DataRow(50, 40)]
		[DataRow(null, 20)]
		[DataRow(20, null)]
		[DataRow(null, null)]
		[DataRow(1, 40)]
		[DataRow(null, 2)]
		[DataRow(2, null)]
		public async Task CacheModels_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
		{
			Stopwatch stopwatch = new Stopwatch();
			CacheModelDef modelDef = CacheModelDefFactory.Get<Book>();

			modelDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
			modelDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

			IDatabase database = RedisConnection.GetDatabase(DatabaseNumber);

			List<Book> books = Mocker.MockMany(10);

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

			(IEnumerable<Book>? cached, bool exists) = await Cache.GetModelsAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);
			(IEnumerable<Book>? cached2, bool exists2) = await Cache.GetModelsAsync<Book>(nameof(Book.Id), ids).ConfigureAwait(false);

			Assert.IsTrue(exists == false && cached == null);
			Assert.IsTrue(exists2 == false && cached2 == null);

			await Cache.RemoveModelsAsync<Book>(nameof(Book.Id), ids).ConfigureAwait(false);
			await Cache.RemoveModelsAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);

			stopwatch.Reset();
			stopwatch.Start();
			await Cache.SetModelsAsync(books).ConfigureAwait(false);
			stopwatch.Stop();

			Console.WriteLine($"Set 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

			Assert.IsTrue(idRedisKeys.Count == database.KeyExists(idRedisKeys.ToArray()));
			Assert.IsTrue(idRedisKeys.Count == database.KeyExists(bookIdRedisKeys.ToArray()));
			Assert.IsTrue(idRedisKeys.Count == database.KeyExists(bookNameRedisKeys.ToArray()));

			stopwatch.Reset();
			stopwatch.Start();
			(IEnumerable<Book>? cached3, bool exists3) = await Cache.GetModelsAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);
			stopwatch.Stop();
			Console.WriteLine($"Get 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

			Assert.IsTrue(exists3);
			Assert.IsTrue(cached3!.Count() == books.Count);
			Assert.IsTrue(SerializeUtil.ToJson(books[0]) == SerializeUtil.ToJson(cached3!.ElementAt(0)));

			(IEnumerable<Book>? cached4, bool exists4) = await Cache.GetModelsAsync<Book>(nameof(Book.Id), ids);

			Assert.IsTrue(exists4);
			Assert.IsTrue(cached4!.Count() == books.Count);
			Assert.IsTrue(SerializeUtil.ToJson(books[0]) == SerializeUtil.ToJson(cached4!.ElementAt(0)));

			stopwatch.Reset();
			stopwatch.Start();
			await Cache.RemoveModelsAsync<Book>(nameof(Book.Id), ids).ConfigureAwait(false);
			stopwatch.Stop();
			Console.WriteLine($"Delete 100 Items, Spend: {stopwatch.ElapsedMilliseconds}");

			Assert.IsTrue(0 == database.KeyExists(bookIdRedisKeys.ToArray()));
			Assert.IsTrue(0 == database.KeyExists(bookNameRedisKeys.ToArray()));
			Assert.IsTrue(0 == database.KeyExists(idRedisKeys.ToArray()));

			//这里应该会和删除后最小minTimestam冲突，导致无法添加
			await Cache.SetModelsAsync<Book>(books).ConfigureAwait(false);

			Assert.IsTrue(/*idRedisKeys.Count*/0 == database.KeyExists(idRedisKeys.ToArray()));
			Assert.IsTrue(/*idRedisKeys.Count*/0 == database.KeyExists(bookIdRedisKeys.ToArray()));
			Assert.IsTrue(/*idRedisKeys.Count*/0 == database.KeyExists(bookNameRedisKeys.ToArray()));

			//await Cache.RemoveModelsAsync<Book>(nameof(Book.Name), bookNames).ConfigureAwait(false);

			//Assert.IsTrue(0 == database.KeyExists(bookIdRedisKeys.ToArray()));
			//Assert.IsTrue(0 == database.KeyExists(bookNameRedisKeys.ToArray()));
			//Assert.IsTrue(0 == database.KeyExists(idRedisKeys.ToArray()));
		}

		[TestMethod]
		[DataRow(19, 15)]
		public async Task CacheModels_Abs_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
		{
			CacheModelDef modelDef = CacheModelDefFactory.Get<Book>();

			modelDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
			modelDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

			IDatabase database = RedisConnection.GetDatabase(DatabaseNumber);

			Book book = Mocker.MockOne();

			await AddToDatabaeAsync(new Book[] { book }).ConfigureAwait(false);

			//typeof(Book).GetProperty("Guid").SetValue(book, "123");
			//book.Guid = "12345";
			//book.Code = "abc";
			//book.BookID = 222;

			await Cache.SetModelAsync(book).ConfigureAwait(false);

			Assert.IsTrue(database.KeyExists(ApplicationName + nameof(Book) + book.Id));
			Assert.IsTrue(database.KeyExists(ApplicationName + nameof(Book) + nameof(Book.BookID) + book.BookID));
			Assert.IsTrue(database.KeyExists(ApplicationName + nameof(Book) + nameof(Book.Name) + book.Name));

			await Task.Delay(10 * 1000);

			(Book? cached3, bool exists3) = await Cache.GetModelAsync<Book>(nameof(Book.Name), book.Name).ConfigureAwait(false);

			Assert.IsTrue(exists3);

			string json1 = SerializeUtil.ToJson(book);
			string json2 = SerializeUtil.ToJson(cached3!);

			Assert.AreEqual(json1, json2);

			await Task.Delay(10 * 1000);

			(Book? cached4, bool exists4) = await Cache.GetModelAsync<Book>(book);

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
		public async Task CacheModels_Version_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
		{
			CacheModelDef modelDef = CacheModelDefFactory.Get<Book>();

			modelDef.AbsoluteTimeRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
			modelDef.SlidingTime = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

			IDatabase database = RedisConnection.GetDatabase(DatabaseNumber);

			IList<Book> books = Mocker.MockMany();

			await AddToDatabaeAsync(books).ConfigureAwait(false);

			await Cache.RemoveModelsAsync<Book>("Id", books.Select(b => b.Id.ToString())).ConfigureAwait(false);

			IEnumerable<bool> oks = await Cache.SetModelsAsync(books).ConfigureAwait(false);

			Assert.IsTrue(oks.All(b => b));

			IEnumerable<bool> oks1 = await Cache.SetModelsAsync(books).ConfigureAwait(false);

			Assert.IsTrue(oks1.All(b => !b));

			typeof(Book).GetProperty("Timestamp")!.SetValue(books[0], TimeUtil.UtcNowTicks);

			IEnumerable<bool> oks2 = await Cache.SetModelsAsync(books).ConfigureAwait(false);

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
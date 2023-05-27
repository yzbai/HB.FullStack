using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class UpdatePropertiesTests : DatabaseTestClass
    {
        private async Task Test_UpdateProperties_UsingTimestamp_Core<T>() where T : class, IBookModel, ITimestamp
        {
            //Add
            var book = Mocker.Mock<T>(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            TimestampUpdatePack updatePack = new TimestampUpdatePack
            {
                Id = book.Id,
                OldTimestamp = (book as ITimestamp)!.Timestamp,
                PropertyNames = new string[] { nameof(IBookModel.Price), nameof(IBookModel.Name) },
                NewPropertyValues = new object?[] { 123456.789, "TTTTTXXXXTTTTT" }
            };

            await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);

            IBookModel? updatedBook = await Db.ScalarAsync<T>(book.Id!, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");
            Assert.IsTrue((updatedBook as ITimestamp)!.Timestamp > book.Timestamp);

            var ex = await Assert.ThrowsExceptionAsync<DbException>(async () =>
            {
                //Repeat Update should have concurrency conflict
                await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);
            });

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);
        }

        [TestMethod]
        public async Task Test_UpdateProperties_UsingTimestamp()
        {
            await Test_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Long_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
        }

        private async Task Test_UpdateProperties_UsingOldNewCompare_Core<T>() where T : IBookModel
        {
            //Add
            var book = Mocker.Mock<T>(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            OldNewCompareUpdatePack updatePack = new OldNewCompareUpdatePack
            {
                Id = book.Id,
                PropertyNames = new string[] { nameof(IBookModel.Price), nameof(IBookModel.Name) },
                OldPropertyValues = new object?[] { book.Price, book.Name },
                NewPropertyValues = new object?[] { 123456.789, "TTTTTXXXXTTTTT" }
            };

            await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);

            var updatedBook = await Db.ScalarAsync<T>(book.Id!, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");

            var ex = await Assert.ThrowsExceptionAsync<DbException>(async () =>
            {
                await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);
            });

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);
        }

        [TestMethod]
        public async Task Test_UpdateProperties_UsingOldNewCompare()
        {
            await Test_UpdateProperties_UsingOldNewCompare_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<MySql_Timestamp_Long_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<MySql_Timeless_Guid_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<MySql_Timeless_Long_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();
        }

        private async Task Test_Batch_UpdateProperties_UsingTimestamp_Core<T>() where T : class, IBookModel, ITimestamp
        {
            //Add
            var models = await AddAndRetrieve<T>(50);

            var updatePacks = new List<TimestampUpdatePack>();

            foreach (var model in models)
            {
                TimestampUpdatePack updatePack = new TimestampUpdatePack
                {
                    Id = model.Id,
                    OldTimestamp = (model as ITimestamp)!.Timestamp,
                    PropertyNames = new string[] { nameof(IBookModel.Price), nameof(IBookModel.Name) },
                    NewPropertyValues = new object?[] { 123456.789, "TTTTTXXXXTTTTT" }
                };

                updatePacks.Add(updatePack);
            }

            var trans = await Trans.BeginTransactionAsync<T>();

            try
            {
                await Db.UpdatePropertiesAsync<T>(updatePacks, "UPDATE_FIELDS_VERSION", trans);
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }

            var rts = await Db.RetrieveAsync<T>(t => SqlStatement.In(t.Id, true, models.Select(m => m.Id)), null);

            Assert.IsTrue(rts.Count == models.Count);
            int number = 0;
            foreach (var rt in rts)
            {
                Assert.IsNotNull(rt);

                Assert.IsTrue(rt.Price == 123456.789);
                Assert.IsTrue(rt.Name == "TTTTTXXXXTTTTT");
                Assert.IsTrue(rt.LastUser == "UPDATE_FIELDS_VERSION");
                Assert.IsTrue((rt as ITimestamp)!.Timestamp > models[number++].Timestamp);

            }

            var trans2 = await Trans.BeginTransactionAsync<T>();

            var ex = await Assert.ThrowsExceptionAsync<DbException>(async () =>
            {
                //Repeat Update should have concurrency conflict
                await Db.UpdatePropertiesAsync<T>(updatePacks, "UPDATE_FIELDS_VERSION", trans2);
                await trans2.CommitAsync();
            });

            await trans2.RollbackAsync();

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_UsingTimestamp()
        {
            await Test_Batch_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
        }

        private async Task Test_Batch_UpdateProperties_UsingOldNewCompare_Core<T>() where T : IBookModel
        {
            //Add
            var books = await AddAndRetrieve<T>(50);
            var updatePacks = new List<OldNewCompareUpdatePack>();

            foreach (var book in books)
            {
                OldNewCompareUpdatePack updatePack = new OldNewCompareUpdatePack
                {
                    Id = book.Id,
                    PropertyNames = new string[] { nameof(IBookModel.Price), nameof(IBookModel.Name) },
                    OldPropertyValues = new object?[] { book.Price, book.Name },
                    NewPropertyValues = new object?[] { 123456.789, "TTTTTXXXXTTTTT" }
                };

                updatePacks.Add(updatePack);
            }

            //Update

            var trans = await Trans.BeginTransactionAsync<T>();
            try
            {
                await Db.UpdatePropertiesAsync<T>(updatePacks, "UPDATE_FIELDS_VERSION", trans);
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }

            var rts = await Db.RetrieveAsync<T>(t => SqlStatement.In(t.Id, true, books.Select(m => m.Id)), null);

            Assert.IsTrue(rts.Count == books.Count);

            foreach (var rt in rts)
            {
                Assert.IsNotNull(rt);

                Assert.IsTrue(rt.Price == 123456.789);
                Assert.IsTrue(rt.Name == "TTTTTXXXXTTTTT");
                Assert.IsTrue(rt.LastUser == "UPDATE_FIELDS_VERSION");
            }

            var trans2 = await Trans.BeginTransactionAsync<T>();

            var ex = await Assert.ThrowsExceptionAsync<DbException>(async () =>
            {
                //Repeat Update should have concurrency conflict
                await Db.UpdatePropertiesAsync<T>(updatePacks, "UPDATE_FIELDS_VERSION", trans2);
                await trans2.CommitAsync();
            });

            await trans2.RollbackAsync();

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_UsingOldNewCompare()
        {
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<MySql_Timeless_Long_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Batch_UpdateProperties_UsingOldNewCompare_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();
        }

        private async Task Test_UpdateProperties_PropertyTrackable_Core<T>() where T : IDbModel, IPropertyTrackableObject
        {
            var model = (await AddAndRetrieve<T>(1)).First();

            Assert.IsTrue(model.IsTracking());
            Mocker.Modify(model);

            var up = model.GetPropertyChangePack();

            await Db.UpdatePropertiesAsync<T>(up, "", null);

            var rt = await Db.ScalarAsync<T>(model.Id!, null);

            if (model is ITimestamp)
            {
                Assert.IsTrue((model as ITimestamp)!.Timestamp < (rt as ITimestamp)!.Timestamp);
                (model as ITimestamp)!.Timestamp = (rt as ITimestamp)!.Timestamp;
            }

            Assert.IsTrue(SerializeUtil.ToJson(model) == SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_UpdateProperties_PropertyTrackable()
        {
            await Test_UpdateProperties_PropertyTrackable_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_UpdateProperties_PropertyTrackable_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Batch_UpdateProperties_PropertyTrackable_Core<T>() where T : IDbModel, IPropertyTrackableObject
        {
            var models = await AddAndRetrieve<T>(50);

            Assert.IsTrue(models.All(m => m.IsTracking()));
            Mocker.Modify(models);

            var ups = models.Select(m => m.GetPropertyChangePack()).ToList();

            var trans = await Trans.BeginTransactionAsync<T>();
            try
            {
                await Db.UpdatePropertiesAsync<T>(ups, "", trans);
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }

            var rts = await Db.RetrieveAsync<T>(t => SqlStatement.In(t.Id, true, models.Select(m => m.Id)), null);

            if (models[0] is ITimestamp)
            {
                for (int i = 0; i < models.Count; ++i)
                {
                    Assert.IsTrue((models[i] as ITimestamp)!.Timestamp < (rts[0] as ITimestamp)!.Timestamp);
                    (models[i] as ITimestamp)!.Timestamp = (rts[0] as ITimestamp)!.Timestamp;
                }
            }

            Assert.IsTrue(SerializeUtil.ToJson(models) == SerializeUtil.ToJson(rts));
        }

        [TestMethod]
        public async Task Test_Batch_UpdateProperties_PropertyTrackable()
        {
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_UpdateProperties_PropertyTrackable_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }
    }
}
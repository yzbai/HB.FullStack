using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database;

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


    }
}

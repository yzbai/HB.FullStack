using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class AddTests : DatabaseTestClass
    {
        private async Task Test_Add_Core<T>() where T : IDbModel
        {
            T model = Mocker.MockOne<T>();

            await Db.AddAsync(model, "lastUsre", null).ConfigureAwait(false);

            var rt = await Db.ScalarAsync<T>(model.Id!, null);

            Assert.IsTrue(SerializeUtil.ToJson(model) == SerializeUtil.ToJson(rt));
        }

        [TestMethod]
        public async Task Test_Add()
        {
            await Test_Add_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Add_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Add_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Add_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Add_Core<MySql_Timeless_Long_BookModel>();
            await Test_Add_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Add_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Add_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Add_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Add_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Add_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Add_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Add_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Batch_Add_Core<T>() where T : IDbModel
        {
            await AddAndRetrieve<T>();
        }

        [TestMethod]
        public async Task Test_Batch_Add()
        {
            await Test_Batch_Add_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_BookModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Batch_Add_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Add_Key_Conflict_Core<T>() where T : class, IDbModel
        {
            IDbModel model = Mocker.Mock<T>(1).First();

            await Db.AddAsync(model, "", null);

            var ex = await Assert.ThrowsExceptionAsync<DbException>(async () =>
            {
                await Db.AddAsync(model, "", null);
            });

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.DuplicateKeyEntry);
        }

        [TestMethod]
        public async Task Test_Add_Key_Conflict()
        {
            await Test_Add_Key_Conflict_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Add_Key_Conflict_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Key_Conflict_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Add_Key_Conflict_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Key_Conflict_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Add_Key_Conflict_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Key_Conflict_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Add_Key_Conflict_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Batch_Add_Key_Conflict_Core<T>() where T : class, IDbModel
        {
            IList<T> models = await AddAndRetrieve<T>(50);

            TransactionContext trans2 = await Trans.BeginTransactionAsync<T>();

            var ex = await Assert.ThrowsExceptionAsync<DbException>(async () =>
            {
                await Db.AddAsync(models, "tester", trans2);
                await trans2.CommitAsync();
            });

            await trans2.RollbackAsync();

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.DuplicateKeyEntry);
        }

        [TestMethod]
        public async Task Test_Batch_Add_Key_Conflict()
        {
            await Test_Batch_Add_Key_Conflict_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_Add_Key_Conflict_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }
    }
}

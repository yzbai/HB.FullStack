using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;
using HB.FullStack.Database;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class UpdateTests : DatabaseTestClass
    {
        private async Task Test_Update_Core<T>() where T : IDbModel
        {
            var models = await AddAndRetrieve<T>();

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                foreach (var model in models)
                {
                    Mocker.Modify(model);

                    await Db.UpdateAsync(model, "", trans);
                }

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                var rts = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, models.Select(s => s.Id)), null);

                Assert.IsTrue(SerializeUtil.ToJson(models) == SerializeUtil.ToJson(rts));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Update()
        {
            await Test_Update_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Update_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Update_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Update_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Update_Core<MySql_Timeless_Long_BookModel>();
            await Test_Update_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Update_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Update_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Update_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Update_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Update_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Update_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Update_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Update_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Update_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Update_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Batch_Update_Core<T>() where T : IDbModel
        {
            var dbModels = Mocker.Mock<T>(50);

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(dbModels, "", trans);

                var rts = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, dbModels.Select(d => d.Id)), trans);

                Mocker.Modify(rts);

                await Db.UpdateAsync(rts, "lastUsre", trans).ConfigureAwait(false);

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                var rts2 = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, dbModels.Select(d => d.Id)), null);

                Assert.IsTrue(SerializeUtil.ToJson(rts) == SerializeUtil.ToJson(rts2));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Batch_Update()
        {
            await Test_Batch_Update_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_BookModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Batch_Update_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }
    }
}

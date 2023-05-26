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
    public class DeleteTests : DatabaseTestClass
    {
        private async Task Test_Batch_Delete_Core<T>() where T : IDbModel
        {
            var models = await AddAndRetrieve<T>();

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.DeleteAsync(models, "Delete", trans);

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                var rts2 = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, models.Select(s => s.Id)), null);

                Assert.IsTrue(rts2.Count == 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Batch_Delete_Core()
        {
            await Test_Batch_Delete_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Batch_Delete_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }



        private async Task Test_Delete_Core<T>() where T : IDbModel
        {
            var models = await AddAndRetrieve<T>();

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                foreach (var model in models)
                {
                    await Db.DeleteAsync(model, "lastUsre", trans).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<T>(m => SqlStatement.In(m.Id, false, models.Select(s => s.Id)), trans).ConfigureAwait(false);

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                Assert.IsTrue(count == 0);
            }
            catch (Exception ex)
            {
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Delete()
        {
            await Test_Delete_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Delete_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Delete_Core<MySql_Timeless_Long_BookModel>();
            await Test_Delete_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Delete_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Delete_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Delete_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Delete_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Delete_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }


    }
}

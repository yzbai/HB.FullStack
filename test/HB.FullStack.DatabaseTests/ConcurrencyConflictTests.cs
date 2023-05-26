using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class ConcurrencyConflictTests : DatabaseTestClass
    {
        private async Task Test_ConcurrencyConflict_Update_Core<T>() where T : IPublisherModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;
            
            if (modelDef.BestConflictCheckMethodWhenUpdate == ConflictCheckMethods.Ignore)
            {
                return;
            }

            var model = Mocker.Mock<T>(1).First();

            await Db.AddAsync(model, "tester", null);

            //client1 and client2 get data same time.
            var rt1 = await Db.ScalarAsync<T>(model.Id!, null);
            var rt2 = await Db.ScalarAsync<T>(model.Id!, null);

            //client1 update rt1
            rt1!.Name = "Update Book1";
            await Db.UpdateAsync(rt1, "test", null);


            //client2 update rt2
            var ex = await Assert.ThrowsExceptionAsync<DbException>(async () =>
            {
                rt2!.Name = "Update book2";
                await Db.UpdateAsync(rt2, "tester", null);
            });

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);
        }

        [TestMethod]
        public async Task Test_ConcurrencyConflict_Update()
        {
            await Test_ConcurrencyConflict_Update_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }
    }
}

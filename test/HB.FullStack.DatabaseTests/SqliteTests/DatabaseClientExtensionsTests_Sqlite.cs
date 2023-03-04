using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database.SQL;
using HB.FullStack.BaseTest.Data.Sqlites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.SQLite
{
    [TestClass()]
    public class DatabaseClientExtensionsTests_Sqlite : BaseTestClass
    {
        [TestMethod()]
        public async Task AddOrUpdateByIdAsyncTestAsync()
        {
            var lst = Mocker.GetCExtModels(1);
            CExtModel model = lst[0];

            await Db.AddOrUpdateByIdAsync(model, "");

            //Assert.AreEqual(model.Version, 0);

            await Db.AddOrUpdateByIdAsync(model, "");

            //Assert.AreEqual(model.Version, 1);
        }

        [TestMethod()]
        public async Task DeleteAsyncTestAsync()
        {
            IList<CExtModel> lst = Mocker.GetCExtModels();

            var trans = await Trans.BeginTransactionAsync<CExtModel>().ConfigureAwait(false);

            try
            {
                //await Db.AddAsync(lst, "Tests", trans).ConfigureAwait(false);

                foreach (var item in lst)
                {
                    await Db.AddAsync(item, "Tests", trans).ConfigureAwait(false);
                }

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }

            await Db.DeleteAsync<CExtModel>(
                e => SqlStatement.In(e.Id, false, lst.Select(e => (object)e.Id).ToArray()), "").ConfigureAwait(false);

            long count = await Db.CountAsync<CExtModel>(e => SqlStatement.In(e.Id, true, lst.Select(e => (object)e.Id).ToArray()), null).ConfigureAwait(false);

            Assert.AreEqual(count, 0);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.FullStack.Database;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using HB.FullStack.Database.SQL;
using System.Linq;
using HB.FullStack.DatabaseTests;

namespace HB.FullStack.Database.Tests
{
    [TestClass()]
    public class DatabaseClientExtensionsTests : BaseTestClass
    {
        [TestMethod()]
        public async Task AddOrUpdateByIdAsyncTestAsync()
        {
            var lst = Mocker.GetCExtModels(1);
            CExtModel model = lst[0];

            await Db.SetByIdAsync(model, null);

            //Assert.AreEqual(model.Version, 0);

            await Db.SetByIdAsync(model, null);

            //Assert.AreEqual(model.Version, 1);
        }

        [TestMethod()]
        public async Task DeleteAsyncTestAsync()
        {
            var lst = Mocker.GetCExtModels();

            var trans = await Trans.BeginTransactionAsync<CExtModel>().ConfigureAwait(false);

            try
            {
                await Db.BatchAddAsync(lst, "Tests", trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }

            await Db.DeleteAsync<CExtModel>(e => SqlStatement.In(e.Id, false, lst.Select(e => (object)e.Id).ToArray())).ConfigureAwait(false);

            long count = await Db.CountAsync<CExtModel>(e => SqlStatement.In(e.Id, true, lst.Select(e => (object)e.Id).ToArray()), null).ConfigureAwait(false);

            Assert.AreEqual(count, 0);
        }
    }
}
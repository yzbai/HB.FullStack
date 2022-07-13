using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.FullStack.DatabaseTests.SQLite
{
    [TestClass]
    public class JoinTest_Sqlite : BaseTestClass
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            AddSomeDataAsync(Db).Wait();
        }

        private static async Task AddSomeDataAsync(IDatabase database)
        {
            A_Client a1 = new A_Client { Name = "a1" };
            A_Client a2 = new A_Client { Name = "a2" };
            A_Client a3 = new A_Client { Name = "a3" };

            B_Client b1 = new B_Client { Name = "b1" };
            B_Client b2 = new B_Client { Name = "b2" };

            await database.AddAsync(a2, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a3, "lastUsre", null).ConfigureAwait(false);

            await database.AddAsync(b1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(b2, "lastUsre", null).ConfigureAwait(false);

            AB_Client a1b1 = new AB_Client { AId = a1.Id, BId = b1.Id };
            AB_Client a1b2 = new AB_Client { AId = a1.Id, BId = b2.Id };
            AB_Client a2b1 = new AB_Client { AId = a2.Id, BId = b1.Id };
            AB_Client a3b2 = new AB_Client { AId = a3.Id, BId = b2.Id };

            C_Client c1 = new C_Client { AId = a1.Id };
            C_Client c2 = new C_Client { AId = a2.Id };
            C_Client c3 = new C_Client { AId = a3.Id };
            C_Client c4 = new C_Client { AId = a1.Id };
            C_Client c5 = new C_Client { AId = a2.Id };
            C_Client c6 = new C_Client { AId = a3.Id };

            await database.AddAsync(a1b1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a1b2, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a2b1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a3b2, "lastUsre", null).ConfigureAwait(false);

            await database.AddAsync(c1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(c2, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(c3, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(c4, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(c5, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(c6, "lastUsre", null).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task Test_1_ThreeTable_JoinTestAsync()
        {
            var from = Db
                .From<A_Client>()
                .LeftJoin<AB_Client>((a, ab) => ab.AId == a.Id)
                .LeftJoin<AB_Client, B_Client>((ab, b) => ab.BId == b.Id);

            try
            {
                IEnumerable<Tuple<A_Client, AB_Client?, B_Client?>>? result = await Db.RetrieveAsync<A_Client, AB_Client, B_Client>(from, Db.Where<A_Client>(), null).ConfigureAwait(false);
                Assert.IsTrue(result.Any());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        [TestMethod]
        public async Task Test_2_TwoTable_JoinTestAsync()
        {
            var from = Db
                .From<C_Client>()
                .LeftJoin<A_Client>((c, a) => c.AId == a.Id);

            try
            {
                IEnumerable<Tuple<C_Client, A_Client?>>? result = await Db.RetrieveAsync<C_Client, A_Client>(from, Db.Where<C_Client>(), null).ConfigureAwait(false);
                Assert.IsTrue(result.Any());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }
    }
}
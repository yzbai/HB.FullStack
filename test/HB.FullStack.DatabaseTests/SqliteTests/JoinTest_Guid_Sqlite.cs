using HB.FullStack.Database;
using HB.FullStack.DatabaseTests.Data.Sqlites;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.FullStack.DatabaseTests.SQLite
{
    [TestClass]
    public class JoinTest_Guid_Sqlite : BaseTestClass
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            AddSomeDataAsync(Db).Wait();
        }

        private static async Task AddSomeDataAsync(IDatabase database)
        {
            Guid_A_Client a1 = new Guid_A_Client { Name = "a1" };
            Guid_A_Client a2 = new Guid_A_Client { Name = "a2" };
            Guid_A_Client a3 = new Guid_A_Client { Name = "a3" };

            Guid_B_Client b1 = new Guid_B_Client { Name = "b1" };
            Guid_B_Client b2 = new Guid_B_Client { Name = "b2" };

            await database.AddAsync(a2, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a3, "lastUsre", null).ConfigureAwait(false);

            await database.AddAsync(b1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(b2, "lastUsre", null).ConfigureAwait(false);

            Guid_AB_Client a1b1 = new Guid_AB_Client { Guid_AId = a1.Id, Guid_BId = b1.Id };
            Guid_AB_Client a1b2 = new Guid_AB_Client { Guid_AId = a1.Id, Guid_BId = b2.Id };
            Guid_AB_Client a2b1 = new Guid_AB_Client { Guid_AId = a2.Id, Guid_BId = b1.Id };
            Guid_AB_Client a3b2 = new Guid_AB_Client { Guid_AId = a3.Id, Guid_BId = b2.Id };

            Guid_C_Client c1 = new Guid_C_Client { Guid_AId = a1.Id };
            Guid_C_Client c2 = new Guid_C_Client { Guid_AId = a2.Id };
            Guid_C_Client c3 = new Guid_C_Client { Guid_AId = a3.Id };
            Guid_C_Client c4 = new Guid_C_Client { Guid_AId = a1.Id };
            Guid_C_Client c5 = new Guid_C_Client { Guid_AId = a2.Id };
            Guid_C_Client c6 = new Guid_C_Client { Guid_AId = a3.Id };

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
                .From<Guid_A_Client>()
                .LeftJoin<Guid_AB_Client>((a, ab) => ab.Guid_AId == a.Id)
                .LeftJoin<Guid_AB_Client, Guid_B_Client>((ab, b) => ab.Guid_BId == b.Id);

            try
            {
                IEnumerable<Tuple<Guid_A_Client, Guid_AB_Client?, Guid_B_Client?>>? result =
                    await Db.RetrieveAsync<Guid_A_Client, Guid_AB_Client, Guid_B_Client>(from, Db.Where<Guid_A_Client>(), null).ConfigureAwait(false);
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
                .From<Guid_C_Client>()
                .LeftJoin<Guid_A_Client>((c, a) => c.Guid_AId == a.Id);

            try
            {
                IEnumerable<Tuple<Guid_C_Client, Guid_A_Client?>>? result = await Db.RetrieveAsync<Guid_C_Client, Guid_A_Client>(from, Db.Where<Guid_C_Client>(), null).ConfigureAwait(false);
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
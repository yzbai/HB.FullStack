using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class JoinTest_MySQL_Guid : BaseTestClass
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            Guid_AddSomeDataAsync(Db).Wait();
        }

        private static async Task Guid_AddSomeDataAsync(IDatabase database)
        {
            Guid_A a1 = new Guid_A { Name = "a1" };
            Guid_A a2 = new Guid_A { Name = "a2" };
            Guid_A a3 = new Guid_A { Name = "a3" };

            Guid_B b1 = new Guid_B { Name = "b1" };
            Guid_B b2 = new Guid_B { Name = "b2" };

            await database.AddAsync(a2, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a3, "lastUsre", null).ConfigureAwait(false);

            await database.AddAsync(b1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(b2, "lastUsre", null).ConfigureAwait(false);

            Guid_AB a1b1 = new Guid_AB { Guid_AId = a1.Id, Guid_BId = b1.Id };
            Guid_AB a1b2 = new Guid_AB { Guid_AId = a1.Id, Guid_BId = b2.Id };

            Guid_AB a2b1 = new Guid_AB { Guid_AId = a2.Id, Guid_BId = b1.Id };
            Guid_AB a3b2 = new Guid_AB { Guid_AId = a3.Id, Guid_BId = b2.Id };

            Guid_C c1 = new Guid_C { Guid_AId = a1.Id };
            Guid_C c2 = new Guid_C { Guid_AId = a2.Id };
            Guid_C c3 = new Guid_C { Guid_AId = a3.Id };
            Guid_C c4 = new Guid_C { Guid_AId = a1.Id };
            Guid_C c5 = new Guid_C { Guid_AId = a2.Id };
            Guid_C c6 = new Guid_C { Guid_AId = a3.Id };

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
        public async Task Guid_Test_1_ThreeTable_JoinTestAsync()
        {
            var from = Db
                .From<Guid_A>()
                .LeftJoin<Guid_AB>((a, ab) => ab.Guid_AId == a.Id)
                .LeftJoin<Guid_AB, Guid_B>((ab, b) => ab.Guid_BId == b.Id);

            try
            {
                IEnumerable<Tuple<Guid_A, Guid_AB?, Guid_B?>>? result = await Db.RetrieveAsync<Guid_A, Guid_AB, Guid_B>(
                    from, Db.Where<Guid_A>(), null).ConfigureAwait(false);
                Assert.IsTrue(result.Any());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        [TestMethod]
        public async Task Guid_Test_2_TwoTable_JoinTestAsync()
        {
            var from = Db
                .From<Guid_C>()
                .LeftJoin<Guid_A>((c, a) => c.Guid_AId == a.Id);

            try
            {
                IEnumerable<Tuple<Guid_C, Guid_A?>>? result = await Db.RetrieveAsync<Guid_C, Guid_A>(
                    from, Db.Where<Guid_C>(), null).ConfigureAwait(false);
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
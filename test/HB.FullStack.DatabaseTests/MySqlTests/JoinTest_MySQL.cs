using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.BaseTest.Data.MySqls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.MySQL
{
    [TestClass]
    public class JoinTest_MySQL : BaseTestClass
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            AddSomeDataAsync(Db).Wait();
        }

        private static async Task AddSomeDataAsync(IDatabase database)
        {
            A a1 = new A { Name = "a1" };
            A a2 = new A { Name = "a2" };
            A a3 = new A { Name = "a3" };

            B b1 = new B { Name = "b1" };
            B b2 = new B { Name = "b2" };

            await database.AddAsync(a2, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(a3, "lastUsre", null).ConfigureAwait(false);

            await database.AddAsync(b1, "lastUsre", null).ConfigureAwait(false);
            await database.AddAsync(b2, "lastUsre", null).ConfigureAwait(false);

            AB a1b1 = new AB { AId = a1.Id, BId = b1.Id };
            AB a1b2 = new AB { AId = a1.Id, BId = b2.Id };

            AB a2b1 = new AB { AId = a2.Id, BId = b1.Id };
            AB a3b2 = new AB { AId = a3.Id, BId = b2.Id };

            C c1 = new C { AId = a1.Id };
            C c2 = new C { AId = a2.Id };
            C c3 = new C { AId = a3.Id };
            C c4 = new C { AId = a1.Id };
            C c5 = new C { AId = a2.Id };
            C c6 = new C { AId = a3.Id };

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
                .From<A>()
                .LeftJoin<AB>((a, ab) => ab.AId == a.Id)
                .LeftJoin<AB, B>((ab, b) => ab.BId == b.Id);

            try
            {
                IEnumerable<Tuple<A, AB?, B?>>? result = await Db.RetrieveAsync<A, AB, B>(from, Db.Where<A>(), null).ConfigureAwait(false);
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
                .From<C>()
                .LeftJoin<A>((c, a) => c.AId == a.Id);

            try
            {
                IEnumerable<Tuple<C, A?>>? result = await Db.RetrieveAsync<C, A>(from, Db.Where<C>(), null).ConfigureAwait(false);
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
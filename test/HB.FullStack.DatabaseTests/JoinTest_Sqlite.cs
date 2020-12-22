using HB.FullStack.Common.Entities;
using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.DatabaseTests
{
    //[TestCaseOrderer("HB.FullStack.Database.Test.TestCaseOrdererByTestName", "HB.FullStack.Database.Test")]
    public class JoinTest_Sqlite : IClassFixture<ServiceFixture_Sqlite>
    {
        private readonly IDatabase _sqlite;
        private readonly ITestOutputHelper _output;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public JoinTest_Sqlite(ITestOutputHelper testOutputHelper, ServiceFixture_Sqlite serviceFixture)
        {
            _output = testOutputHelper;

            _sqlite = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();

            AddSomeDataAsync().Wait();

        }

        /// <summary>
        /// AddSomeDataAsync
        /// </summary>
        /// <returns></returns>
        private async Task AddSomeDataAsync()
        {
            A a1 = new A { Name = "a1" };
            A a2 = new A { Name = "a2" };
            A a3 = new A { Name = "a3" };

            B b1 = new B { Name = "b1" };
            B b2 = new B { Name = "b2" };

            await _sqlite.AddAsync(a2, "lastUsre", null);
            await _sqlite.AddAsync(a1, "lastUsre", null);
            await _sqlite.AddAsync(a3, "lastUsre", null);

            await _sqlite.AddAsync(b1, "lastUsre", null);
            await _sqlite.AddAsync(b2, "lastUsre", null);

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

            await _sqlite.AddAsync(a1b1, "lastUsre", null);
            await _sqlite.AddAsync(a1b2, "lastUsre", null);
            await _sqlite.AddAsync(a2b1, "lastUsre", null);
            await _sqlite.AddAsync(a3b2, "lastUsre", null);

            await _sqlite.AddAsync(c1, "lastUsre", null);
            await _sqlite.AddAsync(c2, "lastUsre", null);
            await _sqlite.AddAsync(c3, "lastUsre", null);
            await _sqlite.AddAsync(c4, "lastUsre", null);
            await _sqlite.AddAsync(c5, "lastUsre", null);
            await _sqlite.AddAsync(c6, "lastUsre", null);
        }

        [Fact]
        public async Task Test_1_ThreeTable_JoinTestAsync()
        {
            IDatabase database = _sqlite;

            var from = database
                .From<A>()
                .LeftJoin<AB>((a, ab) => ab.AId == a.Id)
                .LeftJoin<AB, B>((ab, b) => ab.BId == b.Id);


            try
            {
                IEnumerable<Tuple<A, AB?, B?>>? result = await database.RetrieveAsync<A, AB, B>(from, database.Where<A>(), null);
                Assert.True(result.Count() > 0);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);

                throw ex;
            }


        }

        [Fact]
        public async Task Test_2_TwoTable_JoinTestAsync()
        {
            IDatabase database = _sqlite;
            var from = database
                .From<C>()
                .LeftJoin<A>((c, a) => c.AId == a.Id);


            try
            {
                IEnumerable<Tuple<C, A?>>? result = await database.RetrieveAsync<C, A>(from, database.Where<C>(), null).ConfigureAwait(false);
                Assert.True(result.Count() > 0);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);

                throw ex;
            }
        }
    }
}

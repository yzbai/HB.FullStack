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
            A_Client a1 = new A_Client { Name = "a1" };
            A_Client a2 = new A_Client { Name = "a2" };
            A_Client a3 = new A_Client { Name = "a3" };

            B_Client b1 = new B_Client { Name = "b1" };
            B_Client b2 = new B_Client { Name = "b2" };

            await _sqlite.AddAsync(a2, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(a1, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(a3, "lastUsre", null).ConfigureAwait(false);

            await _sqlite.AddAsync(b1, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(b2, "lastUsre", null).ConfigureAwait(false);

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

            await _sqlite.AddAsync(a1b1, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(a1b2, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(a2b1, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(a3b2, "lastUsre", null).ConfigureAwait(false);

            await _sqlite.AddAsync(c1, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(c2, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(c3, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(c4, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(c5, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(c6, "lastUsre", null).ConfigureAwait(false);
        }

        [Fact]
        public async Task Test_1_ThreeTable_JoinTestAsync()
        {
            IDatabase database = _sqlite;

            var from = database
                .From<A_Client>()
                .LeftJoin<AB_Client>((a, ab) => ab.AId == a.Id)
                .LeftJoin<AB_Client, B_Client>((ab, b) => ab.BId == b.Id);


            try
            {
                IEnumerable<Tuple<A_Client, AB_Client?, B_Client?>>? result = await database.RetrieveAsync<A_Client, AB_Client, B_Client>(from, database.Where<A_Client>(), null).ConfigureAwait(false);
                Assert.True(result.Any());
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);

                throw;
            }


        }

        [Fact]
        public async Task Test_2_TwoTable_JoinTestAsync()
        {
            IDatabase database = _sqlite;
            var from = database
                .From<C_Client>()
                .LeftJoin<A_Client>((c, a) => c.AId == a.Id);


            try
            {
                IEnumerable<Tuple<C_Client, A_Client?>>? result = await database.RetrieveAsync<C_Client, A_Client>(from, database.Where<C_Client>(), null).ConfigureAwait(false);
                Assert.True(result.Any());
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);

                throw;
            }
        }
    }
}

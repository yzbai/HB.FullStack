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
    public class JoinTest_Sqlite_Guid : IClassFixture<ServiceFixture_Sqlite>
    {
        private readonly IDatabase _sqlite;
        private readonly ITestOutputHelper _output;

        public JoinTest_Sqlite_Guid(ITestOutputHelper testOutputHelper, ServiceFixture_Sqlite serviceFixture)
        {
            _output = testOutputHelper;

            _sqlite = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();

            AddSomeDataAsync().Wait();

        }

        /// <summary>
        /// AddSomeDataAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        private async Task AddSomeDataAsync()
        {
            Guid_A_Client a1 = new Guid_A_Client { Name = "a1" };
            Guid_A_Client a2 = new Guid_A_Client { Name = "a2" };
            Guid_A_Client a3 = new Guid_A_Client { Name = "a3" };
                                   
            Guid_B_Client b1 = new Guid_B_Client { Name = "b1" };
            Guid_B_Client b2 = new Guid_B_Client { Name = "b2" };

            await _sqlite.AddAsync(a2, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(a1, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(a3, "lastUsre", null).ConfigureAwait(false);

            await _sqlite.AddAsync(b1, "lastUsre", null).ConfigureAwait(false);
            await _sqlite.AddAsync(b2, "lastUsre", null).ConfigureAwait(false);

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

        /// <summary>
        /// Test_1_ThreeTable_JoinTestAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Test_1_ThreeTable_JoinTestAsync()
        {
            IDatabase database = _sqlite;

            var from = database
                .From<Guid_A_Client>()
                .LeftJoin<Guid_AB_Client>((a, ab) => ab.Guid_AId == a.Id)
                .LeftJoin<Guid_AB_Client, Guid_B_Client>((ab, b) => ab.Guid_BId == b.Id);


            try
            {
                IEnumerable<Tuple<Guid_A_Client, Guid_AB_Client?, Guid_B_Client?>>? result = 
                    await database.RetrieveAsync<Guid_A_Client, Guid_AB_Client, Guid_B_Client>(from, database.Where<Guid_A_Client>(), null).ConfigureAwait(false);
                Assert.True(result.Any());
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);

                throw;
            }


        }

        /// <summary>
        /// Test_2_TwoTable_JoinTestAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Test_2_TwoTable_JoinTestAsync()
        {
            IDatabase database = _sqlite;
            var from = database
                .From<Guid_C_Client>()
                .LeftJoin<Guid_A_Client>((c, a) => c.Guid_AId == a.Id);


            try
            {
                IEnumerable<Tuple<Guid_C_Client, Guid_A_Client?>>? result = await database.RetrieveAsync<Guid_C_Client, Guid_A_Client>(from, database.Where<Guid_C_Client>(), null).ConfigureAwait(false);
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

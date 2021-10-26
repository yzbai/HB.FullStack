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
    public class JoinTest_MySQL_Guid : IClassFixture<ServiceFixture_MySql>
    {
        private readonly IDatabase _mysql;
        private readonly ITestOutputHelper _output;

        public JoinTest_MySQL_Guid(ITestOutputHelper testOutputHelper, ServiceFixture_MySql serviceFixture)
        {
            _output = testOutputHelper;

            _mysql = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();
            Guid_AddSomeDataAsync().Wait();

        }


        /// <summary>
        /// AddSomeDataAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        private async Task Guid_AddSomeDataAsync()
        {
            Guid_A a1 = new Guid_A { Name = "a1" };
            Guid_A a2 = new Guid_A { Name = "a2" };
            Guid_A a3 = new Guid_A { Name = "a3" };

            Guid_B b1 = new Guid_B { Name = "b1" };
            Guid_B b2 = new Guid_B { Name = "b2" };



            await _mysql.AddAsync(a2, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(a1, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(a3, "lastUsre", null).ConfigureAwait(false);

            await _mysql.AddAsync(b1, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(b2, "lastUsre", null).ConfigureAwait(false);

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

            await _mysql.AddAsync(a1b1, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(a1b2, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(a2b1, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(a3b2, "lastUsre", null).ConfigureAwait(false);

            await _mysql.AddAsync(c1, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(c2, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(c3, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(c4, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(c5, "lastUsre", null).ConfigureAwait(false);
            await _mysql.AddAsync(c6, "lastUsre", null).ConfigureAwait(false);

        }


        /// <summary>
        /// Test_1_ThreeTable_JoinTestAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Fact]
        public async Task Guid_Test_1_ThreeTable_JoinTestAsync()
        {
            IDatabase database = _mysql;

            var from = database
                .From<Guid_A>()
                .LeftJoin<Guid_AB>((a, ab) => ab.Guid_AId == a.Id)
                .LeftJoin<Guid_AB, Guid_B>((ab, b) => ab.Guid_BId == b.Id);


            try
            {
                IEnumerable<Tuple<Guid_A, Guid_AB?, Guid_B?>>? result = await database.RetrieveAsync<Guid_A, Guid_AB, Guid_B>(
                    from, database.Where<Guid_A>(), null).ConfigureAwait(false);
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
        public async Task Guid_Test_2_TwoTable_JoinTestAsync()
        {
            IDatabase database = _mysql;
            var from = database
                .From<Guid_C>()
                .LeftJoin<Guid_A>((c, a) => c.Guid_AId == a.Id);


            try
            {
                IEnumerable<Tuple<Guid_C, Guid_A?>>? result = await database.RetrieveAsync<Guid_C, Guid_A>(
                    from, database.Where<Guid_C>(), null).ConfigureAwait(false);
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

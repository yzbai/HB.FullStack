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
    public class MutipleTableTest : IClassFixture<ServiceFixture>
    {
        private readonly IDatabase _mysql;
        private readonly IDatabase _sqlite;
        private readonly ITestOutputHelper _output;

        /// <exception cref="System.ArgumentException"></exception>
        private IDatabase GetDatabase(string databaseType) =>
            databaseType switch
            {
                "MySQL" => _mysql,
                "SQLite" => _sqlite,
                _ => throw new ArgumentException(nameof(databaseType))
            };


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="testOutputHelper"></param>
        /// <param name="serviceFixture"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        /// <exception cref="AggregateException">Ignore.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public MutipleTableTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            _output = testOutputHelper;

            _mysql = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();
            _sqlite = serviceFixture.ServiceProvider2.GetRequiredService<IDatabase>();

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            AddSomeDataAsync().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

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

            AB a1b1 = new AB { AId = a1.Guid, BId = b1.Guid };
            AB a1b2 = new AB { AId = a1.Guid, BId = b2.Guid };

            AB a2b1 = new AB { AId = a2.Guid, BId = b1.Guid };
            AB a3b2 = new AB { AId = a3.Guid, BId = b2.Guid };

            C c1 = new C { AId = a1.Guid };
            C c2 = new C { AId = a2.Guid };
            C c3 = new C { AId = a3.Guid };
            C c4 = new C { AId = a1.Guid };
            C c5 = new C { AId = a2.Guid };
            C c6 = new C { AId = a3.Guid };

            await _mysql.AddAsync(a2, "lastUsre", null);
            await _mysql.AddAsync(a1, "lastUsre", null);
            await _mysql.AddAsync(a3, "lastUsre", null);

            await _mysql.AddAsync(b1, "lastUsre", null);
            await _mysql.AddAsync(b2, "lastUsre", null);

            await _mysql.AddAsync(a1b1, "lastUsre", null);
            await _mysql.AddAsync(a1b2, "lastUsre", null);
            await _mysql.AddAsync(a2b1, "lastUsre", null);
            await _mysql.AddAsync(a3b2, "lastUsre", null);

            await _mysql.AddAsync(c1, "lastUsre", null);
            await _mysql.AddAsync(c2, "lastUsre", null);
            await _mysql.AddAsync(c3, "lastUsre", null);
            await _mysql.AddAsync(c4, "lastUsre", null);
            await _mysql.AddAsync(c5, "lastUsre", null);
            await _mysql.AddAsync(c6, "lastUsre", null);

            await _sqlite.AddAsync(a2, "lastUsre", null);
            await _sqlite.AddAsync(a1, "lastUsre", null);
            await _sqlite.AddAsync(a3, "lastUsre", null);

            await _sqlite.AddAsync(b1, "lastUsre", null);
            await _sqlite.AddAsync(b2, "lastUsre", null);

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

        /// <summary>
        /// Test_1_ThreeTable_JoinTestAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]
        [InlineData("MySQL")]
        [InlineData("SQLite")]
        public async Task Test_1_ThreeTable_JoinTestAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType);

            var from = database
                .From<A>()
                .LeftJoin<AB>((a, ab) => ab.AId == a.Guid)
                .LeftJoin<AB, B>((ab, b) => ab.BId == b.Guid);


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

        /// <summary>
        /// Test_2_TwoTable_JoinTestAsync
        /// </summary>
        /// <param name="databaseType"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Ignore.</exception>
        [Theory]
        [InlineData("MySQL")]
        [InlineData("SQLite")]
        public async Task Test_2_TwoTable_JoinTestAsync(string databaseType)
        {
            IDatabase database = GetDatabase(databaseType);
            var from = database
                .From<C>()
                .LeftJoin<A>((c, a) => c.AId == a.Guid);


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

    [DatabaseEntity]
    public class A : Entity
    {


        [EntityProperty]
        public string Name { get; set; } = default!;
    }

    [DatabaseEntity]
    public class B : Entity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }

    [DatabaseEntity]
    public class AB : Entity
    {

        [EntityProperty]
        public string AId { get; set; } = default!;

        [EntityProperty]
        public string BId { get; set; } = default!;
    }

    [DatabaseEntity]
    public class C : Entity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public string AId { get; set; } = default!;
    }

}

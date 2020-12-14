using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.Infrastructure.Redis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.DatabaseTests
{
    public class MultipleInitializeTest : IClassFixture<ServiceFixture_MySql>
    {

        private readonly IDatabase _mysql;
        private readonly ITransaction _mysqlTransaction;
        private readonly ITestOutputHelper _output;

        public MultipleInitializeTest(ITestOutputHelper testOutputHelper, ServiceFixture_MySql serviceFixture)
        {
            _output = testOutputHelper;

            _mysql = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();
            _mysqlTransaction = serviceFixture.ServiceProvider.GetRequiredService<ITransaction>();

        }

        [Fact]
        public async Task Test_ConcurrenceAsync()
        {

            IDatabase database = _mysql;

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 20; ++i)
            {
                tasks.Add(database.InitializeAsync());
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }


    }
}

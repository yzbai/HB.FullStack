using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;

using OrmBenchmark.Core;

namespace HB.FullStack.Benchmark.Database
{
    public class FullStackDatabaseExecutor : IOrmExecuter
    {
        private IDatabase _database = null!;

        public string Name => "HB.FullStack.Database";

        public void Init(string connectionStrong)
        {
            ServiceFixture serviceFixture = new ServiceFixture();

            _database = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();

            _database.InitializeAsync().Wait();
        }

        public IPost GetItemAsObject(int Id)
        {
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return _database.ScalarAsync<Post>(Id, null).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
#pragma warning restore CS8603 // Possible null reference return.
        }

        public dynamic GetItemAsDynamic(int Id)
        {
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return null;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
#pragma warning restore CS8603 // Possible null reference return.
        }

        public IEnumerable<IPost> GetAllItemsAsObject()
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return _database.RetrieveAllAsync<Post>(null).Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public IEnumerable<dynamic>? GetAllItemsAsDynamic()
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return null;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public void Finish()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database;

using Microsoft.Extensions.DependencyInjection;

using OrmBenchmark.Core;

namespace HB.FullStack.Benchmark.Database
{
    public class FullStackDatabaseExecutor : IOrmExecuter
    {
        private IDatabase _database = null!;
        private ITransaction _transaction = null!;

        private TransactionContext? _transactionContext;

        public string Name => "HB.FullStack.Database";

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="connectionStrong"></param>
        /// <exception cref="DatabaseException">Ignore.</exception>
        public void Init(string connectionStrong)
        {
            ServiceFixture serviceFixture = new ServiceFixture();

            _database = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();

            _database.InitializeAsync().Wait();

            _transaction = serviceFixture.ServiceProvider.GetRequiredService<ITransaction>();

            _transactionContext = _transaction.BeginTransactionAsync<Post>(System.Data.IsolationLevel.ReadUncommitted).Result;
        }

        public async Task<IPost?> GetItemAsObjectAsync(int Id)
        {
            return await _database.ScalarAsync<Post>(Id, _transactionContext).ConfigureAwait(false);
        }

        public dynamic GetItemAsDynamic(int Id)
        {
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return null;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<IEnumerable<IPost>> GetAllItemsAsObjectAsync()
        {
            return await _database.RetrieveAllAsync<Post>(null).ConfigureAwait(false);
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

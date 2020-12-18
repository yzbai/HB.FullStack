using System;
using System.Linq;
using System.Threading.Tasks;
using HB.FullStack.Database.ClientExtension;
using HB.FullStack.Database.ClientExtensions.Tests.Context;
using HB.FullStack.Database.SQL;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HB.FullStack.Database.ClientExtensions.Tests
{
    public class DatabaseClientExtensionsTest : IClassFixture<ServiceFixture_Sqlite>
    {
        private readonly IDatabase _database;
        private readonly ITransaction _transaction;
        public DatabaseClientExtensionsTest(ServiceFixture_Sqlite serviceFixture)
        {
            _database = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();
            _transaction = serviceFixture.ServiceProvider.GetRequiredService<ITransaction>();
        }
        [Fact]
        public async Task Delete_TestAsync()
        {
            var lst = Mocker.GetCExtEntities();

            var trans = await _transaction.BeginTransactionAsync<CExtEntity>().ConfigureAwait(false);

            try
            {
                await _database.BatchAddAsync(lst, "Tests", trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }


            await _database.DeleteAsync<CExtEntity>(e => SqlStatement.In(e.Id, false, lst.Select(e => (object)e.Id).ToArray())).ConfigureAwait(false);

            long count = await _database.CountAsync<CExtEntity>(e => SqlStatement.In(e.Id, true, lst.Select(e => (object)e.Id).ToArray()), null).ConfigureAwait(false);

            Assert.True(count == 0);
        }
    }
}

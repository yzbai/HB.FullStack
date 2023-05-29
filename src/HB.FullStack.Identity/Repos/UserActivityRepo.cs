using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Identity
{
    public class UserActivityRepo<TId> : DbModelRepository<UserActivity<TId>>
    {
        public UserActivityRepo(ILogger<UserActivityRepo<TId>> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(object sender, ModelChangeEventArgs args) => Task.CompletedTask;
    }
}

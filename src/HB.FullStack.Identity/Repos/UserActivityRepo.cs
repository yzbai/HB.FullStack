using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Common.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.DBModels;
using HB.FullStack.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Identity
{
    public class UserActivityRepo : ModelRepository<UserActivity>
    {
        public UserActivityRepo(ILogger<UserActivityRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }


        protected override Task InvalidateCacheItemsOnChanged(IEnumerable<DBModel> sender, DBChangedEventArgs args) => Task.CompletedTask;
    }
}

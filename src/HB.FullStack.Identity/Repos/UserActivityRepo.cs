using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Identity
{
    public class UserActivityRepo : ModelRepository<UserActivity>
    {
        public UserActivityRepo(ILogger<UserActivityRepo> logger, IDatabaseReader databaseReader, IModelCache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(UserActivity sender, DatabaseWriteEventArgs args) => Task.CompletedTask;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;
using HB.FullStack.Server.Identity.Models;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Identity.Repos
{
    public class UserProfileRepo : ModelRepository<UserProfile>
    {
        public UserProfileRepo(ILogger<UserProfileRepo> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
        }

        protected override Task InvalidateCacheItemsOnChanged(object sender, DBChangeEventArgs args)
        {
            return Task.CompletedTask;
        }

        internal Task<UserProfile?> GetByUserIdAsync(Guid userId, TransactionContext trans)
        {
            return DbReader.ScalarAsync<UserProfile>(up => up.UserId == userId, trans);
        }
    }
}

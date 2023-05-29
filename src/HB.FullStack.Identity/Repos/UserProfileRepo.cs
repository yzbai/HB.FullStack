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
    public class UserProfileRepo<TId> : DbModelRepository<UserProfile<TId>>
    {
        public UserProfileRepo(ILogger<UserProfileRepo<TId>> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
        }

        protected override Task InvalidateCacheItemsOnChanged(object sender, ModelChangeEventArgs args)
        {
            return Task.CompletedTask;
        }

        internal Task<UserProfile<TId>?> GetByUserIdAsync(TId userId, TransactionContext trans)
        {
            return DbReader.ScalarAsync<UserProfile<TId>>(up => up.UserId!.Equals(userId), trans);
        }
    }
}

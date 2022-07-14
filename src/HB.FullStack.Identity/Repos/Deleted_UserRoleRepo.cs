using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Identity.Models;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;
using HB.FullStack.Common;

namespace HB.FullStack.Identity
{
    //TODO: 先忽略掉UserRole这种关系表的Repo好了。
    public class Deleted_UserRoleRepo : ModelRepository<UserRole>
    {
        public Deleted_UserRoleRepo(ILogger<Deleted_UserRoleRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(UserRole sender, DatabaseWriteEventArgs args)
        {
            InvalidateCache(new CachedRolesByUserId(sender.UserId).SetTimestamp(args.Timestamp));
            return Task.CompletedTask;
        }

        internal async Task<UserRole?> GetByUserIdAndRoleIdAsync(Guid userId, Guid roleId, TransactionContext? transactionContext)
        {
            UserRole? userRole = await DbReader.ScalarAsync<UserRole>(ur => ur.UserId == userId && ur.RoleId == roleId, transactionContext).ConfigureAwait(false);
            return userRole;
        }
    }
}
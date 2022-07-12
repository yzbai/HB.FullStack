using HB.FullStack.Identity.Models;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    public class RoleRepo : ModelRepository<Role>
    {
        public RoleRepo(ILogger<RoleRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override async Task InvalidateCacheItemsOnChanged(Role sender, DatabaseWriteEventArgs args)
        {
            //解决方案1：删除所有。Role改变，对于CachedRolesByUserId来说，就是Values变了，所以要全部Invalidate
            //问题：速度慢
            //InvalidateCacheByCacheType(new CachedRolesByUserId().Timestamp(args.UtcNowTicks));

            //解决方案2：找到相关的UserId，删除
            //问题：可能会有很多cache条目
            IEnumerable<UserRole> userRoles = await DbReader.RetrieveAsync<UserRole>(ur => ur.RoleId == sender.Id, null).ConfigureAwait(false);

            InvalidateCache(userRoles.Select(ur => new CachedRolesByUserId(ur.UserId)).ToList());
        }

        public Task<IEnumerable<Role>> GetByUserIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return GetUsingCacheAsideAsync(new CachedRolesByUserId(userId), dbReader =>
            {
                var from = dbReader.From<Role>().RightJoin<UserRole>((r, ru) => r.Id == ru.RoleId);
                var where = dbReader.Where<Role>().And<UserRole>(ru => ru.UserId == userId);
                return dbReader.RetrieveAsync(from, where, transContext);
            })!;
        }
    }
}
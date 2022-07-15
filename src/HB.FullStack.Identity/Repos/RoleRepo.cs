using System.Collections.Generic;
using System.Linq;
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
    public class RoleRepo : ModelRepository<Role>
    {
        public RoleRepo(ILogger<RoleRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override async Task InvalidateCacheItemsOnChanged(IEnumerable<DBModel> sender, DBChangedEventArgs args)
        {
            //解决方案1：删除所有。Role改变，对于CachedRolesByUserId来说，就是Values变了，所以要全部Invalidate
            //问题：速度慢
            //InvalidateCacheByCacheType(new CachedRolesByUserId().Timestamp(args.UtcNowTicks));

            //解决方案2：找到相关的UserId，删除
            //问题：可能会有很多cache条目

            //TODO: 解决方案3：删除某个前缀的所有key

            if (args.ChangeType == DBChangeType.Update || args.ChangeType == DBChangeType.Delete)
            {
                if (sender is IEnumerable<Role> roles)
                {
                    foreach (var role in roles)
                    {
                        IEnumerable<UserRole> userRoles = await DbReader.RetrieveAsync<UserRole>(ur => ur.RoleId == role.Id, null).ConfigureAwait(false);

                        InvalidateCache(userRoles.Select(ur => new CachedRolesByUserId(ur.UserId)).ToList());
                    }
                }
            }
        }
    }
}
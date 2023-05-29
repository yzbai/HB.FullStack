using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Identity
{
    public class RoleRepo<TId> : DbModelRepository<Role<TId>>
    {
        public RoleRepo(ILogger<RoleRepo<TId>> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override async Task InvalidateCacheItemsOnChanged(object sender, ModelChangeEventArgs args)
        {
            //解决方案1：删除所有。Role改变，对于CachedRolesByUserId来说，就是Values变了，所以要全部Invalidate
            //问题：速度慢
            //InvalidateCacheByCacheType(new CachedRolesByUserId().Timestamp(args.UtcNowTicks));

            //解决方案2：找到相关的UserId，删除
            //问题：可能会有很多cache条目

            //TODO: 解决方案3：删除某个前缀的所有key

            if (args.ChangeType == ModelChangeType.Update || args.ChangeType == ModelChangeType.Delete || args.ChangeType == ModelChangeType.UpdateProperties)
            {
                if (sender is IEnumerable<Role<TId>> roles)
                {
                    IEnumerable<TId> roleIdList = roles.Select(r => r.Id).ToList();

                    await InvalidCachedRolesByUserId(roleIdList).ConfigureAwait(false);
                }
                else if (sender is IEnumerable<PropertyChangePack> cpps)
                {
                    IEnumerable<TId> roleIdList = cpps.Select(cpp => SerializeUtil.To<TId>(cpp.AddtionalProperties[nameof(Role<TId>.Id)])!);

                    await InvalidCachedRolesByUserId(roleIdList).ConfigureAwait(false);
                }
                else
                {
                    throw CommonExceptions.UnkownEventSender(sender);
                }
            }

            async Task InvalidCachedRolesByUserId(IEnumerable<TId> roleIdList)
            {
                foreach (TId id in roleIdList)
                {
                    IEnumerable<UserRole<TId>> userRoles = await DbReader.RetrieveAsync<UserRole<TId>>(ur => ur.RoleId!.Equals(id), null).ConfigureAwait(false);

                    InvalidateCache(userRoles.Select(ur => new CachedRolesByUserId<TId>(ur.UserId)).ToList());
                }
            }
        }
    }
}
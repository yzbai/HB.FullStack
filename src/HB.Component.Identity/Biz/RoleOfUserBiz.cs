using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Identity.Entities;
using HB.FullStack.Business;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Identity
{
    public class RoleOfUserBiz : BaseEntityBiz<RoleOfUser>
    {
        private readonly IDatabaseReader _databaseReader;
        public RoleOfUserBiz(ILogger<RoleOfUserBiz> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;
        }

        #region Cached RolesByUserGuid

        public Task<IEnumerable<Role>> GetRolesByUserGuidAsync(string userGuid, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedRolesByUserGuid.Key(userGuid), dbReader =>
            {
                var from = dbReader.From<Role>().RightJoin<RoleOfUser>((r, ru) => r.Guid == ru.RoleGuid);
                var where = dbReader.Where<Role>().And<RoleOfUser>(ru => ru.UserGuid == userGuid);

                return dbReader.RetrieveAsync(from, where, transContext);
            })!;
        }

        public async Task AddRolesToUserAsync(string userGuid, string roleGuid, string lastUser, TransactionContext transContext)
        {
            //查重
            long count = await _databaseReader.CountAsync<RoleOfUser>(ru => ru.UserGuid == userGuid && ru.RoleGuid == roleGuid, transContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw new FrameworkException(ErrorCode.DatabaseFoundTooMuch, $"已经有相同的角色. UserGuid:{userGuid}, RoleGuid:{roleGuid}");
            }

            RoleOfUser ru = new RoleOfUser { UserGuid = userGuid, RoleGuid = roleGuid };

            await UpdateAsync(ru, lastUser, transContext).ConfigureAwait(false);


            //Invalidate Cache
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            InvalidateCache(CachedRolesByUserGuid.Key(userGuid).Timestamp(now));
        }

        public async Task DeleteRolesFromUserAsync(string userGuid, string roleGuid, string lastUser, TransactionContext transactionContext)
        {
            //查重

            RoleOfUser? stored = await _databaseReader.ScalarAsync<RoleOfUser>(ru => ru.UserGuid == userGuid && ru.RoleGuid == roleGuid, transactionContext).ConfigureAwait(false);

            if (stored == null)
            {
                throw new FrameworkException(ErrorCode.DatabaseNotFound, $"没有找到这样的角色. UserGuid:{userGuid}, RoleGuid:{roleGuid}");
            }

            await DeleteAsync(stored, lastUser, transactionContext).ConfigureAwait(false);

            //Invalidate Cache
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            InvalidateCache(CachedRolesByUserGuid.Key(userGuid).Timestamp(now));
        }

        #endregion
    }
}

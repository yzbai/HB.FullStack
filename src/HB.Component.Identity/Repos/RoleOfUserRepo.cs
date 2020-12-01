using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Identity
{
    public class RoleOfUserRepo : Repository<RoleOfUser>
    {
        private readonly IDatabaseReader _databaseReader;
        public RoleOfUserRepo(ILogger<RoleOfUserRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;

            EntityUpdated += (roleOfUser, args) =>
            {
                InvalidateCache(CachedRolesByUserGuid.Key(roleOfUser.UserGuid).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };

            EntityDeleted += (roleOfUser, args) =>
            {
                InvalidateCache(CachedRolesByUserGuid.Key(roleOfUser.UserGuid).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };
        }

        #region Read

        public Task<RoleOfUser?> GetByConditionAsync(string userGuid, string roleGuid, TransactionContext? transactionContext = null)
        {
            return _databaseReader.ScalarAsync<RoleOfUser>(ru => ru.UserGuid == userGuid && ru.RoleGuid == roleGuid, transactionContext);
        }

        public Task<IEnumerable<Role>> GetRolesByUserGuidAsync(string userGuid, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedRolesByUserGuid.Key(userGuid), dbReader =>
            {
                var from = dbReader.From<Role>().RightJoin<RoleOfUser>((r, ru) => r.Guid == ru.RoleGuid);
                var where = dbReader.Where<Role>().And<RoleOfUser>(ru => ru.UserGuid == userGuid);

                return dbReader.RetrieveAsync(from, where, transContext);
            })!;
        }

        public Task<long> CountByConditionAsync(string userGuid, string roleGuid, TransactionContext? transContext = null)
        {
            return _databaseReader.CountAsync<RoleOfUser>(ru => ru.UserGuid == userGuid && ru.RoleGuid == roleGuid, transContext);
        }

        #endregion

        public async Task AddRolesToUserAsync(string userGuid, string roleGuid, string lastUser, TransactionContext transContext)
        {
            //查重
            long count = await CountByConditionAsync(userGuid, roleGuid, transContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw new FrameworkException(ErrorCode.DatabaseFoundTooMuch, $"已经有相同的角色. UserGuid:{userGuid}, RoleGuid:{roleGuid}");
            }

            RoleOfUser ru = new RoleOfUser { UserGuid = userGuid, RoleGuid = roleGuid };

            await UpdateAsync(ru, lastUser, transContext).ConfigureAwait(false);
        }



        public async Task DeleteRolesFromUserAsync(string userGuid, string roleGuid, string lastUser, TransactionContext transactionContext)
        {
            //查重
            RoleOfUser? stored = await GetByConditionAsync(userGuid, roleGuid, transactionContext).ConfigureAwait(false);

            if (stored == null)
            {
                throw new FrameworkException(ErrorCode.DatabaseNotFound, $"没有找到这样的角色. UserGuid:{userGuid}, RoleGuid:{roleGuid}");
            }

            await DeleteAsync(stored, lastUser, transactionContext).ConfigureAwait(false);
        }


    }
}

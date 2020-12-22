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
                InvalidateCache(CachedRolesByUserId.Key(roleOfUser.UserId).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };

            EntityDeleted += (roleOfUser, args) =>
            {
                InvalidateCache(CachedRolesByUserId.Key(roleOfUser.UserId).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };
        }

        #region Read

        public Task<RoleOfUser?> GetByConditionAsync(long userId, long roleId, TransactionContext? transactionContext = null)
        {
            return _databaseReader.ScalarAsync<RoleOfUser>(ru => ru.UserId == userId && ru.RoleId == roleId, transactionContext);
        }

        public Task<IEnumerable<Role>> GetRolesByUserIdAsync(long userId, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedRolesByUserId.Key(userId), dbReader =>
            {
                var from = dbReader.From<Role>().RightJoin<RoleOfUser>((r, ru) => r.Id == ru.RoleId);
                var where = dbReader.Where<Role>().And<RoleOfUser>(ru => ru.UserId == userId);

                return dbReader.RetrieveAsync(from, where, transContext);
            })!;
        }

        public Task<long> CountByConditionAsync(long userId, long roleId, TransactionContext? transContext = null)
        {
            return _databaseReader.CountAsync<RoleOfUser>(ru => ru.UserId == userId && ru.RoleId == roleId, transContext);
        }

        #endregion

        public async Task AddRolesToUserAsync(long userId, long roleId, string lastUser, TransactionContext transContext)
        {
            //查重
            long count = await CountByConditionAsync(userId, roleId, transContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw new FrameworkException(ErrorCode.DatabaseFoundTooMuch, $"已经有相同的角色. UserId:{userId}, RoleId:{roleId}");
            }

            RoleOfUser ru = new RoleOfUser { UserId = userId, RoleId = roleId };

            await UpdateAsync(ru, lastUser, transContext).ConfigureAwait(false);
        }

        public async Task DeleteRolesFromUserAsync(long userId, long roleId, string lastUser, TransactionContext transactionContext)
        {
            //查重
            RoleOfUser? stored = await GetByConditionAsync(userId, roleId, transactionContext).ConfigureAwait(false);

            if (stored == null)
            {
                throw new FrameworkException(ErrorCode.DatabaseNotFound, $"没有找到这样的角色. UserId:{userId}, RoleId:{roleId}");
            }

            await DeleteAsync(stored, lastUser, transactionContext).ConfigureAwait(false);
        }


    }
}

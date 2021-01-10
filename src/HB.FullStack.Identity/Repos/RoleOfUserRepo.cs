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
    public class RoleOfUserRepo : DatabaseRepository<RoleOfUser>
    {
        private readonly IDatabaseReader _databaseReader;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="databaseReader"></param>
        /// <param name="cache"></param>
        /// <param name="memoryLockManager"></param>
        /// <exception cref="CacheException"></exception>
        public RoleOfUserRepo(ILogger<RoleOfUserRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;

            EntityAdded += (roleOfUser, args) =>
            {
                InvalidateCache(CachedRolesByUserId.Key(roleOfUser.UserId).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };

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

        public Task<RoleOfUser?> GetByUserIdAndRoleIdAsync(long userId, long roleId, TransactionContext? transactionContext = null)
        {
            return _databaseReader.ScalarAsync<RoleOfUser>(ru => ru.UserId == userId && ru.RoleId == roleId, transactionContext);
        }

        /// <summary>
        /// GetRolesByUserIdAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="CacheException"></exception>
        public Task<IEnumerable<Role>> GetRolesByUserIdAsync(long userId, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedRolesByUserId.Key(userId), dbReader =>
            {
                var from = dbReader.From<Role>().RightJoin<RoleOfUser>((r, ru) => r.Id == ru.RoleId);
                var where = dbReader.Where<Role>().And<RoleOfUser>(ru => ru.UserId == userId);

                return dbReader.RetrieveAsync(from, where, transContext);
            })!;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<long> CountByUserIdAndRoleIdAsync(long userId, long roleId, TransactionContext? transContext = null)
        {
            return _databaseReader.CountAsync<RoleOfUser>(ru => ru.UserId == userId && ru.RoleId == roleId, transContext);
        }

        #endregion




    }
}

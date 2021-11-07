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
using HB.FullStack.Common;

namespace HB.FullStack.Identity
{
    internal class UserRoleRepo : DbEntityRepository<UserRole>
    {
        public UserRoleRepo(ILogger<UserRoleRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(UserRole sender, DatabaseWriteEventArgs args)
        {
            Parallel.Invoke(
                            () => InvalidateCache(CachedUserRolesByUserId.Key(sender.UserId).Timestamp(args.UtcNowTicks))
                        );
            return Task.CompletedTask;
        }

        #region Read

        public Task<UserRole?> GetByUserIdAndRoleIdAsync(Guid userId, Guid roleId, TransactionContext? transactionContext = null)
        {
            return DatabaseReader.ScalarAsync<UserRole>(ru => ru.UserId == userId && ru.RoleId == roleId, transactionContext);
        }

        /// <summary>
        /// GetRolesByUserIdAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="CacheException"></exception>
        public Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedUserRolesByUserId.Key(userId), dbReader =>
            {
                var from = dbReader.From<Role>().RightJoin<UserRole>((r, ru) => r.Id == ru.RoleId);
                var where = dbReader.Where<Role>().And<UserRole>(ru => ru.UserId == userId);

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
        public Task<long> CountByUserIdAndRoleIdAsync(Guid userId, Guid roleId, TransactionContext? transContext = null)
        {
            return DatabaseReader.CountAsync<UserRole>(ru => ru.UserId == userId && ru.RoleId == roleId, transContext);
        }

        #endregion




    }
}

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
    internal class UserRoleRepo : DbEntityRepository<UserRole>
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
        public UserRoleRepo(ILogger<UserRoleRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;

            RegisterEntityChangedEvents(OnEntityChanged);
        }

        private Task OnEntityChanged(UserRole sender, DatabaseWriteEventArgs args)
        {
            InvalidateCache(CachedRolesByUserId.Key(sender.UserId).Timestamp(args.UtcNowTicks));
            return Task.CompletedTask;
        }

        #region Read

        public Task<UserRole?> GetByUserIdAndRoleIdAsync(Guid userId, Guid roleId, TransactionContext? transactionContext = null)
        {
            return _databaseReader.ScalarAsync<UserRole>(ru => ru.UserId == userId && ru.RoleId == roleId, transactionContext);
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
            return TryCacheAsideAsync(CachedRolesByUserId.Key(userId), dbReader =>
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
            return _databaseReader.CountAsync<UserRole>(ru => ru.UserId == userId && ru.RoleId == roleId, transContext);
        }

        #endregion




    }
}

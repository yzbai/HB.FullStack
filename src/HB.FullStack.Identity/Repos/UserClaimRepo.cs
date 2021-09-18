using HB.FullStack.Database;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.FullStack.Identity.Entities;
using System;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Lock.Memory;

namespace HB.FullStack.Identity
{
    internal class UserClaimRepo : DatabaseRepository<UserClaim>
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="databaseReader"></param>
        /// <param name="cache"></param>
        /// <param name="memoryLockManager"></param>
        /// <exception cref="CacheException"></exception>
        public UserClaimRepo(ILogger<UserClaimRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            EntityAdded += (entity, args) =>
            {
                InvalidateCache(CachedUserClaimsByUserId.Key(entity.UserId).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };

            EntityUpdated += (entity, args) =>
            {
                InvalidateCache(CachedUserClaimsByUserId.Key(entity.UserId).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };

            EntityDeleted += (entity, args) =>
            {
                InvalidateCache(CachedUserClaimsByUserId.Key(entity.UserId).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// GetByUserIdAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public Task<IEnumerable<UserClaim>> GetByUserIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedUserClaimsByUserId.Key(userId), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaim>(uc => uc.UserId == userId, transContext);
            })!;
        }
    }
}

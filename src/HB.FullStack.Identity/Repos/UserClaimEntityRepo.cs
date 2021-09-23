using HB.FullStack.Database;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.FullStack.Identity.Entities;
using System;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Common;

namespace HB.FullStack.Identity
{
    internal class UserClaimEntityRepo : DbEntityRepository<UserClaimEntity>
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="databaseReader"></param>
        /// <param name="cache"></param>
        /// <param name="memoryLockManager"></param>
        /// <exception cref="CacheException"></exception>
        public UserClaimEntityRepo(ILogger<UserClaimEntityRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            RegisterEntityChangedEvents(OnEntityChanged);
        }

        private Task OnEntityChanged(UserClaimEntity sender, DatabaseWriteEventArgs args)
        {
            InvalidateCache(CachedUserClaimsByUserId.Key(sender.UserId).Timestamp(args.UtcNowTicks));
            return Task.CompletedTask;
        }

        /// <summary>
        /// GetByUserIdAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public Task<IEnumerable<UserClaimEntity>> GetByUserIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedUserClaimsByUserId.Key(userId), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaimEntity>(uc => uc.UserId == userId, transContext);
            })!;
        }
    }
}

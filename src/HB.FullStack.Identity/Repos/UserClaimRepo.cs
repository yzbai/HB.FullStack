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
    internal class UserClaimRepo : DbEntityRepository<UserClaim>
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="databaseReader"></param>
        /// <param name="cache"></param>
        /// <param name="memoryLockManager"></param>
        
        public UserClaimRepo(ILogger<UserClaimRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(UserClaim sender, DatabaseWriteEventArgs args)
        {
            Parallel.Invoke(
                () => InvalidateCache(new CachedUserClaimsByUserId(sender.UserId).Timestamp(args.UtcNowTicks))
            );

            return Task.CompletedTask;
        }

        /// <summary>
        /// GetByUserIdAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        
        
        public Task<IEnumerable<UserClaim>> GetByUserIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return CacheAsideAsync(new CachedUserClaimsByUserId(userId), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaim>(uc => uc.UserId == userId, transContext);
            })!;
        }
    }
}

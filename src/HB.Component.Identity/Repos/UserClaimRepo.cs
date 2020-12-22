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
    internal class UserClaimRepo : Repository<UserClaim>
    {
        public UserClaimRepo(ILogger<UserClaimRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
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

        public Task<IEnumerable<UserClaim>> GetByUserIdAsync(long userId, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedUserClaimsByUserId.Key(userId), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaim>(uc => uc.UserId == userId, transContext);
            })!;
        }

        public Task AddToUserAsync(long userId, UserClaim userClaim, TransactionContext? transContext = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromUserAsync(long userId, UserClaim userClaim, TransactionContext? transContext = null)
        {
            throw new NotImplementedException();
        }
    }
}

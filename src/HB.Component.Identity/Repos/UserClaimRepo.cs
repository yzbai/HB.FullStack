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
        public UserClaimRepo(ILogger logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            EntityUpdated += (entity, args) =>
            {
                InvalidateCache(CachedUserClaimsByUserGuid.Key(entity.UserGuid).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };

            EntityDeleted += (entity, args) =>
            {
                InvalidateCache(CachedUserClaimsByUserGuid.Key(entity.UserGuid).Timestamp(args.UtcNowTicks));
                return Task.CompletedTask;
            };
        }

        #region Cached UserClaimsByUserGuid

        public Task<IEnumerable<UserClaim>> GetByUserGuidAsync(string userGuid, TransactionContext? transContext = null)
        {
            return TryCacheAsideAsync(CachedUserClaimsByUserGuid.Key(userGuid), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaim>(uc => uc.UserGuid == userGuid, transContext);
            })!;
        }

        public Task AddToUserAsync(string userGuid, UserClaim userClaim, TransactionContext? transContext = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFromUserAsync(string userGuid, UserClaim userClaim, TransactionContext? transContext = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

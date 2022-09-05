using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Common.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Identity
{
    public class UserClaimRepo : ModelRepository<UserClaim>
    {


        public UserClaimRepo(ILogger<UserClaimRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(IEnumerable<DbModel> sender, DBChangedEventArgs args)
        {
            if (sender is IEnumerable<UserClaim> userClaims)
            {
                //大部分都是一个，用不着
                //Parallel.ForEach(userClaims, (userClaim) => InvalidateCache(new CachedUserClaimsByUserId(userClaim.UserId)));

                foreach (var userClaim in userClaims)
                {
                    InvalidateCache(new CachedUserClaimsByUserId(userClaim.UserId));
                }
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<UserClaim>> GetByUserIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return GetUsingCacheAsideAsync(new CachedUserClaimsByUserId(userId), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaim>(uc => uc.UserId == userId, transContext);
            })!;
        }
    }
}
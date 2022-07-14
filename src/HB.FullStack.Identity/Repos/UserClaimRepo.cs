using HB.FullStack.Database;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.FullStack.Identity.Models;
using System;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Common;
using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.Identity
{
    public class UserClaimRepo : ModelRepository<UserClaim>
    {


        public UserClaimRepo(ILogger<UserClaimRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(IEnumerable<DBModel> sender, DBChangedEventArgs args)
        {
            if (sender is IEnumerable<UserClaim> userClaims)
            {
                //大部分都是一个，用不着
                //Parallel.ForEach(userClaims, (userClaim) => InvalidateCache(new CachedUserClaimsByUserId(userClaim.UserId)));

                foreach(var userClaim in userClaims)
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
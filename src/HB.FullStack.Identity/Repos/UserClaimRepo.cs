using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Identity
{
    public class UserClaimRepo : ModelRepository<UserClaim>
    {

        public UserClaimRepo(ILogger<UserClaimRepo> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(object sender, DBChangeEventArgs args)
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
            else if (sender is IEnumerable<PropertyChangePack> cpps)
            {
                foreach (var cpp in cpps)
                {
                    if (cpp.AddtionalProperties.TryGetValue(nameof(UserClaim.UserId), out JsonElement element))
                    {
                        Guid userId = SerializeUtil.To<Guid>(element)!;
                        InvalidateCache(new CachedUserClaimsByUserId(userId));
                    }
                    else
                    {
                        throw CommonExceptions.AddtionalPropertyNeeded(model: nameof(UserClaim), property: nameof(UserClaim.UserId));
                    }
                }
            }
            else
            {
                throw CommonExceptions.UnkownEventSender(sender);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<UserClaim>> GetByUserIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return GetUsingCacheAsideAsync(new CachedUserClaimsByUserId(userId), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaim>(uc => uc.UserId == userId, transContext);
            });
        }
    }
}
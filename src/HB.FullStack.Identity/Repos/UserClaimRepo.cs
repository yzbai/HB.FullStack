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
    public class UserClaimRepo<TId> : DbModelRepository<UserClaim<TId>>
    {

        public UserClaimRepo(ILogger<UserClaimRepo<TId>> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        protected override Task InvalidateCacheItemsOnChanged(object sender, ModelChangeEventArgs args)
        {
            if (sender is IEnumerable<UserClaim<TId>> userClaims)
            {
                //大部分都是一个，用不着
                //Parallel.ForEach(userClaims, (userClaim) => InvalidateCache(new CachedUserClaimsByUserId(userClaim.UserId)));

                foreach (var userClaim in userClaims)
                {
                    InvalidateCache(new CachedUserClaimsByUserId<TId>(userClaim.UserId));
                }
            }
            else if (sender is IEnumerable<PropertyChangePack> cpps)
            {
                foreach (var cpp in cpps)
                {
                    if (cpp.AddtionalProperties.TryGetValue(nameof(UserClaim<TId>.UserId), out JsonElement element))
                    {
                        TId userId = SerializeUtil.To<TId>(element)!;
                        InvalidateCache(new CachedUserClaimsByUserId<TId>(userId));
                    }
                    else
                    {
                        throw CommonExceptions.AddtionalPropertyNeeded(model: nameof(UserClaim<TId>), property: nameof(UserClaim<TId>.UserId));
                    }
                }
            }
            else
            {
                throw CommonExceptions.UnkownEventSender(sender);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<UserClaim<TId>>> GetByUserIdAsync(TId userId, TransactionContext? transContext = null)
        {
            return GetUsingCacheAsideAsync(new CachedUserClaimsByUserId<TId>(userId), dbReader =>
            {
                return dbReader.RetrieveAsync<UserClaim<TId>>(uc => uc.UserId!.Equals(userId), transContext);
            });
        }
    }
}
using HB.FullStack.Identity.Entities;
using HB.FullStack.Database;
using HB.FullStack.Database.SQL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.KVStore;
using HB.FullStack.Repository;
using Microsoft.Extensions.Logging;
using HB.FullStack.Cache;
using HB.FullStack.Lock.Memory;

namespace HB.FullStack.Identity
{
    public class SignInTokenRepo : DbEntityRepository<SignInToken>
    {
        public SignInTokenRepo(ILogger<SignInTokenRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        #region Read

        public Task<IEnumerable<SignInToken>> GetByUserIdAsync(Guid userId, TransactionContext? transactionContext)
        {
            return DbReader.RetrieveAsync<SignInToken>(s => s.UserId == userId, transactionContext);
        }

        public Task<SignInToken?> GetByIdAsync(Guid signInTokenId, TransactionContext? transactionContext)
        {
            return DbReader.ScalarAsync<SignInToken>(signInTokenId, transactionContext);
        }

        protected override Task InvalidateCacheItemsOnChanged(SignInToken sender, DatabaseWriteEventArgs args) => Task.CompletedTask;

        //public Task<SignInToken?> GetByConditionAsync(Guid signInTokenId, string? refreshToken, string deviceId, Guid userId, TransactionContext? transContext = null)
        //{
        //    if (refreshToken.IsNullOrEmpty())
        //    {
        //        return Task.FromResult((SignInToken?)null);
        //    }

        //    return _databaseReader.ScalarAsync<SignInToken>(s =>
        //        s.RefreshToken == refreshToken &&
        //        s.UserId == userId &&
        //        s.Id == signInTokenId &&
        //        s.DeviceId == deviceId, transContext);
        //}

        #endregion
    }
}
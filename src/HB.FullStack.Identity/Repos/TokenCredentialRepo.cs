using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Identity
{
    public class TokenCredentialRepo<TId> : DbModelRepository<TokenCredential<TId>>
    {
        public TokenCredentialRepo(ILogger<TokenCredentialRepo<TId>> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        #region Read

        public Task<IList<TokenCredential<TId>>> GetByUserIdAsync(TId userId, TransactionContext? transactionContext)
        {
            return DbReader.RetrieveAsync<TokenCredential<TId>>(s => s.UserId!.Equals(userId), transactionContext);
        }

        public Task<TokenCredential<TId>?> GetByIdAsync(TId signInCredentialId, TransactionContext? transactionContext)
        {
            return DbReader.ScalarAsync<TokenCredential<TId>>(signInCredentialId!, transactionContext);
        }

        protected override Task InvalidateCacheItemsOnChanged(object sender, ModelChangeEventArgs args) => Task.CompletedTask;

        //public Task<Token?> GetByConditionAsync(Guid signInReceiptId, string? refreshToken, string deviceId, Guid userId, TransactionContext? transContext = null)
        //{
        //    if (refreshToken.IsNullOrEmpty())
        //    {
        //        return Task.FromResult((Token?)null);
        //    }

        //    return _databaseReader.ScalarAsync<Token>(s =>
        //        s.RefreshToken == refreshToken &&
        //        s.UserId == userId &&
        //        s.Id == signInReceiptId &&
        //        s.DeviceId == deviceId, transContext);
        //}

        #endregion
    }
}
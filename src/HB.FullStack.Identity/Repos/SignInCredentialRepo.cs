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
    public class SignInCredentialRepo : ModelRepository<SignInCredential>
    {
        public SignInCredentialRepo(ILogger<SignInCredentialRepo> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager) { }

        #region Read

        public Task<IEnumerable<SignInCredential>> GetByUserIdAsync(Guid userId, TransactionContext? transactionContext)
        {
            return DbReader.RetrieveAsync<SignInCredential>(s => s.UserId == userId, transactionContext);
        }

        public Task<SignInCredential?> GetByIdAsync(Guid signInCredentialId, TransactionContext? transactionContext)
        {
            return DbReader.ScalarAsync<SignInCredential>(signInCredentialId, transactionContext);
        }

        protected override Task InvalidateCacheItemsOnChanged(object sender, DBChangeEventArgs args) => Task.CompletedTask;

        //public Task<SignInReceipt?> GetByConditionAsync(Guid signInReceiptId, string? refreshToken, string deviceId, Guid userId, TransactionContext? transContext = null)
        //{
        //    if (refreshToken.IsNullOrEmpty())
        //    {
        //        return Task.FromResult((SignInReceipt?)null);
        //    }

        //    return _databaseReader.ScalarAsync<SignInReceipt>(s =>
        //        s.RefreshToken == refreshToken &&
        //        s.UserId == userId &&
        //        s.Id == signInReceiptId &&
        //        s.DeviceId == deviceId, transContext);
        //}

        #endregion
    }
}
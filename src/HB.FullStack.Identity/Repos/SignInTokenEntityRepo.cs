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
    internal class SignInTokenEntityRepo : DbEntityRepository<SignInTokenEntity>
    {
        private readonly IDatabaseReader _databaseReader;

        public SignInTokenEntityRepo(ILogger<SignInTokenEntityRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;
        }

        #region Read

        public Task<IEnumerable<SignInTokenEntity>> GetByUserIdAsync(Guid userId, TransactionContext? transactionContext)
        {
            return _databaseReader.RetrieveAsync<SignInTokenEntity>(s => s.UserId == userId, transactionContext);
        }

        public Task<SignInTokenEntity?> GetByIdAsync(Guid signInTokenId, TransactionContext? transactionContext)
        {
            return _databaseReader.ScalarAsync<SignInTokenEntity>(signInTokenId, transactionContext);
        }

        public Task<SignInTokenEntity?> GetByConditionAsync(Guid signInTokenId, string? refreshToken, string deviceId, Guid userId, TransactionContext? transContext = null)
        {
            if (refreshToken.IsNullOrEmpty())
            {
                return Task.FromResult((SignInTokenEntity?)null);
            }

            return _databaseReader.ScalarAsync<SignInTokenEntity>(s =>
                s.UserId == userId &&
                s.Id == signInTokenId &&
                s.RefreshToken == refreshToken &&
                s.DeviceId == deviceId, transContext);
        }

        #endregion
    }
}

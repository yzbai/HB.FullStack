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
    internal class SignInTokenRepo : Repository<SignInToken>
    {
        private readonly IDatabaseReader _databaseReader;

        public SignInTokenRepo(ILogger<SignInTokenRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;
        }

        #region Read

        public Task<IEnumerable<SignInToken>> GetByUserIdAsync(long userId, TransactionContext? transactionContext)
        {
            return _databaseReader.RetrieveAsync<SignInToken>(s => s.UserId == userId, transactionContext);
        }

        public Task<SignInToken?> GetByIdAsync(long signInTokenId, TransactionContext? transactionContext)
        {
            return _databaseReader.ScalarAsync<SignInToken>(signInTokenId, transactionContext);
        }

        public Task<SignInToken?> GetByConditionAsync(long signInTokenId, string? refreshToken, string deviceId, long userId, TransactionContext? transContext = null)
        {
            if (refreshToken.IsNullOrEmpty())
            {
                return Task.FromResult((SignInToken?)null);
            }

            return _databaseReader.ScalarAsync<SignInToken>(s =>
                s.UserId == userId &&
                s.Id == signInTokenId &&
                s.RefreshToken == refreshToken &&
                s.DeviceId == deviceId, transContext);
        }

        #endregion

        public async Task<SignInToken> CreateAsync(
            long userId,
            string deviceId,
            DeviceInfos deviceInfos,
            string deviceVersion,
            string ipAddress,
            TimeSpan expireTimeSpan,
            string lastUser,
            TransactionContext? transactionContext)
        {
            SignInToken token = new SignInToken
            {
                UserId = userId,
                RefreshToken = SecurityUtil.CreateUniqueToken(),
                ExpireAt = TimeUtil.UtcNow + expireTimeSpan,
                DeviceId = deviceId,
                DeviceVersion = deviceVersion,
                DeviceIp = ipAddress,

                DeviceName = deviceInfos.Name,
                DeviceModel = deviceInfos.Model,
                DeviceOSVersion = deviceInfos.OSVersion,
                DevicePlatform = deviceInfos.Platform,
                DeviceIdiom = deviceInfos.Idiom,
                DeviceType = deviceInfos.Type
            };

            await AddAsync(token, lastUser, transactionContext).ConfigureAwait(false);

            return token;
        }

        public async Task DeleteByLogOffTypeAsync(long userId, DeviceIdiom currentIdiom, LogOffType logOffType, string lastUser, TransactionContext transactionContext)
        {
            IEnumerable<SignInToken> resultList = await GetByUserIdAsync(userId, transactionContext).ConfigureAwait(false);

            IEnumerable<SignInToken> toDeletes = logOffType switch
            {
                LogOffType.LogOffAllOthers => resultList,
                LogOffType.LogOffAllButWeb => resultList.Where(s => s.DeviceIdiom != DeviceIdiom.Web),
                LogOffType.LogOffSameIdiom => resultList.Where(s => s.DeviceIdiom == currentIdiom),
                _ => new List<SignInToken>()
            };

            await BatchDeleteAsync(toDeletes, lastUser, transactionContext).ConfigureAwait(false);
        }

        public async Task DeleteByUserIdAsync(long userId, string lastUser, TransactionContext transContext)
        {
            ThrowIf.Null(transContext, nameof(transContext));

            IEnumerable<SignInToken> resultList = await GetByUserIdAsync(userId, transContext).ConfigureAwait(false);

            await BatchDeleteAsync(resultList, lastUser, transContext).ConfigureAwait(false);
        }

        public async Task DeleteByIdAsync(long signInTokenId, string lastUser, TransactionContext transContext)
        {
            ThrowIf.Null(transContext, nameof(transContext));

            SignInToken? signInToken = await GetByIdAsync(signInTokenId, transContext).ConfigureAwait(false);

            if (signInToken != null)
            {
                await DeleteAsync(signInToken, lastUser, transContext).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning($"尝试删除不存在的SignInToken. SignInTokenId:{signInTokenId}");
            }
        }

        public new Task UpdateAsync(SignInToken signInToken, string lastUser, TransactionContext? transContext = null)
        {
            return base.UpdateAsync(signInToken, lastUser, transContext);
        }
    }
}

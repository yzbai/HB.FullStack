using HB.FullStack.Identity.Entities;
using HB.FullStack.Database;
using HB.FullStack.Database.SQL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.KVStore;
using HB.FullStack.Business;
using Microsoft.Extensions.Logging;
using HB.FullStack.Cache;
using HB.FullStack.Lock.Memory;

namespace HB.FullStack.Identity
{
    internal class SignInTokenBiz : BaseEntityBiz<SignInToken>
    {
        private readonly IDatabaseReader _databaseReader;

        public SignInTokenBiz(ILogger<SignInTokenBiz> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager) : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;
        }

        #region Read

        public Task<IEnumerable<SignInToken>> GetByUserGuidAsync(string userGuid, TransactionContext? transactionContext)
        {
            return _databaseReader.RetrieveAsync<SignInToken>(s => s.UserGuid == userGuid, transactionContext);
        }

        public Task<SignInToken?> GetByGuidAsync(string signInTokenGuid, TransactionContext? transactionContext)
        {
            return _databaseReader.ScalarAsync<SignInToken>(s => s.Guid == signInTokenGuid, transactionContext);
        }

        public Task<SignInToken?> GetByConditionAsync(string? signInTokenGuid, string? refreshToken, string deviceId, string? userGuid, TransactionContext? transContext = null)
        {
            if (signInTokenGuid.IsNullOrEmpty() || refreshToken.IsNullOrEmpty() || userGuid.IsNullOrEmpty())
            {
                return Task.FromResult((SignInToken?)null);
            }

            return _databaseReader.ScalarAsync<SignInToken>(s =>
                s.UserGuid == userGuid &&
                s.Guid == signInTokenGuid &&
                s.RefreshToken == refreshToken &&
                s.DeviceId == deviceId, transContext);
        }

        #endregion

        public async Task<SignInToken> CreateAsync(
            string userGuid,
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
                UserGuid = userGuid,
                RefreshToken = SecurityUtil.CreateUniqueToken(),
                ExpireAt = DateTimeOffset.UtcNow + expireTimeSpan,
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

        public async Task DeleteByLogOffTypeAsync(string userGuid, DeviceIdiom currentIdiom, LogOffType logOffType, string lastUser, TransactionContext transactionContext)
        {
            ThrowIf.Empty(userGuid, nameof(userGuid));

            IEnumerable<SignInToken> resultList = await GetByUserGuidAsync(userGuid, transactionContext).ConfigureAwait(false);

            IEnumerable<SignInToken> toDeletes = logOffType switch
            {
                LogOffType.LogOffAllOthers => resultList,
                LogOffType.LogOffAllButWeb => resultList.Where(s => s.DeviceIdiom != DeviceIdiom.Web),
                LogOffType.LogOffSameIdiom => resultList.Where(s => s.DeviceIdiom == currentIdiom),
                _ => new List<SignInToken>()
            };

            await BatchDeleteAsync(toDeletes, lastUser, transactionContext).ConfigureAwait(false);
        }

        public async Task DeleteByUserGuidAsync(string userGuid, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IEnumerable<SignInToken> resultList = await GetByUserGuidAsync(userGuid, transContext).ConfigureAwait(false);

            await BatchDeleteAsync(resultList, lastUser, transContext).ConfigureAwait(false);
        }

        public async Task DeleteByGuidAsync(string signInTokenGuid, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(signInTokenGuid, nameof(signInTokenGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            SignInToken? signInToken = await GetByGuidAsync(signInTokenGuid, transContext).ConfigureAwait(false);

            if (signInToken != null)
            {
                await DeleteAsync(signInToken, lastUser, transContext).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning($"尝试删除不存在的SignInToken. SignInTokenGuid:{signInTokenGuid}");
            }
        }

        public new Task UpdateAsync(SignInToken signInToken, string lastUser, TransactionContext? transContext = null)
        {
            return base.UpdateAsync(signInToken, lastUser, transContext);
        }
    }
}

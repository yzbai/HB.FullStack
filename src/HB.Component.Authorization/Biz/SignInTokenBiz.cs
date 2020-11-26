using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entities;
using HB.FullStack.Database;
using HB.FullStack.Database.SQL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using HB.FullStack.KVStore;

namespace HB.Component.Authorization
{
    internal class SignInTokenBiz : ISignInTokenBiz
    {
        private readonly IKVStore _kv;

        public SignInTokenBiz(IKVStore kv)
        {
            _kv = kv;
        }

        /// <summary>
        /// CreateAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="deviceId"></param>
        /// <param name="deviceInfos"></param>
        /// <param name="deviceVersion"></param>
        /// <param name="deviceAddress"></param>
        /// <param name="ipAddress"></param>
        /// <param name="expireTimeSpan"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        public async Task<SignInToken> CreateAsync(
            string userGuid,
            string deviceId,
            DeviceInfos deviceInfos,
            string deviceVersion,
            string ipAddress,
            TimeSpan expireTimeSpan,
            string lastUser)
        {
            SignInToken token = new SignInToken
            {
                UserGuid = userGuid,
                RefreshToken = SecurityUtil.CreateUniqueToken(),
                RefreshCount = 0,
                Blacked = false,
                DeviceId = deviceId,
                DeviceInfos = deviceInfos,
                DeviceVersion = deviceVersion,
                DeviceIp = ipAddress,
                ExpireAt = DateTimeOffset.UtcNow + expireTimeSpan
            };

            await _kv.AddAsync(token).ConfigureAwait(false);

            return token;
        }

        public async Task DeleteByLogOffTypeAsync(string userGuid, DeviceIdiom currentIdiom, LogOffType logOffType, string lastUser, TransactionContext transContext)
        {
            ThrowIf.Empty(userGuid, nameof(userGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IEnumerable<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(s => s.UserGuid == userGuid, transContext).ConfigureAwait(false);

            IEnumerable<SignInToken> toDeletes = logOffType switch
            {
                LogOffType.LogOffAllOthers => resultList,
                LogOffType.LogOffAllButWeb => resultList.Where(s => s.DeviceInfos.Idiom != DeviceIdiom.Web),
                LogOffType.LogOffSameIdiom => resultList.Where(s => s.DeviceInfos.Idiom == currentIdiom),
                _ => new List<SignInToken>()
            };

            await _db.BatchDeleteAsync(toDeletes, lastUser, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteByUserGuidAsync
        /// </summary>
        /// <param name="userGuid"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        public async Task DeleteByUserGuidAsync(string userGuid, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(userGuid, nameof(userGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IEnumerable<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.UserGuid == userGuid, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, lastUser, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <param name="signInTokenGuid"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string signInTokenGuid, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NullOrEmpty(signInTokenGuid, nameof(signInTokenGuid));
            ThrowIf.Null(transContext, nameof(transContext));

            IEnumerable<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.Guid == signInTokenGuid, transContext).ConfigureAwait(false);

            await _db.BatchDeleteAsync(resultList, lastUser, transContext).ConfigureAwait(false);
        }

        public async Task<SignInToken?> GetAsync(string? signInTokenGuid, string? refreshToken, string deviceId, string? userGuid, TransactionContext? transContext = null)
        {
            if (signInTokenGuid.IsNullOrEmpty() || refreshToken.IsNullOrEmpty() || userGuid.IsNullOrEmpty())
            {
                return null;
            }

            return await _db.ScalarAsync<SignInToken>(s =>
                s.UserGuid == userGuid &&
                s.Guid == signInTokenGuid &&
                s.RefreshToken == refreshToken &&
                s.DeviceId == deviceId, transContext).ConfigureAwait(false);
        }

        /// <summary>
        /// UpdateAsync
        /// </summary>
        /// <param name="signInToken"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        public Task UpdateAsync(SignInToken signInToken, string lastUser, TransactionContext? transContext = null)
        {
            return _db.UpdateAsync(signInToken, lastUser, transContext);
        }
    }
}

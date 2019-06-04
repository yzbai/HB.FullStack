using HB.Framework.Common;
using HB.Framework.Database;
using HB.Framework.KVStore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using HB.Component.Identity.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Framework.Database.SQL;
using HB.Component.Authorization.Entity;
using HB.Component.Authorization.Abstractions;
using HB.Framework.Database.Transaction;

namespace HB.Component.Authorization
{
    internal class SignInTokenBiz : ISignInTokenBiz
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;

        public SignInTokenBiz(IDatabase database, ILogger<SignInTokenBiz> logger)
        {
            _db = database;
            _logger = logger;
        }

        public async Task<SignInToken> CreateAsync(string userGuid, string clientId, string clientType, string clientVersion, string clientAddress, string ipAddress, TimeSpan expireTimeSpan, TransactionContext transContext = null)
        {
            SignInToken token = new SignInToken {
                Guid = SecurityUtil.CreateUniqueToken(),
                UserGuid = userGuid,
                RefreshToken = SecurityUtil.CreateUniqueToken(),
                RefreshCount = 0,
                Blacked = false,
                ClientId = clientId,
                ClientType = clientType,
                ClientVersion = clientVersion,
                ClientAddress = clientAddress,
                ClientIp = ipAddress,
                ExpireAt = DateTimeOffset.UtcNow + expireTimeSpan
            };

            AuthorizationResult err = (await _db.AddAsync(token, transContext).ConfigureAwait(false)).ToAuthorizationResult();

            if (!err.IsSucceeded())
            {
                _logger.LogCritical(0, "AppAuthentication created err.", null);
                return null;
            }

            return token;
        }

        public async Task<AuthorizationResult> DeleteAppClientTokenByUserGuidAsync(string userGuid, TransactionContext transContext)
        {
            if (userGuid.IsNullOrEmpty())
            {
                return AuthorizationResult.ArgumentError();
            }

            transContext.RequireNotNull();

            WhereExpression<SignInToken> where = _db.NewWhere<SignInToken>()
                .Where(at => at.ClientType != Enum.GetName(typeof(ClientType), ClientType.Web))
                .And(at => at.UserGuid == userGuid);

            IList<SignInToken> resultList = await _db.RetrieveAsync(where, transContext).ConfigureAwait(false);

            if (resultList.Count == 0)
            {
                return AuthorizationResult.Succeeded();
            }

            DatabaseResult dbResult = await _db.BatchDeleteAsync(resultList, "default", transContext).ConfigureAwait(false);

            if (!dbResult.IsSucceeded())
            {
                _logger.LogCritical(0, $"DeleteAppClientTokenByUserIdAsync delete failed, userId : {userGuid}", null);
                return dbResult.ToAuthorizationResult();
            }

            return dbResult.ToAuthorizationResult();
        }

        public async Task<AuthorizationResult> DeleteByUserGuidAsync(string userGuid, TransactionContext transContext)
        {
            if (userGuid.IsNullOrEmpty())
            {
                return AuthorizationResult.ArgumentError();
            }

            transContext.RequireNotNull();

            IList<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.UserGuid == userGuid, transContext).ConfigureAwait(false);

            DatabaseResult dbResult = await _db.BatchDeleteAsync(resultList, "default", transContext).ConfigureAwait(false);

            if (!dbResult.IsSucceeded())
            {
                _logger.LogCritical(0, $"DeleteByUserGuidAsync delete failed, userId : {userGuid}", null);
                return dbResult.ToAuthorizationResult();
            }

            return dbResult.ToAuthorizationResult();
        }

        public async Task<AuthorizationResult> DeleteAsync(string signInTokenGuid, TransactionContext transContext)
        {
            if (signInTokenGuid.IsNullOrEmpty())
            {
                return AuthorizationResult.ArgumentError();
            }

            transContext.RequireNotNull();

            IList<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.Guid == signInTokenGuid, transContext).ConfigureAwait(false);

            DatabaseResult dbResult = await _db.BatchDeleteAsync(resultList, "default", transContext).ConfigureAwait(false);

            if (!dbResult.IsSucceeded())
            {
                _logger.LogCritical(0, $"DeleteAsync delete failed, signInTokenGuid : {signInTokenGuid}", null);
                return dbResult.ToAuthorizationResult();
            }

            return dbResult.ToAuthorizationResult();
        }

        public async Task<SignInToken> GetAsync(string signInTokenGuid, string refreshToken, string clientId, string userGuid, TransactionContext transContext = null)
        {
            if (signInTokenGuid.IsNullOrEmpty() || refreshToken.IsNullOrEmpty() || userGuid.IsNullOrEmpty())
            {
                return null;
            }

            return await _db.ScalarAsync<SignInToken>(s =>
                s.UserGuid == userGuid &&
                s.Guid == signInTokenGuid &&
                s.RefreshToken == refreshToken &&
                s.ClientId == clientId, transContext).ConfigureAwait(false);
        }

        public async Task<AuthorizationResult> UpdateAsync(SignInToken signInToken, TransactionContext transContext = null)
        {
            if (signInToken == null)
            {
                return AuthorizationResult.ArgumentError();
            }

            return (await _db.UpdateAsync(signInToken, transContext).ConfigureAwait(false)).ToAuthorizationResult();
        }
    }
}

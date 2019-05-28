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
    public class SignInTokenBiz : ISignInTokenBiz
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
            SignInToken token = new SignInToken
            {
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

            AuthorizationServerResult err = (await _db.AddAsync(token, transContext).ConfigureAwait(false)).ToAuthorizationResult();

            if (!err.IsSucceeded())
            {
                _logger.LogCritical(0, "AppAuthentication created err.", null);
                return null;
            }

            return token;
        }

        public async Task<AuthorizationServerResult> DeleteAppClientTokenByUserGuidAsync(string userGuid, TransactionContext transContext = null)
        {
            //TODO: Test this where expression
            WhereExpression<SignInToken> where = _db.NewWhere<SignInToken>()
                .Where(at=>at.ClientType != Enum.GetName(typeof(ClientType), ClientType.Web))
                .And(at=>at.UserGuid == userGuid);

            IList<SignInToken> resultList = await _db.RetrieveAsync(where, transContext).ConfigureAwait(false);

            foreach (SignInToken at in resultList)
            {
                AuthorizationServerResult err = (await _db.DeleteAsync(at, transContext).ConfigureAwait(false)).ToAuthorizationResult();

                if (!err.IsSucceeded())
                {
                    _logger.LogCritical(0, "DeleteAppClientTokenByUserIdAsync delete failed, userId : " + userGuid, null);

                    return err;
                }
            }

            return AuthorizationServerResult.Succeeded();
        }

        public async Task<AuthorizationServerResult> DeleteByUserGuidAsync(string userGuid, TransactionContext transContext = null)
        {
            IList<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.UserGuid == userGuid, transContext).ConfigureAwait(false);

            foreach (SignInToken at in resultList)
            {
                AuthorizationServerResult err = (await _db.DeleteAsync(at, transContext).ConfigureAwait(false)).ToAuthorizationResult();

                if (!err.IsSucceeded())
                {
                    _logger.LogCritical(0, "AuthenticationToken delete failed, userId : " + userGuid, null);

                    return err;
                }
            }

            return AuthorizationServerResult.Succeeded();
        }

        public async Task<AuthorizationServerResult> DeleteAsync(string signInTokenGuid, TransactionContext transContext = null)
        {
            IList<SignInToken> resultList = await _db.RetrieveAsync<SignInToken>(at => at.Guid == signInTokenGuid, transContext).ConfigureAwait(false);

            foreach (SignInToken at in resultList)
            {
                AuthorizationServerResult err = (await _db.DeleteAsync(at, transContext).ConfigureAwait(false)).ToAuthorizationResult();

                if (!err.IsSucceeded())
                {
                    _logger.LogCritical(0, "AuthenticationToken delete failed, userTokenIdentifier : " + signInTokenGuid, null);

                    return err;
                }
            }

            return AuthorizationServerResult.Succeeded();
        }

        public Task<SignInToken> GetAsync(string signInTokenGuid, string refreshToken, string clientId, string userGuid, TransactionContext transContext = null)
        {
            return _db.ScalarAsync<SignInToken>(s => 
                s.UserGuid ==userGuid && 
                s.Guid == signInTokenGuid && 
                s.RefreshToken == refreshToken && 
                s.ClientId == clientId, transContext );
        }

        public async Task<AuthorizationServerResult> UpdateAsync(SignInToken signInToken, TransactionContext transContext = null)
        {
            return (await _db.UpdateAsync(signInToken, transContext).ConfigureAwait(false)).ToAuthorizationResult();
        }
    }
}

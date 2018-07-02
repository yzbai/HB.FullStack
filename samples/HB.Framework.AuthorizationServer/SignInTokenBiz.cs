using HB.Framework.Business;
using HB.Framework.Common;
using HB.Framework.Database;
using HB.Framework.KVStore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using HB.Framework.Identity.Abstractions;
using Microsoft.Extensions.Logging;
using HB.Framework.AuthorizationServer.Abstractions;
using System.Threading.Tasks;
using HB.Framework.Database.SQL;

namespace HB.Framework.AuthorizationServer
{
    public class SignInTokenBiz : BaseBiz, ISignInTokenBiz
    {
        private AuthorizationServerOptions _options;

        public SignInTokenBiz(IDatabase database, IKVStore kvstore, IDistributedCache cache, IOptions<AuthorizationServerOptions> options, ILogger<SignInTokenBiz> logger) 
            : base(database, kvstore, cache, logger)
        {
            _options = options.Value;
        }

        public async Task<SignInToken> CreateNewTokenAsync(long userId, string clientId, string clientType, string clientVersion, string clientAddress, string ipAddress, TimeSpan expireTimeSpan, DbTransactionContext transContext = null)
        {
            SignInToken token = new SignInToken
            {
                UserId = userId,
                SignInTokenIdentifier = SecurityHelper.CreateUniqueToken(),
                RefreshToken = SecurityHelper.CreateUniqueToken(),
                RefreshCount = 0,
                Blacked = false,
                ClientId = clientId,
                ClientType = clientType,
                ClientVersion = clientVersion,
                ClientAddress = clientAddress,
                ClientIp = ipAddress,
                ExpireAt = DateTimeOffset.UtcNow + expireTimeSpan
            };

            AuthorizationServerResult err = (await Database.AddAsync(token, transContext)).ToAuthorizationResult();

            if (err != AuthorizationServerResult.Succeeded)
            {
                Logger.LogCritical(0, "AppAuthentication created err.", null);
                return null;
            }

            return token;
        }

        public async Task<AuthorizationServerResult> DeleteAppClientTokenByUserIdAsync(long userId, DbTransactionContext transContext = null)
        {
            //TODO: Test this where expression
            Where<SignInToken> where = Database.Where<SignInToken>()
                .where(at=>at.ClientType != Enum.GetName(typeof(ClientType), ClientType.Web))
                .And(at=>at.UserId == userId);

            IList<SignInToken> resultList = await Database.RetrieveAsync<SignInToken>(where, transContext);

            foreach (SignInToken at in resultList)
            {
                AuthorizationServerResult err = (await Database.DeleteAsync(at, transContext)).ToAuthorizationResult();

                if (err != AuthorizationServerResult.Succeeded)
                {
                    Logger.LogCritical(0, "DeleteAppClientTokenByUserIdAsync delete failed, userId : " + userId, null);

                    return err;
                }
            }

            return AuthorizationServerResult.Succeeded;
        }

        public async Task<AuthorizationServerResult> DeleteByUserIdAsync(long userId, DbTransactionContext transContext = null)
        {
            IList<SignInToken> resultList = await Database.RetrieveAsync<SignInToken>(at => at.UserId == userId, transContext);

            foreach (SignInToken at in resultList)
            {
                AuthorizationServerResult err = (await Database.DeleteAsync(at, transContext)).ToAuthorizationResult();

                if (err != AuthorizationServerResult.Succeeded)
                {
                    Logger.LogCritical(0, "AuthenticationToken delete failed, userId : " + userId, null);

                    return err;
                }
            }

            return AuthorizationServerResult.Succeeded;
        }

        public async Task<AuthorizationServerResult> DeleteBySignInTokenIdentifierAsync(string signInTokenIdentifier, DbTransactionContext transContext = null)
        {
            IList<SignInToken> resultList = await Database.RetrieveAsync<SignInToken>(at => at.SignInTokenIdentifier == signInTokenIdentifier, transContext);

            foreach (SignInToken at in resultList)
            {
                AuthorizationServerResult err = (await Database.DeleteAsync(at, transContext)).ToAuthorizationResult();

                if (err != AuthorizationServerResult.Succeeded)
                {
                    Logger.LogCritical(0, "AuthenticationToken delete failed, userTokenIdentifier : " + signInTokenIdentifier, null);

                    return err;
                }
            }

            return AuthorizationServerResult.Succeeded;
        }

        public Task<SignInToken> RetrieveByAsync(string signInTokenIdentifier, string refreshToken, string clientId, long userId, DbTransactionContext transContext = null)
        {
            return Database.ScalarAsync<SignInToken>(s => 
                s.UserId ==userId && 
                s.SignInTokenIdentifier == signInTokenIdentifier && 
                s.RefreshToken == refreshToken && 
                s.ClientId == clientId, transContext );
        }

        public async Task<AuthorizationServerResult> UpdateAsync(SignInToken signInToken, DbTransactionContext transContext = null)
        {
            return (await Database.UpdateAsync(signInToken, transContext)).ToAuthorizationResult();
        }
    }
}

using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.AuthorizationServer.Abstractions
{
    public interface ISignInTokenBiz
    {
        Task<SignInToken> CreateNewTokenAsync(long userId, string clientId, string clientType, string clientVersion, string clientAddress, string ipAddress, TimeSpan expireTimeSpan, DbTransactionContext transContext = null);
        Task<AuthorizationServerResult> DeleteByUserIdAsync(long userId, DbTransactionContext transContext = null);
        Task<AuthorizationServerResult> DeleteAppClientTokenByUserIdAsync(long id, DbTransactionContext transContext = null);
        Task<AuthorizationServerResult> DeleteBySignInTokenIdentifierAsync(string signInTokenIdentifier, DbTransactionContext transContext = null);
        Task<SignInToken> RetrieveByAsync(string signInTokenIdentifier, string refreshToken, string clientId, long userId, DbTransactionContext transContext = null);
        Task<AuthorizationServerResult> UpdateAsync(SignInToken signInToken, DbTransactionContext transContext = null);
    }
}

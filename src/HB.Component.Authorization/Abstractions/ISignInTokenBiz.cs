using HB.Component.Authorization.Entity;
using HB.Framework.Database;
using HB.Framework.Database.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Authorization.Abstractions
{
    public interface ISignInTokenBiz
    {
        Task<SignInToken> CreateNewTokenAsync(long userId, string clientId, string clientType, string clientVersion, string clientAddress, string ipAddress, TimeSpan expireTimeSpan, DatabaseTransactionContext transContext = null);
        Task<AuthorizationServerResult> DeleteByUserIdAsync(long userId, DatabaseTransactionContext transContext = null);
        Task<AuthorizationServerResult> DeleteAppClientTokenByUserIdAsync(long id, DatabaseTransactionContext transContext = null);
        Task<AuthorizationServerResult> DeleteBySignInTokenIdentifierAsync(string signInTokenIdentifier, DatabaseTransactionContext transContext = null);
        Task<SignInToken> RetrieveByAsync(string signInTokenIdentifier, string refreshToken, string clientId, long userId, DatabaseTransactionContext transContext = null);
        Task<AuthorizationServerResult> UpdateAsync(SignInToken signInToken, DatabaseTransactionContext transContext = null);
    }
}

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
    internal interface ISignInTokenBiz
    {
        Task<SignInToken> CreateAsync(string userGuid, string clientId, string clientType, string clientVersion, string clientAddress, string ipAddress, TimeSpan expireTimeSpan, TransactionContext transContext = null);
        Task<AuthorizationResult> DeleteAppClientTokenByUserGuidAsync(string userGuid, TransactionContext transContext);
        Task<AuthorizationResult> DeleteAsync(string signInTokenGuid, TransactionContext transContext);
        Task<AuthorizationResult> DeleteByUserGuidAsync(string userGuid, TransactionContext transContext);
        Task<SignInToken> GetAsync(string signInTokenGuid, string refreshToken, string clientId, string userGuid, TransactionContext transContext = null);
        Task<AuthorizationResult> UpdateAsync(SignInToken signInToken, TransactionContext transContext = null);
    }
}

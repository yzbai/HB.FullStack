using HB.Framework.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HB.Framework.Database;
using HB.Framework.KVStore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using HB.Framework.AuthorizationServer.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using HB.Framework.Identity;
using System.Threading.Tasks;
using HB.Framework.Identity.Abstractions;

namespace HB.Framework.AuthorizationServer
{
    public class RefreshManager : BaseBiz, IRefreshManager
    {
        private static readonly string Frequency_Check_Key_Prefix = "Refresh_Freq_Check";
        private ISignInTokenBiz _signInTokenBiz;
        private ICredentialManager _credentialManager;
        private AuthorizationServerOptions _options;
        private IUserBiz _userBiz;
        private IJwtBuilder _jwtBuilder;

        public RefreshManager(IDatabase database, IKVStore kvStore, IDistributedCache cache, ILogger<RefreshManager> logger, 
            IOptions<AuthorizationServerOptions> options,  ICredentialManager credentialManager, ISignInTokenBiz signInTokenBiz, IUserBiz userBiz, IJwtBuilder jwtBuilder) : base(database, kvStore, cache, logger)
        {
            _options = options.Value;
            _credentialManager = credentialManager;
            _signInTokenBiz = signInTokenBiz;
            _userBiz = userBiz;
            _jwtBuilder = jwtBuilder;
        }

        public async Task<RefreshResult> RefreshAccessTokenAsync(RefreshContext context)
        {
            RefreshResult result = new RefreshResult { AccessToken = string.Empty, RefreshToken = context.RefreshToken };

            #region 频率检查

            //解决并发涌入
            if (!(await FrequencyCheckAsync(context.ClientId)))
            {
                return result;
            }

            #endregion

            #region AccessToken, Claims 验证

            ClaimsPrincipal claimsPrincipal = ValidateToken(context.AccessToken);

            if (claimsPrincipal == null)
            {
                return result;
            }

            long userId = claimsPrincipal.GetUserId();

            if (userId <= 0)
            {
                return result;
            }

            #endregion

            #region SignInToken 验证

            SignInToken signInToken = await _signInTokenBiz.RetrieveByAsync(
                claimsPrincipal.GetSignInTokenIdentifier(),
                context.RefreshToken,
                context.ClientId,
                userId
                );

            if (signInToken == null || signInToken.Blacked)
            {
                return result;
            }

            #endregion

            #region User 信息变动验证

            User user = await _userBiz.ValidateSecurityStampAsync(userId, claimsPrincipal.GetUserSecurityStamp());

            if (user == null)
            {
                await BlackSignInTokenAsync(signInToken);
                result.RefreshToken = string.Empty;
                return result;
            }

            #endregion

            #region 更新SignInToken

            signInToken.RefreshCount++;

            AuthorizationServerResult authorizationServerResult = await _signInTokenBiz.UpdateAsync(signInToken);

            if (authorizationServerResult != AuthorizationServerResult.Succeeded)
            {
                return result;
            }

            #endregion

            #region 发布新的AccessToken

            result.AccessToken = await _jwtBuilder.BuildJwtAsync(user, signInToken, claimsPrincipal.GetAudience());

            return result;

            #endregion
        }

        private async Task BlackSignInTokenAsync(SignInToken signInToken)
        {
            AuthorizationServerResult result = await _signInTokenBiz.DeleteBySignInTokenIdentifierAsync(signInToken.SignInTokenIdentifier);

            if (result != AuthorizationServerResult.Succeeded)
            {
                Logger.LogCritical($"SignInToken delete failure. Identifier:{signInToken.SignInTokenIdentifier}");
            }
        }

        private ClaimsPrincipal ValidateToken(string token)
        {
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidIssuer = _options.OpenIdConnectConfiguration.Issuer,
                IssuerSigningKeys = _credentialManager.GetIssuerSigningKeys()
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out SecurityToken validatedToken);
        }

        private async Task<bool> FrequencyCheckAsync(string clientId)
        {
            string key = $"{Frequency_Check_Key_Prefix}:{clientId}";
            string value = await Cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(value))
            {
                await Cache.SetStringAsync(key, "Hit", new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = _options.RefreshIntervalTimeSpan });
                return true;
            }

            return false;
        }
    }
}

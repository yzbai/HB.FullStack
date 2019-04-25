using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HB.Framework.Database;
using HB.Framework.KVStore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using HB.Component.Identity;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Framework.Common;
using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entity;
using HB.Component.Identity.Entity;
using HB.Framework.Cache;

namespace HB.Component.Authorization
{
    public class RefreshManager : IRefreshManager
    {
        private readonly ISignInTokenBiz _signInTokenBiz;
        private readonly ICredentialManager _credentialManager;
        private readonly AuthorizationServerOptions _options;
        //private IUserBiz _userBiz;
        private readonly IIdentityManager _identityManager;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly IFrequencyChecker _frequencyChecker;

        private readonly ILogger _logger;

        public RefreshManager(IOptions<AuthorizationServerOptions> options, ILogger<RefreshManager> logger, 
            IFrequencyChecker frequencyChecker, ISignInTokenBiz signInTokenBiz, IIdentityManager identityManager, 
            ICredentialManager credentialManager,  IJwtBuilder jwtBuilder)
        {
            _options = options.Value;
            _credentialManager = credentialManager;
            _signInTokenBiz = signInTokenBiz;
            _identityManager = identityManager;
            _jwtBuilder = jwtBuilder;

            _frequencyChecker = frequencyChecker;

            _logger = logger;
        }

        public async Task<RefreshResult> RefreshAccessTokenAsync(RefreshContext context)
        {
            #region 频率检查

            //解决并发涌入

            if (!(await _frequencyChecker.CheckAsync(context.ClientId, _options.RefreshIntervalTimeSpan).ConfigureAwait(false)))
            {
                return RefreshResult.TooFrequent();
            }

            #endregion

            #region AccessToken, Claims 验证

            ClaimsPrincipal claimsPrincipal = ValidateToken(context);

            if (claimsPrincipal == null)
            {
                return RefreshResult.InvalideAccessToken();
            }

            long userId = claimsPrincipal.GetUserId();

            if (userId <= 0)
            {
                _logger.LogWarning("Refresh token error. UserId should > 0. Context : {0}", JsonUtil.ToJson(context));
                return RefreshResult.InvalideUserId();
            }

            #endregion

            #region SignInToken 验证

            SignInToken signInToken = await _signInTokenBiz.RetrieveByAsync(
                claimsPrincipal.GetSignInTokenIdentifier(),
                context.RefreshToken,
                context.ClientId,
                userId
                ).ConfigureAwait(false);

            if (signInToken == null || signInToken.Blacked)
            {
                _logger.LogWarning("Refresh token error. signInToken not saved in db. Context : {0}", JsonUtil.ToJson(context));
                return RefreshResult.NoTokenInStore();
            }

            #endregion

            #region User 信息变动验证

            User user = await _identityManager.ValidateSecurityStampAsync(userId, claimsPrincipal.GetUserSecurityStamp()).ConfigureAwait(false);

            if (user == null)
            {
                await BlackSignInTokenAsync(signInToken).ConfigureAwait(false);

                _logger.LogWarning("Refresh token error. User SecurityStamp Changed. Context : {0}", JsonUtil.ToJson(context));

                return RefreshResult.UserSecurityStampChanged();
            }

            #endregion

            #region 更新SignInToken

            signInToken.RefreshCount++;

            AuthorizationServerResult authorizationServerResult = await _signInTokenBiz.UpdateAsync(signInToken).ConfigureAwait(false);

            if (!authorizationServerResult.IsSucceeded())
            {
                _logger.LogError("Refresh token error. Update SignIn Error. Context : {0}", JsonUtil.ToJson(context));
                return RefreshResult.UpdateSignInTokenError();
            }

            #endregion

            #region 发布新的AccessToken

            RefreshResult result = new RefreshResult() { Status = RefreshResultStatus.Succeeded };

            result.AccessToken = await _jwtBuilder.BuildJwtAsync(user, signInToken, claimsPrincipal.GetAudience()).ConfigureAwait(false);
            result.RefreshToken = context.RefreshToken;
            result.CurrentUser = user;

            return result;

            #endregion
        }

        private async Task BlackSignInTokenAsync(SignInToken signInToken)
        {
            AuthorizationServerResult result = await _signInTokenBiz.DeleteBySignInTokenIdentifierAsync(signInToken.SignInTokenIdentifier).ConfigureAwait(false);

            if (!result.IsSucceeded())
            {
                _logger.LogCritical($"SignInToken delete failure. Identifier:{signInToken.SignInTokenIdentifier}");
            }
        }

        private ClaimsPrincipal ValidateToken(RefreshContext context)
        {
            try
            {
                TokenValidationParameters parameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidIssuer = _options.OpenIdConnectConfiguration.Issuer,
                    IssuerSigningKeys = _credentialManager.GetIssuerSigningKeys()
                };

                return new JwtSecurityTokenHandler().ValidateToken(context.AccessToken, parameters, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "wrong token to refren.Context : {0}", JsonUtil.ToJson(context));
                return null;
            }
        }
    }
}

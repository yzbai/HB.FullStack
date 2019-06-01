using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entity;
using HB.Component.Identity;
using HB.Component.Identity.Entity;
using HB.Framework.Cache;
using HB.Framework.Database.Transaction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly ITransaction transaction;
        private readonly AuthorizationOptions _options;
        private readonly SignInOptions _signInOptions;
        private readonly IIdentityService _identityService;
        private readonly ISignInTokenBiz _signInTokenBiz;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly ICredentialBiz _credentialBiz;
        private readonly IFrequencyChecker _frequencyChecker;

        private readonly ILogger logger;

        public AuthorizationService(ITransaction transaction, IOptions<AuthorizationOptions> options, ILogger<AuthorizationService> logger, IFrequencyChecker frequencyChecker,
            ISignInTokenBiz signInTokenBiz, IIdentityService identityManager, IJwtBuilder jwtBuilder, ICredentialBiz credentialManager)
        {
            this.transaction = transaction;
            _options = options.Value;
            _signInOptions = _options.SignInOptions;

            this.logger = logger;
            _frequencyChecker = frequencyChecker;

            _signInTokenBiz = signInTokenBiz;
            _identityService = identityManager;
            _jwtBuilder = jwtBuilder;
            _credentialBiz = credentialManager;

        }

        public async Task<AuthorizationResult> SignOutAsync(string signInTokenGuid)
        {
            TransactionContext transactionContext = transaction.BeginTransaction<SignInToken>();
            try
            {
                AuthorizationResult result = await _signInTokenBiz.DeleteAsync(signInTokenGuid, transactionContext).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    transaction.Rollback(transactionContext);
                    return result;
                }

                transaction.Commit(transactionContext);

                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback(transactionContext);
                logger.LogCritical(ex, $"SignInTokenGuid:{signInTokenGuid}");
                return AuthorizationResult.Throwed();
            }
        }

        public async Task<SignInResult> SignInAsync(SignInContext context)
        {
            TransactionContext transactionContext = transaction.BeginTransaction<SignInToken>();

            try
            {
                #region Retrieve User

                User user = null;

                if (!context.IsValid())
                {
                    return SignInResult.ArgumentError();
                }

                if (context.SignInType == SignInType.BySms)
                {
                    if (string.IsNullOrEmpty(context.Mobile))
                    {
                        return SignInResult.ArgumentError();
                    }

                    user = await _identityService.GetUserByMobileAsync(context.Mobile).ConfigureAwait(false);
                }
                else if (context.SignInType == SignInType.ByMobileAndPassword)
                {
                    if (string.IsNullOrEmpty(context.Mobile) || string.IsNullOrEmpty(context.Password))
                    {
                        return SignInResult.ArgumentError();
                    }

                    user = await _identityService.GetUserByMobileAsync(context.Mobile).ConfigureAwait(false);
                }
                else if (context.SignInType == SignInType.ByUserNameAndPassword)
                {
                    if (string.IsNullOrEmpty(context.UserName) || string.IsNullOrEmpty(context.Password))
                    {
                        return SignInResult.ArgumentError();
                    }

                    user = await _identityService.GetUserByUserNameAsync(context.UserName).ConfigureAwait(false);
                }

                #endregion

                #region New User 

                bool newUserCreated = false;

                if (user == null && context.SignInType == SignInType.BySms)
                {
                    IdentityResult identityResult = await _identityService.CreateUserByMobileAsync(context.UserType, context.Mobile, context.UserName, context.Password, true).ConfigureAwait(false);

                    if (identityResult.Status == IdentityResultStatus.Failed)
                    {
                        return SignInResult.NewUserCreateFailed();
                    }
                    else if (identityResult.Status == IdentityResultStatus.EmailAlreadyTaken)
                    {
                        return SignInResult.NewUserCreateFailedEmailAlreadyTaken();
                    }
                    else if (identityResult.Status == IdentityResultStatus.MobileAlreadyTaken)
                    {
                        return SignInResult.NewUserCreateFailedMobileAlreadyTaken();
                    }
                    else if (identityResult.Status == IdentityResultStatus.UserNameAlreadyTaken)
                    {
                        return SignInResult.NewUserCreateFailedUserNameAlreadyTaken();
                    }

                    newUserCreated = true;

                    user = identityResult.User;
                }

                if (user == null)
                {
                    return SignInResult.NoSuchUser();
                }

                #endregion

                #region Password Check

                if (context.SignInType == SignInType.ByMobileAndPassword || context.SignInType == SignInType.ByUserNameAndPassword)
                {
                    if (!PassowrdCheck(user, context.Password))
                    {
                        await OnPasswordCheckFailedAsync(user).ConfigureAwait(false);
                        return SignInResult.PasswordWrong();
                    }
                }

                #endregion

                #region Pre Sign Check 

                SignInResult result = await PreSignInCheckAsync(user).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    return result;
                }

                #endregion

                #region Logoff App Client

                if (context.ClientType != ClientType.Web && _signInOptions.AllowOnlyOneAppClient)
                {
                    AuthorizationResult authorizationResult = await _signInTokenBiz.DeleteAppClientTokenByUserGuidAsync(user.Guid, transactionContext).ConfigureAwait(false);

                    if (!authorizationResult.IsSucceeded())
                    {
                        transaction.Rollback(transactionContext);
                        return SignInResult.LogoffOtherClientFailed();
                    }
                }

                #endregion

                #region Create User Token

                SignInToken userToken = await _signInTokenBiz.CreateAsync(
                    user.Guid,
                    context.ClientId,
                    context.ClientType.GetDescription(),
                    context.ClientVersion,
                    context.ClientAddress,
                    context.ClientIp,
                    context.RememberMe ? _signInOptions.RefreshTokenLongExpireTimeSpan : _signInOptions.RefreshTokenShortExpireTimeSpan,
                    transactionContext).ConfigureAwait(false);

                if (userToken == null)
                {
                    transaction.Rollback(transactionContext);
                    return SignInResult.AuthtokenCreatedFailed();
                }

                #endregion

                #region Construct Jwt

                result.AccessToken = await _jwtBuilder.BuildJwtAsync(user, userToken, context.SignToWhere).ConfigureAwait(false);
                result.RefreshToken = userToken.RefreshToken;
                result.NewUserCreated = newUserCreated;
                result.CurrentUser = user;

                transaction.Commit(transactionContext);

                return result;

                #endregion
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"SignInContext:{JsonUtil.ToJson(context)}");
                transaction.Rollback(transactionContext);
                return SignInResult.Throwed();
            }
        }

        public async Task<RefreshResult> RefreshAccessTokenAsync(RefreshContext context)
        {
            if (!context.IsValid())
            {
                return RefreshResult.ArgumentError();
            }

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

            string userGuid = claimsPrincipal.GetUserGuid();

            if (string.IsNullOrEmpty(userGuid))
            {
                logger.LogWarning($"Refresh token error. UserGuid should not empty. Context : {JsonUtil.ToJson(context)}");
                return RefreshResult.InvalideUserGuid();
            }

            #endregion

            #region SignInToken 验证

            User user;
            SignInToken signInToken;
            TransactionContext transactionContext = transaction.BeginTransaction<SignInToken>();

            try
            {

                signInToken = await _signInTokenBiz.GetAsync(
                    claimsPrincipal.GetSignInTokenGuid(),
                    context.RefreshToken,
                    context.ClientId,
                    userGuid,
                    transactionContext
                    ).ConfigureAwait(false);

                if (signInToken == null || signInToken.Blacked)
                {
                    transaction.Rollback(transactionContext);
                    logger.LogWarning("Refresh token error. signInToken not saved in db. Context : {0}", JsonUtil.ToJson(context));
                    return RefreshResult.NoTokenInStore();
                }

                #endregion

                #region User 信息变动验证

                user = await _identityService.ValidateSecurityStampAsync(userGuid, claimsPrincipal.GetUserSecurityStamp()).ConfigureAwait(false);

                if (user == null)
                {
                    transaction.Rollback(transactionContext);

                    await BlackSignInTokenAsync(signInToken).ConfigureAwait(false);

                    logger.LogWarning("Refresh token error. User SecurityStamp Changed. Context : {0}", JsonUtil.ToJson(context));

                    return RefreshResult.UserSecurityStampChanged();
                }

                #endregion

                #region 更新SignInToken

                signInToken.RefreshCount++;

                AuthorizationResult authorizationServerResult = await _signInTokenBiz.UpdateAsync(signInToken, transactionContext).ConfigureAwait(false);

                if (!authorizationServerResult.IsSucceeded())
                {
                    transaction.Rollback(transactionContext);

                    logger.LogError("Refresh token error. Update SignIn Error. Context : {0}", JsonUtil.ToJson(context));
                    return RefreshResult.UpdateSignInTokenError();
                }

                #endregion

                transaction.Commit(transactionContext);

            }
            catch (Exception ex)
            {
                transaction.Rollback(transactionContext);
                logger.LogCritical(ex, $"RefreshContext:{JsonUtil.ToJson(context)}");

                return RefreshResult.Throwed();
            }

            #region 发布新的AccessToken

            RefreshResult result = new RefreshResult() { Status = RefreshResultStatus.Succeeded };

            result.AccessToken = await _jwtBuilder.BuildJwtAsync(user, signInToken, claimsPrincipal.GetAudience()).ConfigureAwait(false);
            result.RefreshToken = context.RefreshToken;
            result.CurrentUser = user;

            return result;

            #endregion
        }

        private async Task<SignInResult> PreSignInCheckAsync(User user)
        {
            if (user == null)
            {
                return SignInResult.NoSuchUser();
            }

            //2, 手机验证
            if (_signInOptions.RequireMobileConfirmed && !user.MobileConfirmed)
            {
                return SignInResult.MobileNotConfirmed();
            }

            //3, 邮件验证
            if (_signInOptions.RequireEmailConfirmed && !user.EmailConfirmed)
            {
                return SignInResult.EmailNotConfirmed();
            }

            //4, Lockout 检查
            if (_signInOptions.RequiredLockoutCheck && user.LockoutEnabled && user.LockoutEndDate > DateTimeOffset.UtcNow)
            {
                return SignInResult.LockedOut();
            }

            //5, 一天内,最大失败数检测
            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                if (DateTimeOffset.UtcNow - user.AccessFailedLastTime < TimeSpan.FromDays(_signInOptions.AccessFailedRecoveryDays))
                {
                    if (user.AccessFailedCount > _signInOptions.MaxFailedCount)
                    {
                        return SignInResult.OverMaxFailedCount();
                    }
                }
            }

            if (_signInOptions.RequiredLockoutCheck)
            {
                await _identityService.SetLockoutAsync(user.Guid, false).ConfigureAwait(false);
            }

            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                await _identityService.SetAccessFailedCountAsync(user.Guid, 0).ConfigureAwait(false);
            }

            if (_signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }

            return SignInResult.Succeeded();
        }

        private static bool PassowrdCheck(User user, string password)
        {
            string passwordHash = SecurityUtil.EncryptPwdWithSalt(password, user.Guid);
            return passwordHash.Equals(user.PasswordHash, GlobalSettings.Comparison);
        }

        private async Task OnPasswordCheckFailedAsync(User user)
        {
            IdentityResult identityResult = IdentityResult.Failed();

            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                identityResult = await _identityService.SetAccessFailedCountAsync(user.Guid, user.AccessFailedCount + 1).ConfigureAwait(false);

                if (!identityResult.IsSucceeded())
                {
                    logger.LogCritical($"OnPasswordCheckFailedAsync Failed at SetAccessFailedCountAsync, UserGuid:{user.Guid}");
                }
            }

            if (_signInOptions.RequiredLockoutCheck)
            {
                if (user.AccessFailedCount + 1 > _signInOptions.LockoutAfterAccessFailedCount)
                {
                    identityResult = await _identityService.SetLockoutAsync(user.Guid, true, _signInOptions.LockoutTimeSpan).ConfigureAwait(false);

                    if (!identityResult.IsSucceeded())
                    {
                        logger.LogCritical($"OnPasswordCheckFailedAsync Failed at SetLockoutAsync, UserGuid:{user.Guid}");
                    }
                }
            }
        }

        private async Task BlackSignInTokenAsync(SignInToken signInToken)
        {
            TransactionContext transactionContext = transaction.BeginTransaction<SignInToken>();
            try
            {
                AuthorizationResult result = await _signInTokenBiz.DeleteAsync(signInToken.Guid, transactionContext).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    transaction.Rollback(transactionContext);
                    logger.LogCritical($"SignInToken delete failure. Identifier:{signInToken.Guid}");
                }

                transaction.Commit(transactionContext);
            }
            catch (Exception ex)
            {
                transaction.Rollback(transactionContext);
                logger.LogCritical(ex, $"SignInToken : {JsonUtil.ToJson(signInToken)}");
            }
        }

        private ClaimsPrincipal ValidateToken(RefreshContext context)
        {
            try
            {
                TokenValidationParameters parameters = new TokenValidationParameters {
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidIssuer = _options.OpenIdConnectConfiguration.Issuer,
                    IssuerSigningKeys = _credentialBiz.GetIssuerSigningKeys()
                };

                return new JwtSecurityTokenHandler().ValidateToken(context.AccessToken, parameters, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "wrong token to refren.Context : {0}", JsonUtil.ToJson(context));
                return null;
            }
        }
    }
}

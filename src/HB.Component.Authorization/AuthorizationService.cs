using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entities;
using HB.Component.Identity;
using HB.Component.Identity.Entities;
using HB.Framework.Database;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly IDatabase _database;
        private readonly AuthorizationServerOptions _options;
        private readonly SignInOptions _signInOptions;
        private readonly IIdentityService _identityService;
        private readonly ISignInTokenBiz _signInTokenBiz;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly ICredentialBiz _credentialBiz;
        private readonly DistributedCacheFrequencyChecker _frequencyChecker;

        //private readonly ILogger logger;

        public AuthorizationService(IDatabase database, IOptions<AuthorizationServerOptions> options, IDistributedCache distributedCache,
            ISignInTokenBiz signInTokenBiz, IIdentityService identityManager, IJwtBuilder jwtBuilder, ICredentialBiz credentialManager/*, ILogger<AuthorizationService> logger*/)
        {
            _database = database;
            _options = options.Value;
            _signInOptions = _options.SignInOptions;

            //this.logger = logger;
            _frequencyChecker = new DistributedCacheFrequencyChecker(distributedCache);

            _signInTokenBiz = signInTokenBiz;
            _identityService = identityManager;
            _jwtBuilder = jwtBuilder;
            _credentialBiz = credentialManager;

        }

        /// <exception cref="FileNotFoundException">证书文件不存在</exception>
        /// <exception cref="ArgumentException">Json无法解析</exception>
        public JsonWebKeySet GetJsonWebKeySet()
        {
            return _credentialBiz.JsonWebKeySet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signInTokenGuid"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public async Task SignOutAsync(string signInTokenGuid, string lastUser)
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                await _signInTokenBiz.DeleteAsync(signInTokenGuid, lastUser, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public async Task SignOutAsync(string userGuid, DeviceIdiom idiom, LogOffType logOffType, string lastUser)
        {
            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>(IsolationLevel.ReadCommitted).ConfigureAwait(false);

            try
            {
                await _signInTokenBiz.DeleteByLogOffTypeAsync(userGuid, idiom, logOffType, lastUser, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// SignInAsync
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<SignInResult> SignInAsync<TUser, TUserClaim, TRole, TRoleOfUser>(SignInContext context, string lastUser)
            where TUser : IdentityUser, new()
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new()
        {
            ThrowIf.NotValid(context);

            switch (context.SignInType)
            {
                case SignInType.ByMobileAndPassword:
                    ThrowIf.NullOrEmpty(context.Mobile, "SignInContext.Mobile");
                    ThrowIf.NullOrEmpty(context.Password, "SignInContext.Password");
                    break;
                case SignInType.BySms:
                    ThrowIf.NullOrEmpty(context.Mobile, "SignInContext.Mobile");
                    break;
                case SignInType.ByLoginNameAndPassword:
                    ThrowIf.NullOrEmpty(context.LoginName, "SignInContext.LoginName");
                    ThrowIf.NullOrEmpty(context.Password, "SignInContext.Password");
                    break;
                default:
                    break;
            }

            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>(IsolationLevel.ReadCommitted).ConfigureAwait(false);

            try
            {
                //查询用户
                TUser? user = context.SignInType switch
                {
                    SignInType.ByLoginNameAndPassword => await _identityService.GetUserByLoginNameAsync<TUser>(context.LoginName!).ConfigureAwait(false),
                    SignInType.BySms => await _identityService.GetUserByMobileAsync<TUser>(context.Mobile!).ConfigureAwait(false),
                    SignInType.ByMobileAndPassword => await _identityService.GetUserByMobileAsync<TUser>(context.Mobile!).ConfigureAwait(false),
                    _ => null
                };

                //不存在，则新建用户
                bool newUserCreated = false;

                if (user == null && context.SignInType == SignInType.BySms)
                {
                    user = await _identityService.CreateUserByMobileAsync<TUser>(context.Mobile!, context.LoginName, context.Password, true, lastUser).ConfigureAwait(false);

                    newUserCreated = true;
                }

                if (user == null)
                {
                    throw new AuthorizationException(ErrorCode.AuthorizationNotFound, $"SignInContext:{SerializeUtil.ToJson(context)}");
                }

                //密码检查
                if (context.SignInType == SignInType.ByMobileAndPassword || context.SignInType == SignInType.ByLoginNameAndPassword)
                {
                    if (!PassowrdCheck(user, context.Password!))
                    {
                        await OnPasswordCheckFailedAsync(user, lastUser).ConfigureAwait(false);

                        throw new AuthorizationException(ErrorCode.AuthorizationPasswordWrong, $"SignInContext:{SerializeUtil.ToJson(context)}");
                    }
                }

                //其他检查
                await PreSignInCheckAsync(user, lastUser).ConfigureAwait(false);

                //注销其他客户端
                await _signInTokenBiz.DeleteByLogOffTypeAsync(user.Guid, context.DeviceInfos.Idiom, context.LogOffType, context.DeviceInfos.Name, transactionContext).ConfigureAwait(false);

                //创建Token
                SignInToken userToken = await _signInTokenBiz.CreateAsync(
                    user.Guid,
                    context.DeviceId,
                    context.DeviceInfos,
                    context.DeviceVersion,
                    //context.DeviceAddress,
                    context.DeviceIp,
                    context.RememberMe ? _signInOptions.RefreshTokenLongExpireTimeSpan : _signInOptions.RefreshTokenShortExpireTimeSpan,
                    lastUser,
                    transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

                //构造 Jwt
                SignInResult result = new SignInResult
                (
                    accessToken: await _jwtBuilder.BuildJwtAsync<TUserClaim, TRole, TRoleOfUser>(user, userToken, context.SignToWhere).ConfigureAwait(false),
                    refreshToken: userToken.RefreshToken,
                    newUserCreated: newUserCreated,
                    currentUser: user
                );

                return result;

            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        //TODO: 做好详细的历史纪录，各个阶段都要打log。一有风吹草动，就立马删除SignInToken
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>新的AccessToken</returns>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        public async Task<string> RefreshAccessTokenAsync<TUser, TUserClaim, TRole, TRoleOfUser>(RefreshContext context, string lastUser)
            where TUser : IdentityUser, new()
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new()
        {
            ThrowIf.NotValid(context);

            //频率检查

            //解决并发涌入

            if (!(await _frequencyChecker.CheckAsync(nameof(RefreshAccessTokenAsync), context.DeviceId, _options.RefreshIntervalTimeSpan).ConfigureAwait(false)))
            {
                throw new AuthorizationException(ErrorCode.AuthorizationTooFrequent, $"Context:{SerializeUtil.ToJson(context)}");
            }

            //AccessToken, Claims 验证

            ClaimsPrincipal? claimsPrincipal = null;

            try
            {
                claimsPrincipal = ValidateTokenWithoutLifeCheck(context);
            }
            catch (Exception ex)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationInvalideAccessToken, $"Context: {SerializeUtil.ToJson(context)}", ex);
            }

            //TODO: 这里缺DeviceId验证. 放在了StartupUtil.cs中

            if (claimsPrincipal == null)
            {
                //TODO: Black concern SigninToken by RefreshToken
                throw new AuthorizationException(ErrorCode.AuthorizationInvalideAccessToken, $"Context: {SerializeUtil.ToJson(context)}");
            }

            if (claimsPrincipal.GetDeviceId() != context.DeviceId)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationInvalideDeviceId, $"Context: {SerializeUtil.ToJson(context)}");
            }

            string? userGuid = claimsPrincipal.GetUserGuid();

            if (string.IsNullOrEmpty(userGuid))
            {
                throw new AuthorizationException(ErrorCode.AuthorizationInvalideUserGuid, $"Context: {SerializeUtil.ToJson(context)}");
            }


            //SignInToken 验证
            TUser? user;
            SignInToken? signInToken;
            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>(IsolationLevel.ReadCommitted).ConfigureAwait(false);

            try
            {
                signInToken = await _signInTokenBiz.GetAsync(
                    claimsPrincipal.GetSignInTokenGuid(),
                    context.RefreshToken,
                    context.DeviceId,
                    userGuid,
                    transactionContext
                    ).ConfigureAwait(false);

                if (signInToken == null || signInToken.Blacked)
                {
                    //await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationNoTokenInStore, $"Refresh token error. signInToken not saved in db. ");
                }

                //验证SignInToken过期问题

                if (signInToken.ExpireAt < DateTimeOffset.UtcNow)
                {
                    await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                    await BlackSignInTokenAsync(signInToken, lastUser).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationRefreshTokenExpired, $"Refresh Token Expired.");
                }

                // User 信息变动验证

                user = await _identityService.GetUserBySecurityStampAsync<TUser>(userGuid, claimsPrincipal.GetUserSecurityStamp()).ConfigureAwait(false);

                if (user == null)
                {
                    await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                    await BlackSignInTokenAsync(signInToken, lastUser).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationUserSecurityStampChanged, $"Refresh token error. User SecurityStamp Changed.");
                }

                // 更新SignInToken
                signInToken.RefreshCount++;

                await _signInTokenBiz.UpdateAsync(signInToken, lastUser, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);

            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            // 发布新的AccessToken

            return await _jwtBuilder.BuildJwtAsync<TUserClaim, TRole, TRoleOfUser>(user, signInToken, claimsPrincipal.GetAudience()).ConfigureAwait(false);
        }

        /// <summary>
        /// PreSignInCheckAsync
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        private Task PreSignInCheckAsync<TUser>(TUser user, string lastUser) where TUser : IdentityUser, new()
        {
            ThrowIf.Null(user, nameof(user));

            //2, 手机验证
            if (_signInOptions.RequireMobileConfirmed && !user.MobileConfirmed)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationMobileNotConfirmed, $"user:{SerializeUtil.ToJson(user)}");
            }

            //3, 邮件验证
            if (_signInOptions.RequireEmailConfirmed && !user.EmailConfirmed)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationEmailNotConfirmed, $"user:{SerializeUtil.ToJson(user)}");
            }

            //4, Lockout 检查
            if (_signInOptions.RequiredLockoutCheck && user.LockoutEnabled && user.LockoutEndDate > DateTimeOffset.UtcNow)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationLockedOut, $"user:{SerializeUtil.ToJson(user)}");
            }

            //5, 一天内,最大失败数检测
            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                if (DateTimeOffset.UtcNow - user.AccessFailedLastTime < TimeSpan.FromDays(_signInOptions.AccessFailedRecoveryDays))
                {
                    if (user.AccessFailedCount > _signInOptions.MaxFailedCount)
                    {
                        throw new AuthorizationException(ErrorCode.AuthorizationOverMaxFailedCount, $"user:{SerializeUtil.ToJson(user)}");
                    }
                }
            }
            Task setLockTask = _signInOptions.RequiredLockoutCheck ? _identityService.SetLockoutAsync<TUser>(user.Guid, false, lastUser) : Task.CompletedTask;
            Task setAccessFailedCountTask = _signInOptions.RequiredMaxFailedCountCheck ? _identityService.SetAccessFailedCountAsync<TUser>(user.Guid, 0, lastUser) : Task.CompletedTask;

            if (_signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }

            return Task.WhenAll(setLockTask, setAccessFailedCountTask);
        }

        /// <summary>
        /// PassowrdCheck
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="System.Reflection.TargetInvocationException">Ignore.</exception>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        private static bool PassowrdCheck(IdentityUser user, string password)
        {
            string passwordHash = SecurityUtil.EncryptPwdWithSalt(password, user.Guid);
            return passwordHash.Equals(user.PasswordHash, GlobalSettings.Comparison);
        }

        private Task OnPasswordCheckFailedAsync<TUser>(TUser user, string lastUser) where TUser : IdentityUser, new()
        {
            Task setAccessFailedCountTask = Task.CompletedTask;

            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                setAccessFailedCountTask = _identityService.SetAccessFailedCountAsync<TUser>(user.Guid, user.AccessFailedCount + 1, lastUser);
            }

            Task setLockoutTask = Task.CompletedTask;

            if (_signInOptions.RequiredLockoutCheck)
            {
                if (user.AccessFailedCount + 1 > _signInOptions.LockoutAfterAccessFailedCount)
                {
                    setLockoutTask = _identityService.SetLockoutAsync<TUser>(user.Guid, true, lastUser, _signInOptions.LockoutTimeSpan);
                }
            }

            return Task.WhenAll(setAccessFailedCountTask, setLockoutTask);
        }

        /// <summary>
        /// BlackSignInTokenAsync
        /// </summary>
        /// <param name="signInToken"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        private async Task BlackSignInTokenAsync(SignInToken signInToken, string lastUser)
        {
            //TODO: 详细记录Black SiginInToken 的历史纪录
            TransactionContext transactionContext = await _database.BeginTransactionAsync<SignInToken>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                await _signInTokenBiz.DeleteAsync(signInToken.Guid, lastUser, transactionContext).ConfigureAwait(false);

                await _database.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                throw;
            }
        }

        /// <summary>
        /// ValidateTokenWithoutLifeCheck
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="SecurityTokenDecryptionFailedException"></exception>
        /// <exception cref="SecurityTokenEncryptionKeyNotFoundException"></exception>
        /// <exception cref="SecurityTokenException"></exception>
        /// <exception cref="SecurityTokenExpiredException"></exception>
        /// <exception cref="SecurityTokenInvalidAudienceException"></exception>
        /// <exception cref="SecurityTokenInvalidLifetimeException"></exception>
        /// <exception cref="SecurityTokenInvalidSignatureException"></exception>
        /// <exception cref="SecurityTokenNoExpirationException"></exception>
        /// <exception cref="SecurityTokenNotYetValidException"></exception>
        /// <exception cref="SecurityTokenReplayAddFailedException"></exception>
        /// <exception cref="SecurityTokenReplayDetectedException"></exception>
        /// <exception cref="HB.Component.Authorization.AuthorizationException"></exception>
        private ClaimsPrincipal ValidateTokenWithoutLifeCheck(RefreshContext context)
        {
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidIssuer = _options.OpenIdConnectConfiguration.Issuer,
                IssuerSigningKeys = _credentialBiz.IssuerSigningKeys,
                TokenDecryptionKey = _credentialBiz.DecryptionSecurityKey
            };
            return new JwtSecurityTokenHandler().ValidateToken(context.AccessToken, parameters, out _);
        }


    }
}

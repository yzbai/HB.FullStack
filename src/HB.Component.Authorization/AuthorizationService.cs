using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entities;
using HB.Component.Identity;
using HB.Component.Identity.Entities;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.DistributedLock;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly AuthorizationServerOptions _options;
        private readonly ITransaction _transaction;
        private readonly IDistributedLockManager _lockManager;

        private readonly IIdentityService _identityService;
        private readonly SignInTokenBiz _signInTokenBiz;

        //Jwt Signing
        private readonly JsonWebKeySet _jsonWebKeySet;
        private readonly IEnumerable<SecurityKey> _issuerSigningKeys;

        //Jwt Content Encrypt
        private readonly EncryptingCredentials _encryptingCredentials;
        private readonly SecurityKey _decryptionSecurityKey;

        public AuthorizationService(IOptions<AuthorizationServerOptions> options, ITransaction transaction, IDistributedLockManager lockManager,
            SignInTokenBiz signInTokenBiz, IIdentityService identityService/*, ILogger<AuthorizationService> logger*/)
        {
            _options = options.Value;
            _transaction = transaction;
            _lockManager = lockManager;
            _identityService = identityService;

            _signInTokenBiz = signInTokenBiz;

            #region Initialize Jwt Signing Credentials

            X509Certificate2? cert = CertificateUtil.GetBySubject(_options.SigningCertificateSubject);

            if (cert == null)
            {
                throw new AuthorizationException(ErrorCode.JwtSigningCertNotFound, $"Subject:{_options.SigningCertificateSubject}");
            }

            _jsonWebKeySet = CredentialHelper.CreateJsonWebKeySet(cert);
            _issuerSigningKeys = _jsonWebKeySet.GetSigningKeys();

            #endregion

            #region Initialize Jwt Content Encrypt/Decrypt Credentials

            X509Certificate2? encryptionCert = CertificateUtil.GetBySubject(_options.EncryptingCertificateSubject);

            if (encryptionCert == null)
            {
                throw new FrameworkException(ErrorCode.JwtEncryptionCertNotFound, $"Subject:{_options.EncryptingCertificateSubject}");
            }

            _encryptingCredentials = CredentialHelper.GetEncryptingCredentials(encryptionCert);
            _decryptionSecurityKey = CredentialHelper.GetSecurityKey(encryptionCert);

            #endregion
        }

        public JsonWebKeySet JsonWebKeySet => _jsonWebKeySet;

        public async Task SignOutAsync(string signInTokenGuid, string lastUser)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);
            try
            {
                await _signInTokenBiz.DeleteByGuidAsync(signInTokenGuid, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public async Task SignOutAsync(string userGuid, DeviceIdiom idiom, LogOffType logOffType, string lastUser)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                await _signInTokenBiz.DeleteByLogOffTypeAsync(userGuid, idiom, logOffType, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<SignInResult> SignInAsync(SignInContext context, string lastUser)
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

            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                //查询用户
                User? user = context.SignInType switch
                {
                    SignInType.ByLoginNameAndPassword => await _identityService.GetUserByLoginNameAsync(context.LoginName!).ConfigureAwait(false),
                    SignInType.BySms => await _identityService.GetUserByMobileAsync(context.Mobile!).ConfigureAwait(false),
                    SignInType.ByMobileAndPassword => await _identityService.GetUserByMobileAsync(context.Mobile!).ConfigureAwait(false),
                    _ => null
                };

                //不存在，则新建用户
                bool newUserCreated = false;

                if (user == null && context.SignInType == SignInType.BySms)
                {
                    user = await _identityService.CreateUserAsync(
                        mobile: context.Mobile!,
                        email: null,
                        loginName: context.LoginName,
                        password: context.Password,
                        mobileConfirmed: true,
                        emailConfirmed: false,
                        lastUser: lastUser).ConfigureAwait(false);

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
                    context.RememberMe ? _options.SignInOptions.RefreshTokenLongExpireTimeSpan : _options.SignInOptions.RefreshTokenShortExpireTimeSpan,
                    lastUser,
                    transactionContext).ConfigureAwait(false);

                //构造 Jwt



                IEnumerable<Claim> claims = GetClaims(user, transactionContext);

                SignInResult result = new SignInResult
                (
                    accessToken: await JwtHelper.BuildJwt(user, userToken, context.SignToWhere).ConfigureAwait(false),
                    refreshToken: userToken.RefreshToken,
                    newUserCreated: newUserCreated,
                    currentUser: user
                );

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return result;

            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        //TODO: 做好详细的历史纪录，各个阶段都要打log。一有风吹草动，就立马删除SignInToken
        /// <returns>新的AccessToken</returns>
        public async Task<string> RefreshAccessTokenAsync(RefreshContext context, string lastUser)
            where User : User, new()
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new()
        {
            ThrowIf.NotValid(context);

            //解决并发涌入
            using IDistributedLock distributedLock = await _lockManager.NoWaitLockAsync(
                nameof(RefreshAccessTokenAsync) + context.DeviceId,
                _options.RefreshIntervalTimeSpan).ConfigureAwait(false);

            if (!distributedLock.IsAcquired)
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
            User? user;
            SignInToken? signInToken;
            //TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                signInToken = await _signInTokenBiz.GetAsync(
                    claimsPrincipal.GetSignInTokenGuid(),
                    context.RefreshToken,
                    context.DeviceId,
                    userGuid,


                    ).ConfigureAwait(false);

                if (signInToken == null || signInToken.Blacked)
                {
                    //await _database.RollbackAsync(transactionContext).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationNoTokenInStore, $"Refresh token error. signInToken not saved in db. ");
                }

                //验证SignInToken过期问题

                if (signInToken.ExpireAt < DateTimeOffset.UtcNow)
                {
                    //await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                    await BlackSignInTokenAsync(signInToken, lastUser).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationRefreshTokenExpired, $"Refresh Token Expired.");
                }

                // User 信息变动验证

                user = await _identityService.GetUserByUserGuidAsync(userGuid).ConfigureAwait(false);

                if (user == null || user.SecurityStamp != claimsPrincipal.GetUserSecurityStamp())
                {
                    //await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                    await BlackSignInTokenAsync(signInToken, lastUser).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationUserSecurityStampChanged, $"Refresh token error. User SecurityStamp Changed.");
                }

                // 更新SignInToken
                signInToken.RefreshCount++;

                await _signInTokenBiz.UpdateAsync(signInToken, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            // 发布新的AccessToken

            return await _jwtBuilder.BuildJwtAsync(user, signInToken, claimsPrincipal.GetAudience()).ConfigureAwait(false);
        }

        private Task PreSignInCheckAsync(User user, string lastUser) where User : User, new()
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
            Task setLockTask = _signInOptions.RequiredLockoutCheck ? _identityService.SetLockoutAsync(user.Guid, false, lastUser) : Task.CompletedTask;
            Task setAccessFailedCountTask = _signInOptions.RequiredMaxFailedCountCheck ? _identityService.SetAccessFailedCountAsync(user.Guid, 0, lastUser) : Task.CompletedTask;

            if (_signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }

            return Task.WhenAll(setLockTask, setAccessFailedCountTask);
        }

        private static bool PassowrdCheck(User user, string password)
        {
            string passwordHash = SecurityUtil.EncryptPwdWithSalt(password, user.Guid);
            return passwordHash.Equals(user.PasswordHash, GlobalSettings.Comparison);
        }

        private Task OnPasswordCheckFailedAsync(User user, string lastUser)
        {
            Task setAccessFailedCountTask = Task.CompletedTask;

            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                setAccessFailedCountTask = _identityService.SetAccessFailedCountAsync(user.Guid, user.AccessFailedCount + 1, lastUser);
            }

            Task setLockoutTask = Task.CompletedTask;

            if (_signInOptions.RequiredLockoutCheck)
            {
                if (user.AccessFailedCount + 1 > _signInOptions.LockoutAfterAccessFailedCount)
                {
                    setLockoutTask = _identityService.SetLockoutAsync(user.Guid, true, lastUser, _signInOptions.LockoutTimeSpan);
                }
            }

            return Task.WhenAll(setAccessFailedCountTask, setLockoutTask);
        }

        /// <summary>
        /// BlackSignInTokenAsync
        /// </summary>
        /// <param name="signInToken"></param>
        /// <returns></returns>
        private async Task BlackSignInTokenAsync(SignInToken signInToken, string lastUser)
        {
            //TODO: 详细记录Black SiginInToken 的历史纪录
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);
            try
            {
                await _signInTokenBiz.DeleteByGuidAsync(signInToken.Guid, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                throw;
            }
        }

        private ClaimsPrincipal ValidateTokenWithoutLifeCheck(RefreshContext context)
        {
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidIssuer = _options.OpenIdConnectConfiguration.Issuer,
                IssuerSigningKeys = _issuerSigningKeys,
                TokenDecryptionKey = _decryptionSecurityKey
            };
            return new JwtSecurityTokenHandler().ValidateToken(context.AccessToken, parameters, out _);
        }

        public static IEnumerable<Claim> GetClaims(User user, IEnumerable<Role> roles, IEnumerable<UserClaim> userClaims, SignInToken signInToken)
        {
            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimExtensionTypes.UserGuid, user.Guid),
                new Claim(ClaimExtensionTypes.SecurityStamp, user.SecurityStamp),
                //new Claim(ClaimExtensionTypes.UserId, user.Id.ToString(GlobalSettings.Culture)),
                new Claim(ClaimExtensionTypes.LoginName, user.LoginName??""),
                //new Claim(ClaimExtensionTypes.MobilePhone, user.Mobile??""),
                //new Claim(ClaimExtensionTypes.IsMobileConfirmed, user.MobileConfirmed.ToString(GlobalSettings.Culture))
            };

            foreach (UserClaim item in userClaims)
            {
                if (item.AddToJwt)
                {
                    claims.Add(new Claim(item.ClaimType, item.ClaimValue));
                }

            }

            foreach (Role item in roles)
            {
                claims.Add(new Claim(ClaimExtensionTypes.Role, item.Name));
            }


            claims.Add(new Claim(ClaimExtensionTypes.SignInTokenGuid, signInToken.Guid));
            claims.Add(new Claim(ClaimExtensionTypes.DeviceId, signInToken.DeviceId));

            return claims;
        }

    }
}

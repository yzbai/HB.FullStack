using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Identity.Entities;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HB.FullStack.Identity
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly AuthorizationServiceOptions _options;
        private readonly ILogger _logger;
        private readonly ITransaction _transaction;
        private readonly IDistributedLockManager _lockManager;

        private readonly UserRepo _userRepo;
        private readonly SignInTokenRepo _signInTokenRepo;
        private readonly RoleOfUserRepo _roleOfUserRepo;
        private readonly UserClaimRepo _userClaimRepo;
        private readonly UserLoginControlRepo _userLoginControlRepo;

        private readonly IIdentityService _identityService;

        //Jwt Signing
        private string _jsonWebKeySetJson = null!;
        private IEnumerable<SecurityKey> _issuerSigningKeys = null!;
        private SigningCredentials _signingCredentials = null!;


        //Jwt Content Encrypt
        private EncryptingCredentials _encryptingCredentials = null!;
        private SecurityKey _decryptionSecurityKey = null!;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="transaction"></param>
        /// <param name="lockManager"></param>
        /// <param name="userRepo"></param>
        /// <param name="signInTokenRepo"></param>
        /// <param name="roleOfUserRepo"></param>
        /// <param name="userClaimRepo"></param>
        /// <param name="userLoginControlRepo"></param>
        /// <param name="identityService"></param>
        /// <exception cref="IdentityException"></exception>
        public AuthorizationService(
            IOptions<AuthorizationServiceOptions> options,
            ILogger<AuthorizationService> logger,
            ITransaction transaction,
            IDistributedLockManager lockManager,
            UserRepo userRepo,
            SignInTokenRepo signInTokenRepo,
            RoleOfUserRepo roleOfUserRepo,
            UserClaimRepo userClaimRepo,
            UserLoginControlRepo userLoginControlRepo,
            IIdentityService identityService)
        {
            _options = options.Value;
            _logger = logger;
            _transaction = transaction;
            _lockManager = lockManager;

            _userRepo = userRepo;
            _roleOfUserRepo = roleOfUserRepo;
            _userClaimRepo = userClaimRepo;
            _userLoginControlRepo = userLoginControlRepo;
            _signInTokenRepo = signInTokenRepo;

            _identityService = identityService;

            InitializeCredencials();
        }

        public string JsonWebKeySetJson => _jsonWebKeySetJson;

        /// <summary>
        /// SignInAsync
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="KVStoreException"></exception>
        /// <exception cref="CacheException"></exception>
        public async Task<UserAccessResult> SignInAsync(SignInContext context, string lastUser)
        {
            ThrowIf.NotValid(context, nameof(context));

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
                    SignInType.ByLoginNameAndPassword => await _userRepo.GetByLoginNameAsync(context.LoginName!, transactionContext).ConfigureAwait(false),
                    SignInType.BySms => await _userRepo.GetByMobileAsync(context.Mobile!, transactionContext).ConfigureAwait(false),
                    SignInType.ByMobileAndPassword => await _userRepo.GetByMobileAsync(context.Mobile!, transactionContext).ConfigureAwait(false),
                    _ => null
                };

                //不存在，则新建用户

                if (user == null && context.SignInType == SignInType.BySms)
                {
                    user = await _identityService.CreateUserAsync(context.Mobile!, null, context.LoginName, context.Password, true, false, lastUser, transactionContext).ConfigureAwait(false);
                }

                if (user == null)
                {
                    throw new IdentityException(IdentityErrorCode.AuthorizationNotFound, $"SignInContext:{SerializeUtil.ToJson(context)}");
                }

                UserLoginControl userLoginControl = await GetOrCreateUserLoginControlAsync(lastUser, user.Id).ConfigureAwait(false);

                //密码检查
                if (context.SignInType == SignInType.ByMobileAndPassword || context.SignInType == SignInType.ByLoginNameAndPassword)
                {
                    if (!PassowrdCheck(user, context.Password!))
                    {
                        await OnSignInFailedAsync(userLoginControl, lastUser).ConfigureAwait(false);

                        throw new IdentityException(IdentityErrorCode.AuthorizationPasswordWrong, $"SignInContext:{SerializeUtil.ToJson(context)}");
                    }
                }

                //其他检查
                await PreSignInCheckAsync(user, userLoginControl, lastUser).ConfigureAwait(false);

                //注销其他客户端
                await DeleteSignInTokensAsync(user.Id, context.DeviceInfos.Idiom, context.LogOffType, context.DeviceInfos.Name, transactionContext).ConfigureAwait(false);

                //创建Token

                SignInToken signInToken = new SignInToken
                (
                    userId: user.Id,
                    refreshToken: SecurityUtil.CreateUniqueToken(),
                    expireAt: TimeUtil.UtcNow + (context.RememberMe ? _options.SignInOptions.RefreshTokenLongExpireTimeSpan : _options.SignInOptions.RefreshTokenShortExpireTimeSpan),
                    deviceId: context.DeviceId,
                    deviceVersion: context.DeviceVersion,
                    deviceIp: context.DeviceIp,

                    deviceName: context.DeviceInfos.Name,
                    deviceModel: context.DeviceInfos.Model,
                    deviceOSVersion: context.DeviceInfos.OSVersion,
                    devicePlatform: context.DeviceInfos.Platform,
                    deviceIdiom: context.DeviceInfos.Idiom,
                    deviceType: context.DeviceInfos.Type
                );

                await _signInTokenRepo.AddAsync(signInToken, lastUser, transactionContext).ConfigureAwait(false);

                //构造 Jwt
                string jwt = await ConstructJwtAsync(user, signInToken, context.SignToWhere, transactionContext).ConfigureAwait(false);

                UserAccessResult result = new UserAccessResult
                (
                    accessToken: jwt,
                    refreshToken: signInToken.RefreshToken,
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



        /// <summary>
        /// RefreshAccessTokenAsync
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="CacheException"></exception>
        public async Task<UserAccessResult> RefreshAccessTokenAsync(RefreshContext context, string lastUser)
        {
            ThrowIf.NotValid(context, nameof(context));

            //解决并发涌入
            using IDistributedLock distributedLock = await _lockManager.NoWaitLockAsync(
               nameof(RefreshAccessTokenAsync) + context.DeviceId,
               _options.RefreshIntervalTimeSpan, notUnlockWhenDispose: true).ConfigureAwait(false);

            if (!distributedLock.IsAcquired)
            {
                throw new IdentityException(IdentityErrorCode.AuthorizationTooFrequent, $"Context:{SerializeUtil.ToJson(context)}");
            }

            //AccessToken, Claims 验证

            ClaimsPrincipal? claimsPrincipal = null;

            try
            {
                claimsPrincipal = JwtHelper.ValidateTokenWithoutLifeCheck(context.AccessToken, _options.OpenIdConnectConfiguration.Issuer, _issuerSigningKeys, _decryptionSecurityKey);
            }
            catch (Exception ex)
            {
                throw new IdentityException(IdentityErrorCode.AuthorizationInvalideAccessToken, $"Context: {SerializeUtil.ToJson(context)}", ex);
            }

            //TODO: 这里缺DeviceId验证. 放在了StartupUtil.cs中

            if (claimsPrincipal == null)
            {
                //TODO: Black concern SigninToken by RefreshToken
                throw new IdentityException(IdentityErrorCode.AuthorizationInvalideAccessToken, $"Context: {SerializeUtil.ToJson(context)}");
            }

            if (claimsPrincipal.GetDeviceId() != context.DeviceId)
            {
                throw new IdentityException(IdentityErrorCode.AuthorizationInvalideDeviceId, $"Context: {SerializeUtil.ToJson(context)}");
            }

            long userId = claimsPrincipal.GetUserId().GetValueOrDefault();

            if (userId <= 0)
            {
                throw new IdentityException(IdentityErrorCode.AuthorizationInvalideUserId, $"Context: {SerializeUtil.ToJson(context)}");
            }

            //SignInToken 验证
            User? user;
            SignInToken? signInToken = null;
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                signInToken = await _signInTokenRepo.GetByConditionAsync(
                    claimsPrincipal.GetSignInTokenId().GetValueOrDefault(),
                    context.RefreshToken,
                    context.DeviceId,
                    userId,
                    transactionContext).ConfigureAwait(false);

                if (signInToken == null || signInToken.Blacked)
                {
                    throw new IdentityException(IdentityErrorCode.AuthorizationNoTokenInStore, $"Refresh token error. signInToken not saved in db. ");
                }

                //验证SignInToken过期问题

                if (signInToken.ExpireAt < TimeUtil.UtcNow)
                {
                    throw new IdentityException(IdentityErrorCode.AuthorizationRefreshTokenExpired, $"Refresh Token Expired.");
                }

                // User 信息变动验证

                user = await _userRepo.GetByIdAsync(userId, transactionContext).ConfigureAwait(false);

                if (user == null || user.SecurityStamp != claimsPrincipal.GetUserSecurityStamp())
                {
                    throw new IdentityException(IdentityErrorCode.AuthorizationUserSecurityStampChanged, $"Refresh token error. User SecurityStamp Changed.");
                }

                // 更新SignInToken
                signInToken.RefreshCount++;

                await _signInTokenRepo.UpdateAsync(signInToken, lastUser, transactionContext).ConfigureAwait(false);

                // 发布新的AccessToken

                string accessToken = await ConstructJwtAsync(user, signInToken, claimsPrincipal.GetAudience(), transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return new UserAccessResult(accessToken, context.RefreshToken, user);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                if (signInToken != null)
                {
                    await _signInTokenRepo.DeleteAsync(signInToken, lastUser, null).ConfigureAwait(false);
                }

                throw;
            }
        }

        /// <summary>
        /// SignOutAsync
        /// </summary>
        /// <param name="signInTokenId"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public async Task SignOutAsync(long signInTokenId, string lastUser)
        {
            ThrowIf.NotLongId(signInTokenId, nameof(signInTokenId));

            TransactionContext transContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                SignInToken? signInToken = await _signInTokenRepo.GetByIdAsync(signInTokenId, transContext).ConfigureAwait(false);

                if (signInToken != null)
                {
                    await _signInTokenRepo.DeleteAsync(signInToken, lastUser, transContext).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogWarning($"尝试删除不存在的SignInToken. SignInTokenId:{signInTokenId}");
                }

                await transContext.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// SignOutAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="idiom"></param>
        /// <param name="logOffType"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public async Task SignOutAsync(long userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser)
        {
            ThrowIf.NotLongId(userId, nameof(userId));

            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                await DeleteSignInTokensAsync(userId, idiom, logOffType, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// OnSignInFailedBySmsAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        /// <exception cref="KVStoreException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="CacheException"></exception>
        public async Task OnSignInFailedBySmsAsync(string mobile, string lastUser)
        {
            User? user = await _userRepo.GetByMobileAsync(mobile).ConfigureAwait(false);

            if (user == null)
            {
                return;
            }

            UserLoginControl userLoginControl = await GetOrCreateUserLoginControlAsync(lastUser, user.Id).ConfigureAwait(false);

            await OnSignInFailedAsync(userLoginControl, lastUser).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteSignInTokensAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="idiom"></param>
        /// <param name="logOffType"></param>
        /// <param name="lastUser"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        private async Task DeleteSignInTokensAsync(long userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser, TransactionContext transactionContext)
        {
            IEnumerable<SignInToken> resultList = await _signInTokenRepo.GetByUserIdAsync(userId, transactionContext).ConfigureAwait(false);

            IEnumerable<SignInToken> toDeletes = logOffType switch
            {
                LogOffType.LogOffAllOthers => resultList,
                LogOffType.LogOffAllButWeb => resultList.Where(s => s.DeviceIdiom != DeviceIdiom.Web),
                LogOffType.LogOffSameIdiom => resultList.Where(s => s.DeviceIdiom == idiom),
                _ => new List<SignInToken>()
            };

            await _signInTokenRepo.DeleteAsync(toDeletes, lastUser, transactionContext).ConfigureAwait(false);
        }

        /// <summary>
        /// ConstructJwtAsync
        /// </summary>
        /// <param name="user"></param>
        /// <param name="signInToken"></param>
        /// <param name="signToWhere"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="CacheException"></exception>
        private async Task<string> ConstructJwtAsync(User user, SignInToken signInToken, string? signToWhere, TransactionContext transactionContext)
        {
            IEnumerable<Role> roles = await _roleOfUserRepo.GetRolesByUserIdAsync(user.Id, transactionContext).ConfigureAwait(false);
            IEnumerable<UserClaim> userClaims = await _userClaimRepo.GetByUserIdAsync(user.Id, transactionContext).ConfigureAwait(false);

            IEnumerable<Claim> claims = ConstructClaims(user, roles, userClaims, signInToken);

            string jwt = JwtHelper.BuildJwt(
                claims,
                _options.OpenIdConnectConfiguration.Issuer,
                _options.NeedAudienceToBeChecked ? signToWhere : null,
                _options.SignInOptions.AccessTokenExpireTimeSpan,
                _signingCredentials,
                _encryptingCredentials);
            return jwt;
        }

        /// <summary>
        /// PreSignInCheck
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userLoginControl"></param>
        /// <param name="lastUser"></param>
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="KVStoreException"></exception>
        private async Task PreSignInCheckAsync(User user, UserLoginControl userLoginControl, string lastUser)
        {
            ThrowIf.Null(user, nameof(user));

            SignInOptions signInOptions = _options.SignInOptions;

            //2, 手机验证
            if (signInOptions.RequireMobileConfirmed && !user.MobileConfirmed)
            {
                throw new IdentityException(IdentityErrorCode.AuthorizationMobileNotConfirmed, $"用户手机需要通过验证. UserId:{user.Id}");
            }

            //3, 邮件验证
            if (signInOptions.RequireEmailConfirmed && !user.EmailConfirmed)
            {
                throw new IdentityException(IdentityErrorCode.AuthorizationEmailNotConfirmed, $"用户邮箱需要通过验证. UserId:{user.Id}");
            }

            //4, Lockout 检查
            if (signInOptions.RequiredLockoutCheck && userLoginControl.LockoutEnabled && userLoginControl.LockoutEndDate > TimeUtil.UtcNow)
            {
                throw new IdentityException(IdentityErrorCode.AuthorizationLockedOut, $"用户已经被锁定。LockoutEndDate:{userLoginControl.LockoutEndDate}, UserId:{user.Id}");
            }

            //5, 一段时间内,最大失败数检测
            if (signInOptions.RequiredMaxFailedCountCheck && userLoginControl.LoginFailedLastTime.HasValue)
            {
                if (TimeUtil.UtcNow - userLoginControl.LoginFailedLastTime < TimeSpan.FromDays(signInOptions.AccessFailedRecoveryDays))
                {
                    if (userLoginControl.LoginFailedCount > signInOptions.MaxFailedCount)
                    {
                        throw new IdentityException(IdentityErrorCode.AuthorizationOverMaxFailedCount, $"用户今日已经达到最大失败登陆数. UserId:{user.Id}");
                    }
                }
            }

            //重置LoginControl
            if (userLoginControl.LockoutEnabled || userLoginControl.LoginFailedCount != 0)
            {
                userLoginControl.LockoutEnabled = false;
                userLoginControl.LoginFailedCount = 0;

                await _userLoginControlRepo.UpdateAsync(userLoginControl, lastUser).ConfigureAwait(false);
            }

            if (signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }
        }

        /// <summary>
        /// OnSignInFailed
        /// </summary>
        /// <param name="userLoginControl"></param>
        /// <param name="lastUser"></param>
        /// <exception cref="KVStoreException"></exception>
        private async Task OnSignInFailedAsync(UserLoginControl userLoginControl, string lastUser)
        {
            if (_options.SignInOptions.RequiredLockoutCheck)
            {
                if (userLoginControl.LoginFailedCount > _options.SignInOptions.LockoutAfterAccessFailedCount)
                {
                    userLoginControl.LockoutEnabled = true;
                    userLoginControl.LockoutEndDate = TimeUtil.UtcNow + _options.SignInOptions.LockoutTimeSpan;

                    _logger.LogWarning($"有用户重复登陆失败，账户已锁定.UserId:{userLoginControl.UserId}, LastUser:{lastUser}");
                }
            }

            if (_options.SignInOptions.RequiredMaxFailedCountCheck)
            {
                userLoginControl.LoginFailedCount++;
            }

            await _userLoginControlRepo.UpdateAsync(userLoginControl, lastUser).ConfigureAwait(false);
        }

        /// <summary>
        /// InitializeCredencials
        /// </summary>
        /// <exception cref="IdentityException"></exception>
        private void InitializeCredencials()
        {
            //Initialize Jwt Signing Credentials
            X509Certificate2? cert = CertificateUtil.GetCertificateFromSubjectOrFile(
                _options.JwtSigningCertificateSubject,
                _options.JwtSigningCertificateFileName,
                _options.JwtSigningCertificateFilePassword);

            _signingCredentials = CredentialHelper.GetSigningCredentials(cert, _options.SigningAlgorithm);
            _jsonWebKeySetJson = CredentialHelper.CreateJsonWebKeySetJson(cert);
            _issuerSigningKeys = CredentialHelper.GetIssuerSigningKeys(cert);

            //Initialize Jwt Content Encrypt/Decrypt Credentials
            X509Certificate2 encryptionCert = CertificateUtil.GetCertificateFromSubjectOrFile(
                _options.JwtContentCertificateSubject,
                _options.JwtContentCertificateFileName,
                _options.JwtContentCertificateFilePassword);

            _encryptingCredentials = CredentialHelper.GetEncryptingCredentials(encryptionCert);
            _decryptionSecurityKey = CredentialHelper.GetSecurityKey(encryptionCert);
        }

        private static bool PassowrdCheck(User user, string password)
        {
            string passwordHash = SecurityUtil.EncryptPwdWithSalt(password, user.SecurityStamp);
            return passwordHash.Equals(user.PasswordHash, GlobalSettings.Comparison);
        }

        private static IEnumerable<Claim> ConstructClaims(User user, IEnumerable<Role> roles, IEnumerable<UserClaim> userClaims, SignInToken signInToken)
        {
            IList<Claim> claims = new List<Claim>
            {
                new Claim(ClaimExtensionTypes.UserId, user.Id.ToString(GlobalSettings.Culture)),
                new Claim(ClaimExtensionTypes.SecurityStamp, user.SecurityStamp),
                new Claim(ClaimExtensionTypes.LoginName, user.LoginName ?? ""),

                new Claim(ClaimExtensionTypes.SignInTokenId, signInToken.Id.ToString(GlobalSettings.Culture)),
                new Claim(ClaimExtensionTypes.DeviceId, signInToken.DeviceId),
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
            return claims;
        }

        /// <summary>
        /// GetOrCreateUserLoginControlAsync
        /// </summary>
        /// <param name="lastUser"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="KVStoreException"></exception>
        private async Task<UserLoginControl> GetOrCreateUserLoginControlAsync(string lastUser, long userId)
        {
            UserLoginControl? userLoginControl = await _userLoginControlRepo.GetAsync(userId).ConfigureAwait(false);

            if (userLoginControl == null)
            {
                userLoginControl = new UserLoginControl { UserId = userId };
                await _userLoginControlRepo.AddAsync(userLoginControl, lastUser).ConfigureAwait(false);
            }

            return userLoginControl;
        }
    }
}

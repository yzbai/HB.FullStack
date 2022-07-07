using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using HB.FullStack.Common.Extensions;
using HB.FullStack.Database;
using HB.FullStack.Identity.Models;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using static HB.FullStack.Identity.LengthConventions;

namespace HB.FullStack.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly IdentityOptions _options;
        private readonly ILogger _logger;
        private readonly ITransaction _transaction;
        private readonly IDistributedLockManager _lockManager;

        private readonly UserRepo _userRepo;
        private readonly SignInTokenRepo _signInTokenRepo;
        private readonly UserClaimRepo _userClaimRepo;
        private readonly LoginControlRepo _userLoginControlRepo;
        private readonly RoleRepo _roleRepo;
        private readonly UserActivityRepo _userActivityModelRepo;
        private readonly UserRoleRepo _userRoleRepo;

        //Jwt Signing
        private string _jsonWebKeySetJson = null!;

        private IEnumerable<SecurityKey> _issuerSigningKeys = null!;
        private SigningCredentials _signingCredentials = null!;

        //Jwt Content Encrypt
        private EncryptingCredentials _encryptingCredentials = null!;

        private SecurityKey _decryptionSecurityKey = null!;

        private readonly HashSet<string> _validAudiences = new HashSet<string>();

        public IdentityService(
            IOptions<IdentityOptions> options,
            ILogger<IdentityService> logger,
            ITransaction transaction,
            IDistributedLockManager lockManager,
            UserRepo userRepo,
            SignInTokenRepo signInTokenRepo,
            UserClaimRepo userClaimRepo,
            LoginControlRepo userLoginControlRepo,
            RoleRepo roleRepo,
            UserRoleRepo userRoleRepo,
            UserActivityRepo userActivityModelRepo)
        {
            _options = options.Value;
            _logger = logger;
            _transaction = transaction;
            _lockManager = lockManager;

            _userRepo = userRepo;
            _userClaimRepo = userClaimRepo;
            _userLoginControlRepo = userLoginControlRepo;
            _signInTokenRepo = signInTokenRepo;

            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;

            _userActivityModelRepo = userActivityModelRepo;

            InitializeCredencials();

            //ValidateAudiences
            if (_options.ValidAudiences != null)
            {
                foreach (string audience in _options.ValidAudiences)
                {
                    _validAudiences.Add(audience);
                }
            }
        }

        public string JsonWebKeySetJson => _jsonWebKeySetJson;

        public async Task<UserToken> SignInAsync(SignInContext context, string lastUser)
        {
            ThrowIf.NotValid(context, nameof(context));

            EnsureValidateAudience(context);

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
                    user = await CreateUserAsync(context.Mobile!, null, context.LoginName, context.Password, true, false, lastUser, transactionContext).ConfigureAwait(false);
                }

                if (user == null)
                {
                    throw IdentityExceptions.AuthorizationNotFound(signInContext: context);
                }

                LoginControl userLoginControl = await GetOrCreateUserLoginControlAsync(lastUser, user.Id).ConfigureAwait(false);

                //密码检查
                if (context.SignInType == SignInType.ByMobileAndPassword || context.SignInType == SignInType.ByLoginNameAndPassword)
                {
                    if (!PassowrdCheck(user, context.Password!))
                    {
                        await OnSignInFailedAsync(userLoginControl, lastUser).ConfigureAwait(false);

                        throw IdentityExceptions.AuthorizationPasswordWrong(signInContext: context);
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

                UserToken result = new UserToken
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

        private void EnsureValidateAudience(SignInContext context)
        {
            if (!_validAudiences.Contains(context.SignToWhere))
            {
                throw IdentityExceptions.AudienceNotFound(context);
            }
        }

        public async Task<UserToken> RefreshAccessTokenAsync(RefreshContext context, string lastUser)
        {
            ThrowIf.NotValid(context, nameof(context));

            //解决并发涌入
            using IDistributedLock distributedLock = await _lockManager.NoWaitLockAsync(
               nameof(RefreshAccessTokenAsync) + context.DeviceId,
               _options.RefreshIntervalTimeSpan, notUnlockWhenDispose: true).ConfigureAwait(false);

            if (!distributedLock.IsAcquired)
            {
                throw IdentityExceptions.AuthorizationTooFrequent(context: context);
            }

            //AccessToken, Claims 验证

            ClaimsPrincipal? claimsPrincipal = null;

            try
            {
                claimsPrincipal = JwtHelper.ValidateTokenWithoutLifeCheck(
                    context.AccessToken,
                    _options.OpenIdConnectConfiguration.Issuer,
                    _options.NeedAudienceToBeChecked,
                    _options.ValidAudiences,
                    _issuerSigningKeys,
                    _decryptionSecurityKey);
            }
            catch (Exception ex)
            {
                throw IdentityExceptions.AuthorizationInvalideAccessToken(context: context, innerException: ex);
            }

            if (claimsPrincipal == null)
            {
                //TODO: Black concern SigninToken by RefreshToken
                throw IdentityExceptions.AuthorizationInvalideAccessToken(context: context);
            }

            if (claimsPrincipal.GetDeviceId() != context.DeviceId)
            {
                throw IdentityExceptions.AuthorizationInvalideDeviceId(context: context);
            }

            Guid userId = claimsPrincipal.GetUserId().GetValueOrDefault();

            if (userId.IsEmpty())
            {
                throw IdentityExceptions.AuthorizationInvalideUserId(context: context);
            }

            //SignInToken 验证
            User? user;
            SignInToken? signInToken = null;
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                signInToken = await _signInTokenRepo.GetByIdAsync(claimsPrincipal.GetSignInTokenId().GetValueOrDefault(), transactionContext).ConfigureAwait(false);

                if (signInToken == null ||
                    signInToken.Blacked ||
                    signInToken.RefreshToken != context.RefreshToken ||
                    signInToken.DeviceId != context.DeviceId ||
                    signInToken.UserId != userId)
                {
                    throw IdentityExceptions.AuthorizationNoTokenInStore(cause: "Refresh token error. signInToken not saved in db. ");
                }

                //验证SignInToken过期问题,即RefreshToken是否过期

                if (signInToken.ExpireAt < TimeUtil.UtcNow)
                {
                    throw IdentityExceptions.AuthorizationRefreshTokenExpired();
                }

                // User 信息变动验证

                user = await _userRepo.GetByIdAsync(userId, transactionContext).ConfigureAwait(false);

                if (user == null || user.SecurityStamp != claimsPrincipal.GetUserSecurityStamp())
                {
                    throw IdentityExceptions.AuthorizationUserSecurityStampChanged(cause: "Refresh token error. User SecurityStamp Changed.");
                }

                // 更新SignInToken
                /*
                 * 在 OAuth 2.0 安全最佳实践中, 推荐 refresh_token 是一次性的, 什么意思呢? 
                 * 使用 refresh_token 获取 access_token 时, 同时会返回一个 新的 refresh_token, 之前的 refresh_token 就会失效, 
                 * 但是两个 refresh_token 的绝对过期时间是一样的, 所以不会存在 refresh_token 快过期就获取一个新的, 然后重复，永不过期的情况
                 */
                signInToken.RefreshCount++;
                signInToken.RefreshToken = SecurityUtil.CreateUniqueToken();

                await _signInTokenRepo.UpdateAsync(signInToken, lastUser, transactionContext).ConfigureAwait(false);

                // 发布新的AccessToken
                string accessToken = await ConstructJwtAsync(user, signInToken, claimsPrincipal.GetAudience()!, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return new UserToken(accessToken, signInToken.RefreshToken, user);
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

        public async Task SignOutAsync(Guid signInTokenId, string lastUser)
        {
            ThrowIf.Empty(ref signInTokenId, nameof(signInTokenId));

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
                    _logger.LogWarning("尝试删除不存在的SignInToken. {SignInTokenId}", signInTokenId);
                }

                await transContext.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task SignOutAsync(Guid userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser)
        {
            ThrowIf.Empty(ref userId, nameof(userId));

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

        public async Task OnSignInFailedBySmsAsync(string mobile, string lastUser)
        {
            User? user = await _userRepo.GetByMobileAsync(mobile).ConfigureAwait(false);

            if (user == null)
            {
                return;
            }

            LoginControl userLoginControl = await GetOrCreateUserLoginControlAsync(lastUser, user.Id).ConfigureAwait(false);

            await OnSignInFailedAsync(userLoginControl, lastUser).ConfigureAwait(false);
        }

        private async Task DeleteSignInTokensAsync(Guid userId, DeviceIdiom idiom, LogOffType logOffType, string lastUser, TransactionContext transactionContext)
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

        private async Task<string> ConstructJwtAsync(User user, SignInToken signInToken, string audience, TransactionContext transactionContext)
        {
            IEnumerable<Role> roles = await _roleRepo.GetByUserIdAsync(user.Id, transactionContext).ConfigureAwait(false);
            IEnumerable<UserClaim> userClaims = await _userClaimRepo.GetByUserIdAsync(user.Id, transactionContext).ConfigureAwait(false);

            IEnumerable<Claim> claims = ConstructClaims(user, roles, userClaims, signInToken);

            string jwt = JwtHelper.BuildJwt(
                claims,
                _options.OpenIdConnectConfiguration.Issuer,
                _options.NeedAudienceToBeChecked ? audience : null,
                _options.SignInOptions.AccessTokenExpireTimeSpan,
                _signingCredentials,
                _encryptingCredentials);
            return jwt;
        }

        private async Task PreSignInCheckAsync(User user, LoginControl userLoginControl, string lastUser)
        {
            ThrowIf.Null(user, nameof(user));

            SignInOptions signInOptions = _options.SignInOptions;

            //2, 手机验证
            if (signInOptions.RequireMobileConfirmed && !user.MobileConfirmed)
            {
                throw IdentityExceptions.AuthorizationMobileNotConfirmed(userId: user.Id);
            }

            //3, 邮件验证
            if (signInOptions.RequireEmailConfirmed && !user.EmailConfirmed)
            {
                throw IdentityExceptions.AuthorizationEmailNotConfirmed(userId: user.Id);
            }

            //4, Lockout 检查
            if (signInOptions.RequiredLockoutCheck && userLoginControl.LockoutEnabled && userLoginControl.LockoutEndDate > TimeUtil.UtcNow)
            {
                throw IdentityExceptions.AuthorizationLockedOut(lockoutEndDate: userLoginControl.LockoutEndDate, userId: user.Id);
            }

            //5, 一段时间内,最大失败数检测
            if (signInOptions.RequiredMaxFailedCountCheck && userLoginControl.LoginFailedLastTime.HasValue)
            {
                if (TimeUtil.UtcNow - userLoginControl.LoginFailedLastTime < TimeSpan.FromDays(signInOptions.AccessFailedRecoveryDays))
                {
                    if (userLoginControl.LoginFailedCount > signInOptions.MaxFailedCount)
                    {
                        throw IdentityExceptions.AuthorizationOverMaxFailedCount(userId: user.Id);
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

        private async Task OnSignInFailedAsync(LoginControl userLoginControl, string lastUser)
        {
            if (_options.SignInOptions.RequiredLockoutCheck)
            {
                if (userLoginControl.LoginFailedCount > _options.SignInOptions.LockoutAfterAccessFailedCount)
                {
                    userLoginControl.LockoutEnabled = true;
                    userLoginControl.LockoutEndDate = TimeUtil.UtcNow + _options.SignInOptions.LockoutTimeSpan;

                    _logger.LogWarning("有用户重复登陆失败，账户已锁定.{UserId}, {LastUser}", userLoginControl.UserId, lastUser);
                }
            }

            if (_options.SignInOptions.RequiredMaxFailedCountCheck)
            {
                userLoginControl.LoginFailedCount++;
            }

            await _userLoginControlRepo.UpdateAsync(userLoginControl, lastUser).ConfigureAwait(false);
        }

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
                new Claim(ClaimExtensionTypes.USER_ID, user.Id.ToString()),
                new Claim(ClaimExtensionTypes.SECURITY_STAMP, user.SecurityStamp),
                //new Claim(ClaimExtensionTypes.LoginName, user.LoginName ?? ""),

                new Claim(ClaimExtensionTypes.SIGN_IN_TOKEN_ID, signInToken.Id.ToString()),
                new Claim(ClaimExtensionTypes.DEVICE_ID, signInToken.DeviceId),
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
                claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, item.Name));
            }
            return claims;
        }

        private async Task<LoginControl> GetOrCreateUserLoginControlAsync(string lastUser, Guid userId)
        {
            LoginControl? userLoginControl = await _userLoginControlRepo.GetAsync(userId).ConfigureAwait(false);

            if (userLoginControl == null)
            {
                userLoginControl = new LoginControl { UserId = userId };
                await _userLoginControlRepo.AddAsync(userLoginControl, lastUser).ConfigureAwait(false);
            }

            return userLoginControl;
        }

        private async Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext? transactionContext = null)
        {
            ThrowIf.NotMobile(mobile, nameof(mobile), true);
            ThrowIf.NotEmail(email, nameof(email), true);
            ThrowIf.NotLoginName(loginName, nameof(loginName), true);
            ThrowIf.NotPassword(password, nameof(password), true);

            if (mobile == null && email == null && loginName == null)
            {
                throw IdentityExceptions.IdentityMobileEmailLoginNameAllNull();
            }

            if (!mobileConfirmed && !emailConfirmed && password == null)
            {
                throw IdentityExceptions.IdentityNothingConfirmed();
            }

            bool ownTrans = transactionContext == null;

            TransactionContext transContext = transactionContext ?? await _transaction.BeginTransactionAsync<User>().ConfigureAwait(false);

            try
            {
                long count = await _userRepo.CountUserAsync(loginName, mobile, email, transContext).ConfigureAwait(false);

                if (count != 0)
                {
                    throw IdentityExceptions.IdentityAlreadyTaken(mobile: mobile, email: email, loginName: loginName);
                }

                User user = new User(loginName, mobile, email, password, mobileConfirmed, emailConfirmed);

                await _userRepo.AddAsync(user, lastUser, transContext).ConfigureAwait(false);

                if (ownTrans)
                {
                    await transContext.CommitAsync().ConfigureAwait(false);
                }

                return user;
            }
            catch
            {
                if (ownTrans)
                {
                    await transContext.RollbackAsync().ConfigureAwait(false);
                }

                throw;
            }
        }

        #region Role

        public async Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser)
        {
            //TODO: 需要重新构建 jwt

            ThrowIf.Empty(ref userId, nameof(userId));
            ThrowIf.Empty(ref roleId, nameof(roleId));

            TransactionContext trans = await _transaction.BeginTransactionAsync<UserRole>().ConfigureAwait(false);
            try
            {
                //查重
                IEnumerable<Role> storeds = await _roleRepo.GetByUserIdAsync(userId, trans).ConfigureAwait(false);

                if (storeds.Any(ur => ur.Id == roleId))
                {
                    throw IdentityExceptions.FoundTooMuch(userId: userId, roleId: roleId, cause: "已经有相同的角色");
                }

                UserRole ru = new UserRole(userId, roleId);

                await _userRoleRepo.AddAsync(ru, lastUser, trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<bool> TryRemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser)
        {
            //需要重新构建 jwt

            TransactionContext trans = await _transaction.BeginTransactionAsync<UserRole>().ConfigureAwait(false);

            try
            {
                //查重
                IEnumerable<Role> storeds = await _roleRepo.GetByUserIdAsync(userId, trans).ConfigureAwait(false);

                Role? stored = storeds.SingleOrDefault(ur => ur.Id == roleId);

                if (stored == null)
                {
                    return false;
                }

                UserRole? userRole = await _userRoleRepo.GetByUserIdAndRoleIdAsync(userId, roleId, trans).ConfigureAwait(false);

                await _userRoleRepo.DeleteAsync(userRole!, lastUser, trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);

                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                await trans.RollbackAsync().ConfigureAwait(false);

                _logger.LogTryRemoveRoleFromUserError(userId, roleId, lastUser, ex);

                return false;
            }
        }

        #endregion

        #region UserActivity

        public async Task RecordUserActivityAsync(Guid? signInTokenId, Guid? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode)
        {
            if (SerializeUtil.TryToJson(errorCode, out string? resultError))
            {
                if (resultError?.Length > MAX_RESULT_ERROR_LENGTH)
                {
                    _logger.LogWarning("记录UserActivity时，ErrorCode过长，已截断, {ErrorCode}", resultError);

                    resultError = resultError[..MAX_RESULT_ERROR_LENGTH];
                }
            }

            if (arguments != null && arguments.Length > MAX_ARGUMENTS_LENGTH)
            {
                _logger.LogWarning("记录UserActivity时，Arguments过长，已截断, {Arguments}", arguments);

                arguments = arguments[..MAX_ARGUMENTS_LENGTH];
            }

            if (url != null && url.Length > MAX_URL_LENGTH)
            {
                _logger.LogWarning("记录UserActivity时，url过长，已截断, {Url}", url);

                url = url[..MAX_URL_LENGTH];
            }

            UserActivity model = new UserActivity
            {
                SignInTokenId = signInTokenId,
                UserId = userId,
                Ip = ip,
                Url = url,
                HttpMethod = httpMethod,
                Arguments = arguments,
                ResultStatusCode = resultStatusCode,
                ResultType = resultType,
                ResultError = resultError
            };

            await _userActivityModelRepo.AddAsync(model, "", null).ConfigureAwait(false);
        }

        #endregion
    }
}
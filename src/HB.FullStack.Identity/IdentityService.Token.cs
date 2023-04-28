using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database;
using HB.FullStack.Server.Identity.Context;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using HB.FullStack.Server.Services;
using HB.FullStack.Server.Identity.Repos;

namespace HB.FullStack.Server.Identity
{
    internal partial class IdentityService : IIdentityService
    {
        private readonly IdentityOptions _options;
        private readonly ILogger _logger;
        private readonly ITransaction _transaction;
        private readonly IDistributedLockManager _lockManager;
        private readonly ISmsService _smsServerService;

        private readonly UserRepo _userRepo;
        private readonly UserProfileRepo _userProfileRepo;
        private readonly TokenCredentialRepo _signInCredentialRepo;
        private readonly UserClaimRepo _userClaimRepo;
        private readonly LoginControlRepo _userLoginControlRepo;
        private readonly RoleRepo _roleRepo;
        private readonly UserActivityRepo _userActivityModelRepo;

        private string _jwtJsonWebKeySet = null!;
        private SigningCredentials _jwtSigningCredentials = null!;
        private IEnumerable<SecurityKey> _jwtSigningKeys = null!;
        private EncryptingCredentials _jwtContentEncryptCredentials = null!;
        private SecurityKey _jwtContentDecryptionSecurityKey = null!;

        private readonly HashSet<string> _validAudiences = new HashSet<string>();

        public IdentityService(
            IOptions<IdentityOptions> options,
            ILogger<IdentityService> logger,
            ITransaction transaction,
            IDistributedLockManager lockManager,
            ISmsService smsServerService,
            UserRepo userRepo,
            UserProfileRepo userProfileRepo,
            TokenCredentialRepo signInCredentialRepo,
            UserClaimRepo userClaimRepo,
            LoginControlRepo userLoginControlRepo,
            RoleRepo roleRepo,
            UserActivityRepo userActivityModelRepo)
        {
            _options = options.Value;
            _logger = logger;
            _transaction = transaction;
            _lockManager = lockManager;
            _smsServerService = smsServerService;

            _userRepo = userRepo;
            _userProfileRepo = userProfileRepo;
            _userClaimRepo = userClaimRepo;
            _userLoginControlRepo = userLoginControlRepo;
            _signInCredentialRepo = signInCredentialRepo;
            _roleRepo = roleRepo;
            _userActivityModelRepo = userActivityModelRepo;

            InitJwtCredencials();
            InitValidAudiences();

            void InitJwtCredencials()
            {
                X509Certificate2? signingCert = CertificateUtil.GetCertificateFromSubjectOrFile(
                    _options.JwtSettings.JwtSigningCertificateSubject,
                    _options.JwtSettings.JwtSigningCertificateFileName,
                    _options.JwtSettings.JwtSigningCertificateFilePassword);

                _jwtJsonWebKeySet = CredentialHelper.CreateJsonWebKeySetJson(signingCert);

                _jwtSigningCredentials = CredentialHelper.GetSigningCredentials(signingCert, _options.JwtSettings.SigningAlgorithm);
                _jwtSigningKeys = CredentialHelper.GetIssuerSigningKeys(signingCert);

                X509Certificate2 contentEncryptionCert = CertificateUtil.GetCertificateFromSubjectOrFile(
                    _options.JwtSettings.JwtContentCertificateSubject,
                    _options.JwtSettings.JwtContentCertificateFileName,
                    _options.JwtSettings.JwtContentCertificateFilePassword);

                _jwtContentEncryptCredentials = CredentialHelper.GetEncryptingCredentials(contentEncryptionCert);
                _jwtContentDecryptionSecurityKey = CredentialHelper.GetSecurityKey(contentEncryptionCert);
            }

            void InitValidAudiences()
            {
                //ValidAudiences
                if (_options.JwtSettings.ValidAudiences != null)
                {
                    foreach (string audience in _options.JwtSettings.ValidAudiences)
                    {
                        _validAudiences.Add(audience);
                    }
                }
            }
        }

        public string JsonWebKeySet => _jwtJsonWebKeySet;

        public async Task<Token> GetTokenAsync(SignInContext context, string lastUser)
        {
            ThrowIf.NotValid(context, nameof(context));
            EnsureValidAudience(context);

            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<TokenCredential>().ConfigureAwait(false);

            try
            {
                User user = await EnsureUser(context, lastUser, transactionContext).ConfigureAwait(false);

                await EnsureTokenCheckAsync(context, user, lastUser).ConfigureAwait(false);

                await DeleteTokenCredentialsAsync(context.Exclusivity, user.Id, context.DeviceInfos.Idiom, context.DeviceInfos.Name, transactionContext).ConfigureAwait(false);

                //创建Credential
                TokenCredential tokenCredential = new TokenCredential
                (
                    userId: user.Id,
                    refreshToken: SecurityUtil.CreateUniqueToken(),
                    expireAt: TimeUtil.UtcNow + (context.RememberMe ? _options.SignInSettings.RefreshTokenLongExpireTimeSpan : _options.SignInSettings.RefreshTokenShortExpireTimeSpan),

                    clientId: context.ClientInfos.ClientId,
                    clientVersion: context.ClientInfos.ClientVersion,
                    clientIp: context.ClientInfos.ClientIp,

                    deviceName: context.DeviceInfos.Name,
                    deviceModel: context.DeviceInfos.Model,
                    deviceOSVersion: context.DeviceInfos.OSVersion,
                    devicePlatform: context.DeviceInfos.Platform,
                    deviceIdiom: context.DeviceInfos.Idiom,
                    deviceType: context.DeviceInfos.Type
                );

                await _signInCredentialRepo.AddAsync(tokenCredential, lastUser, transactionContext).ConfigureAwait(false);

                //构造 Jwt
                string accessToken = await ConstructAccessTokenAsync(user, tokenCredential, context.Audience, transactionContext).ConfigureAwait(false);

                Token signInReceipt = new Token(accessToken, tokenCredential.RefreshToken, user);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return signInReceipt;
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            async Task EnsureTokenCheckAsync(SignInContext context, User user, string lastUser)
            {
                LoginControl loginControl = await GetOrCreateUserLoginControlAsync(lastUser, user.Id).ConfigureAwait(false);

                //1, Password验证
                if (context is IHasPassword hasPassword)
                {
                    string passwordHash = SecurityUtil.EncryptPasswordWithSalt(hasPassword.Password, user.SecurityStamp);

                    if (!passwordHash.Equals(user.PasswordHash, Globals.Comparison))
                    {
                        await OnInvalidPasswordOrCodeAsync(loginControl, lastUser).ConfigureAwait(false);

                        throw IdentityExceptions.AuthorizationPasswordWrong(signInContext: context);
                    }
                }

                SignInSettings signInOptions = _options.SignInSettings;

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
                if (signInOptions.RequiredLockoutCheck && loginControl.LockoutEnabled && loginControl.LockoutEndDate > TimeUtil.UtcNow)
                {
                    throw IdentityExceptions.AuthorizationLockedOut(lockoutEndDate: loginControl.LockoutEndDate, userId: user.Id);
                }

                //5, 一段时间内,最大失败数检测
                if (signInOptions.RequiredMaxFailedCountCheck && loginControl.LoginFailedLastTime.HasValue)
                {
                    if (TimeUtil.UtcNow - loginControl.LoginFailedLastTime < TimeSpan.FromDays(signInOptions.AccessFailedRecoveryDays))
                    {
                        if (loginControl.LoginFailedCount > signInOptions.MaxFailedCount)
                        {
                            throw IdentityExceptions.AuthorizationOverMaxFailedCount(userId: user.Id);
                        }
                    }
                }

                //重置LoginControl
                if (loginControl.LockoutEnabled || loginControl.LoginFailedCount != 0)
                {
                    loginControl.LockoutEnabled = false;
                    loginControl.LoginFailedCount = 0;

                    await _userLoginControlRepo.UpdateAsync(loginControl, lastUser).ConfigureAwait(false);
                }

                if (signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
                {
                    //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
                }
            }

            async Task<User> EnsureUser(SignInContext context, string lastUser, TransactionContext transactionContext)
            {
                User? user = null;

                switch (context)
                {
                    case SignInByEmail signInByEmail:
                        user = await _userRepo.GetByEmailAsync(signInByEmail.Email, transactionContext).ConfigureAwait(false);
                        break;

                    case SignInByLoginName signInByLoginName:
                        user = await _userRepo.GetByLoginNameAsync(signInByLoginName.LoginName, transactionContext).ConfigureAwait(false);
                        break;

                    case SignInByMobile signInByMobile:
                        user = await _userRepo.GetByMobileAsync(signInByMobile.Mobile, transactionContext).ConfigureAwait(false);
                        break;

                    case SignInBySms signInBySms:

                        await EnsureValidSmsCode(signInBySms, lastUser).ConfigureAwait(false);

                        user = await _userRepo.GetByMobileAsync(signInBySms.Mobile, transactionContext).ConfigureAwait(false);

                        if (user == null && signInBySms.RegisterIfNotExists)
                        {
                            user = await CreateUserAsync(signInBySms.Mobile, null, null, null, true, false, lastUser, transactionContext).ConfigureAwait(false);
                        }
                        break;

                    default:
                        break;
                }

                return user ?? throw IdentityExceptions.IdentityUserNotExists(signInContext: context);
            }
        }

        public async Task<Token> RefreshTokenAsync(RefreshContext context, string lastUser)
        {
            ThrowIf.NotValid(context, nameof(context));

            //解决并发涌入：同一设备的在这么长时间内只能刷新一次。
            //如果刷新失败呢？那就重新登录吧

            using IDistributedLock distributedLock = await _lockManager.NoWaitLockAsync(
               nameof(RefreshTokenAsync) + context.ClientInfos.ClientId,
               _options.JwtSettings.RefreshIntervalTimeSpan,
               notUnlockWhenDispose: true).ConfigureAwait(false);

            if (!distributedLock.IsAcquired)
            {
                //直接短路。
                throw IdentityExceptions.SignInReceiptRefreshConcurrentError(context: context);
            }

            ClaimsPrincipal claimsPrincipal = EnsureClaimsPrincipal(context);

            Guid userId = claimsPrincipal.GetUserId() ?? throw IdentityExceptions.RefreshSignInReceiptError("UserId验证不通过", null, context);
            Guid signInCredentialId = claimsPrincipal.GetSignInCredentialId() ?? throw IdentityExceptions.RefreshSignInReceiptError("SignInCredentialId验证不通过", null, context);

            //TokenCredential 验证
            User? user;
            TokenCredential? signInCredential = null;
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<TokenCredential>().ConfigureAwait(false);

            try
            {
                signInCredential = await _signInCredentialRepo.GetByIdAsync(signInCredentialId, transactionContext).ConfigureAwait(false);

                if (signInCredential == null ||
                    signInCredential.Blacked ||
                    signInCredential.RefreshToken != context.RefreshToken ||
                    signInCredential.ClientId != context.ClientInfos.ClientId ||
                    signInCredential.UserId != userId)
                {
                    throw IdentityExceptions.RefreshSignInReceiptError("SignInCredential验证不通过", null, new { signInCredential?.Blacked, signInCredential?.ClientId, signInCredential?.UserId });
                }

                //验证SignInCredential过期问题,即RefreshToken是否过期
                if (signInCredential.ExpireAt < TimeUtil.UtcNow)
                {
                    throw IdentityExceptions.RefreshSignInReceiptError("SignInCredential过期", null, null);
                }

                // User 信息变动验证
                user = await _userRepo.GetByIdAsync(userId, transactionContext).ConfigureAwait(false);

                if (user == null || user.SecurityStamp != claimsPrincipal.GetUserSecurityStamp())
                {
                    throw IdentityExceptions.RefreshSignInReceiptError("用户SecurityStamp变动", null, null);
                }

                // 更新SignInCredential
                signInCredential.RefreshCount++;
                signInCredential.RefreshToken = SecurityUtil.CreateUniqueToken();

                await _signInCredentialRepo.UpdateAsync(signInCredential, lastUser, transactionContext).ConfigureAwait(false);

                // 发布新的AccessToken
                string accessToken = await ConstructAccessTokenAsync(user, signInCredential, claimsPrincipal.GetAudience()!, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return new Token(accessToken, signInCredential.RefreshToken, user);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                if (signInCredential != null)
                {
                    await _signInCredentialRepo.DeleteAsync(signInCredential, lastUser, null).ConfigureAwait(false);
                }

                throw;
            }

            ClaimsPrincipal EnsureClaimsPrincipal(RefreshContext context)
            {
                ClaimsPrincipal? claimsPrincipal;
                try
                {
                    claimsPrincipal = JwtHelper.ValidateTokenWithoutLifeCheck(
                        context.AccessToken,
                        _options.JwtSettings.OpenIdConnectConfiguration.Issuer,
                        _options.JwtSettings.NeedAudienceToBeChecked,
                        _options.JwtSettings.ValidAudiences,
                        _jwtSigningKeys,
                        _jwtContentDecryptionSecurityKey);
                }
                catch (Exception ex)
                {
                    throw IdentityExceptions.RefreshSignInReceiptError("验证过期的AccessToken时出错", ex, context);
                }

                if (claimsPrincipal == null)
                {
                    //TODO: Black concern TokenCredential by RefreshToken
                    throw IdentityExceptions.RefreshSignInReceiptError("验证过期的AccessToken时出错", null, context);
                }

                if (claimsPrincipal.GetClientId() != context.ClientInfos.ClientId)
                {
                    throw IdentityExceptions.RefreshSignInReceiptError("ClientId验证不通过", null, context);
                }

                return claimsPrincipal;
            }
        }

        public async Task DeleteTokenAsync(Guid signInCredentialId, string lastUser)
        {
            ThrowIf.Empty(ref signInCredentialId, nameof(signInCredentialId));

            TransactionContext transContext = await _transaction.BeginTransactionAsync<TokenCredential>().ConfigureAwait(false);

            try
            {
                TokenCredential? signInCredential = await _signInCredentialRepo.GetByIdAsync(signInCredentialId, transContext).ConfigureAwait(false);

                if (signInCredential != null)
                {
                    await _signInCredentialRepo.DeleteAsync(signInCredential, lastUser, transContext).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogWarning("尝试删除不存在的SignInCredential. {SignInCredentialId}", signInCredentialId);
                }

                await transContext.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task DeleteTokenAsync(Guid userId, DeviceIdiom idiom, SignInExclusivity logOffType, string lastUser)
        {
            ThrowIf.Empty(ref userId, nameof(userId));

            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<TokenCredential>().ConfigureAwait(false);

            try
            {
                await DeleteTokenCredentialsAsync(logOffType, userId, idiom, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        private void EnsureValidAudience(IHasAudience hasAudience)
        {
            if (!_validAudiences.Contains(hasAudience.Audience))
            {
                throw IdentityExceptions.AudienceNotFound(hasAudience);
            }
        }

        private async Task EnsureValidSmsCode(IBySmsCode context, string lastUser)
        {
            if (!await _smsServerService.ValidateAsync(context.Mobile, context.SmsCode).ConfigureAwait(false))
            {
                OnInvalidSmsCode(context, lastUser)
                    .SafeFireAndForget(ex => _logger.LogError(ex, "开火失败。InValidSmsCode: {Mobile}, {SmsCode}", context.Mobile, context.SmsCode));

                throw IdentityExceptions.IdentityInvalidSmsCode(context: context);
            }

            async Task OnInvalidSmsCode(IBySmsCode context, string lastUser)
            {
                User? user = await _userRepo.GetByMobileAsync(context.Mobile).ConfigureAwait(false);

                if (user != null)
                {
                    LoginControl loginControl = await GetOrCreateUserLoginControlAsync(lastUser, user.Id).ConfigureAwait(false);

                    await OnInvalidPasswordOrCodeAsync(loginControl, lastUser).ConfigureAwait(false);
                }
            }
        }

        private async Task DeleteTokenCredentialsAsync(SignInExclusivity logOffType, Guid userId, DeviceIdiom idiom, string lastUser, TransactionContext transactionContext)
        {
            IEnumerable<TokenCredential> resultList = await _signInCredentialRepo.GetByUserIdAsync(userId, transactionContext).ConfigureAwait(false);

            IEnumerable<TokenCredential> toDeletes = logOffType switch
            {
                SignInExclusivity.LogOffAllOthers => resultList,
                SignInExclusivity.LogOffAllButWeb => resultList.Where(s => s.DeviceIdiom != DeviceIdiom.Web),
                SignInExclusivity.LogOffSameIdiom => resultList.Where(s => s.DeviceIdiom == idiom),
                _ => new List<TokenCredential>()
            };

            await _signInCredentialRepo.DeleteAsync(toDeletes, lastUser, transactionContext).ConfigureAwait(false);
        }

        private async Task<string> ConstructAccessTokenAsync(User user, TokenCredential signInCredential, string audience, TransactionContext transactionContext)
        {
            IEnumerable<Claim> jwtClaims = await GetClaimsAsync(user, signInCredential).ConfigureAwait(false);

            string jwt = JwtHelper.CreateJwt(
                jwtClaims,
                _options.JwtSettings.OpenIdConnectConfiguration.Issuer,
                _options.JwtSettings.NeedAudienceToBeChecked ? audience : null,
                _options.SignInSettings.AccessTokenExpireTimeSpan,
                _jwtSigningCredentials,
                _jwtContentEncryptCredentials);

            return jwt;

            async Task<IEnumerable<Claim>> GetClaimsAsync(User user, TokenCredential signInCredential)
            {
                IEnumerable<Role> roles = await _userRepo.GetRolesByUserIdAsync(user.Id, transactionContext).ConfigureAwait(false);
                IEnumerable<UserClaim> userClaims = await _userClaimRepo.GetByUserIdAsync(user.Id, transactionContext).ConfigureAwait(false);

                IList<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimExtensionTypes.USER_ID, user.Id.ToString()),
                    new Claim(ClaimExtensionTypes.SECURITY_STAMP, user.SecurityStamp),
                    new Claim(ClaimExtensionTypes.SIGN_IN_CREDENTIAL_ID, signInCredential.Id.ToString()),
                    new Claim(ClaimExtensionTypes.CLIENT_ID, signInCredential.ClientId),
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
        }

        private async Task OnInvalidPasswordOrCodeAsync(LoginControl loginControl, string lastUser)
        {
            if (_options.SignInSettings.RequiredLockoutCheck)
            {
                if (loginControl.LoginFailedCount > _options.SignInSettings.LockoutAfterAccessFailedCount)
                {
                    loginControl.LockoutEnabled = true;
                    loginControl.LockoutEndDate = TimeUtil.UtcNow + _options.SignInSettings.LockoutTimeSpan;

                    _logger.LogWarning("有用户重复登陆失败，账户已锁定.{UserId}, {LastUser}", loginControl.UserId, lastUser);
                }
            }

            if (_options.SignInSettings.RequiredMaxFailedCountCheck)
            {
                loginControl.LoginFailedCount++;
            }

            await _userLoginControlRepo.UpdateAsync(loginControl, lastUser).ConfigureAwait(false);
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

        private async Task<User> CreateUserAsync(string? mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext? transactionContext = null)
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
    }
}
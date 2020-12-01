using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Identity.Entities;
using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HB.FullStack.Identity
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly AuthorizationServiceOptions _options;
        private readonly ITransaction _transaction;
        private readonly IDistributedLockManager _lockManager;

        private readonly UserRepo _userRepo;
        private readonly SignInTokenRepo _signInTokenRepo;
        private readonly RoleOfUserRepo _roleOfUserRepo;
        private readonly UserClaimRepo _userClaimRepo;
        private readonly UserLoginControlRepo _userLoginControlRepo;

        private readonly IIdentityService _identityService;

        //Jwt Signing
        private JsonWebKeySet _jsonWebKeySet = null!;
        private IEnumerable<SecurityKey> _issuerSigningKeys = null!;
        private SigningCredentials _signingCredentials = null!;


        //Jwt Content Encrypt
        private EncryptingCredentials _encryptingCredentials = null!;
        private SecurityKey _decryptionSecurityKey = null!;

        public AuthorizationService(IOptions<AuthorizationServiceOptions> options, ITransaction transaction, IDistributedLockManager lockManager,
            UserRepo userRepo,
            SignInTokenRepo signInTokenRepo,
            RoleOfUserRepo roleOfUserRepo,
            UserClaimRepo userClaimRepo,
            UserLoginControlRepo userLoginControlRepo,
            IIdentityService identityService)
        {
            _options = options.Value;
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

        public JsonWebKeySet JsonWebKeySet => _jsonWebKeySet;

        private void InitializeCredencials()
        {
            #region Initialize Jwt Signing Credentials

            X509Certificate2? cert = CertificateUtil.GetBySubject(_options.SigningCertificateSubject);

            if (cert == null)
            {
                throw new AuthorizationException(ErrorCode.JwtSigningCertNotFound, $"Subject:{_options.SigningCertificateSubject}");
            }

            _signingCredentials = CredentialHelper.GetSigningCredentials(cert, _options.SigningAlgorithm);
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

        public async Task SignOutAsync(string signInTokenGuid, string lastUser)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);
            try
            {
                await _signInTokenRepo.DeleteByGuidAsync(signInTokenGuid, lastUser, transactionContext).ConfigureAwait(false);

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
                await _signInTokenRepo.DeleteByLogOffTypeAsync(userGuid, idiom, logOffType, lastUser, transactionContext).ConfigureAwait(false);

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
                    SignInType.ByLoginNameAndPassword => await _userRepo.GetByLoginNameAsync(context.LoginName!, transactionContext).ConfigureAwait(false),
                    SignInType.BySms => await _userRepo.GetByMobileAsync(context.Mobile!, transactionContext).ConfigureAwait(false),
                    SignInType.ByMobileAndPassword => await _userRepo.GetByMobileAsync(context.Mobile!, transactionContext).ConfigureAwait(false),
                    _ => null
                };

                //不存在，则新建用户
                bool newUserCreated = false;

                if (user == null && context.SignInType == SignInType.BySms)
                {
                    user = await _userRepo.CreateAsync(
                        mobile: context.Mobile!,
                        email: null,
                        loginName: context.LoginName,
                        password: context.Password,
                        mobileConfirmed: true,
                        emailConfirmed: false,
                        lastUser: lastUser,
                        transactionContext).ConfigureAwait(false);

                    newUserCreated = true;
                }

                if (user == null)
                {
                    throw new AuthorizationException(ErrorCode.AuthorizationNotFound, $"SignInContext:{SerializeUtil.ToJson(context)}");
                }

                UserLoginControl userLoginControl = await _userLoginControlRepo.GetOrCreateByUserGuidAsync(user.Guid).ConfigureAwait(false);

                //密码检查
                if (context.SignInType == SignInType.ByMobileAndPassword || context.SignInType == SignInType.ByLoginNameAndPassword)
                {
                    if (!PassowrdCheck(user, context.Password!))
                    {
                        OnSignInFailed(userLoginControl, lastUser);

                        throw new AuthorizationException(ErrorCode.AuthorizationPasswordWrong, $"SignInContext:{SerializeUtil.ToJson(context)}");
                    }
                }

                //其他检查
                PreSignInCheck(user, userLoginControl, lastUser);

                //注销其他客户端
                await _signInTokenRepo.DeleteByLogOffTypeAsync(user.Guid, context.DeviceInfos.Idiom, context.LogOffType, context.DeviceInfos.Name, transactionContext).ConfigureAwait(false);

                //创建Token
                SignInToken signInToken = await _signInTokenRepo.CreateAsync(
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

                string jwt = await ConstructJwtAsync(user, signInToken, context.SignToWhere, transactionContext).ConfigureAwait(false);

                SignInResult result = new SignInResult
                (
                    accessToken: jwt,
                    refreshToken: signInToken.RefreshToken,
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
                claimsPrincipal = JwtHelper.ValidateTokenWithoutLifeCheck(context.AccessToken, _options.OpenIdConnectConfiguration.Issuer, _issuerSigningKeys, _decryptionSecurityKey);
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
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);

            try
            {
                signInToken = await _signInTokenRepo.GetByConditionAsync(
                    claimsPrincipal.GetSignInTokenGuid(),
                    context.RefreshToken,
                    context.DeviceId,
                    userGuid,
                    transactionContext).ConfigureAwait(false);

                if (signInToken == null || signInToken.Blacked)
                {
                    throw new AuthorizationException(ErrorCode.AuthorizationNoTokenInStore, $"Refresh token error. signInToken not saved in db. ");
                }

                //验证SignInToken过期问题

                if (signInToken.ExpireAt < DateTimeOffset.UtcNow)
                {
                    await BlackSignInTokenAsync(signInToken, lastUser).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationRefreshTokenExpired, $"Refresh Token Expired.");
                }

                // User 信息变动验证

                user = await _userRepo.GetByGuidAsync(userGuid, transactionContext).ConfigureAwait(false);

                if (user == null || user.SecurityStamp != claimsPrincipal.GetUserSecurityStamp())
                {
                    await BlackSignInTokenAsync(signInToken, lastUser).ConfigureAwait(false);

                    throw new AuthorizationException(ErrorCode.AuthorizationUserSecurityStampChanged, $"Refresh token error. User SecurityStamp Changed.");
                }

                // 更新SignInToken
                signInToken.RefreshCount++;

                await _signInTokenRepo.UpdateAsync(signInToken, lastUser, transactionContext).ConfigureAwait(false);

                // 发布新的AccessToken

                string jwt = await ConstructJwtAsync(user, signInToken, claimsPrincipal.GetAudience(), transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return jwt;
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

            UserLoginControl userLoginControl = await _userLoginControlRepo.GetOrCreateByUserGuidAsync(user.Guid).ConfigureAwait(false);

            OnSignInFailed(userLoginControl, lastUser);
        }

        private async Task<string> ConstructJwtAsync(User user, SignInToken signInToken, string? signToWhere, TransactionContext transactionContext)
        {
            IEnumerable<Role> roles = await _roleOfUserRepo.GetRolesByUserGuidAsync(user.Guid, transactionContext).ConfigureAwait(false);
            IEnumerable<UserClaim> userClaims = await _userClaimRepo.GetByUserGuidAsync(user.Guid, transactionContext).ConfigureAwait(false);

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

        private void PreSignInCheck(User user, UserLoginControl userLoginControl, string lastUser)
        {
            ThrowIf.Null(user, nameof(user));

            SignInOptions signInOptions = _options.SignInOptions;

            //2, 手机验证
            if (signInOptions.RequireMobileConfirmed && !user.MobileConfirmed)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationMobileNotConfirmed, $"user:{SerializeUtil.ToJson(user)}");
            }

            //3, 邮件验证
            if (signInOptions.RequireEmailConfirmed && !user.EmailConfirmed)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationEmailNotConfirmed, $"user:{SerializeUtil.ToJson(user)}");
            }

            //4, Lockout 检查
            if (signInOptions.RequiredLockoutCheck && userLoginControl.LockoutEnabled && userLoginControl.LockoutEndDate > DateTimeOffset.UtcNow)
            {
                throw new AuthorizationException(ErrorCode.AuthorizationLockedOut, $"user:{SerializeUtil.ToJson(user)}");
            }

            //5, 一段时间内,最大失败数检测
            if (signInOptions.RequiredMaxFailedCountCheck && userLoginControl.LoginFailedLastTime.HasValue)
            {
                if (DateTimeOffset.UtcNow - userLoginControl.LoginFailedLastTime < TimeSpan.FromDays(signInOptions.AccessFailedRecoveryDays))
                {
                    if (userLoginControl.LoginFailedCount > signInOptions.MaxFailedCount)
                    {
                        throw new AuthorizationException(ErrorCode.AuthorizationOverMaxFailedCount, $"user:{SerializeUtil.ToJson(user)}");
                    }
                }
            }

            userLoginControl.LockoutEnabled = false;
            userLoginControl.LoginFailedCount = 0;

            _userLoginControlRepo.UpdateAsync(userLoginControl, lastUser).Fire();

            if (signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }
        }

        private static bool PassowrdCheck(User user, string password)
        {
            string passwordHash = SecurityUtil.EncryptPwdWithSalt(password, user.Guid);
            return passwordHash.Equals(user.PasswordHash, GlobalSettings.Comparison);
        }

        private void OnSignInFailed(UserLoginControl userLoginControl, string lastUser)
        {
            if (_options.SignInOptions.RequiredLockoutCheck)
            {
                if (userLoginControl.LoginFailedCount + 1 > _options.SignInOptions.LockoutAfterAccessFailedCount)
                {
                    userLoginControl.LockoutEnabled = true;
                    userLoginControl.LockoutEndDate = DateTimeOffset.UtcNow + _options.SignInOptions.LockoutTimeSpan;
                }
            }

            if (_options.SignInOptions.RequiredMaxFailedCountCheck)
            {
                userLoginControl.LoginFailedCount++;
            }

            _userLoginControlRepo.UpdateAsync(userLoginControl, lastUser).Fire();
        }

        private async Task BlackSignInTokenAsync(SignInToken signInToken, string lastUser)
        {
            //TODO: 详细记录Black SiginInToken 的历史纪录
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<SignInToken>().ConfigureAwait(false);
            try
            {
                await _signInTokenRepo.DeleteByGuidAsync(signInToken.Guid, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);

                throw;
            }
        }

        private static IEnumerable<Claim> ConstructClaims(User user, IEnumerable<Role> roles, IEnumerable<UserClaim> userClaims, SignInToken signInToken)
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

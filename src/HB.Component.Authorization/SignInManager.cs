using HB.Framework.Common;
using System;
using HB.Component.Identity.Abstractions;
using Microsoft.Extensions.Options;
using HB.Component.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Authorization.Abstractions;
using HB.Component.Authorization.Entity;
using HB.Component.Identity.Entity;
using System.Security.Claims;

namespace HB.Component.Authorization
{
    public class SignInManager : ISignInManager
    {
        private AuthorizationServerOptions _options;
        private SignInOptions _signInOptions;
        private IUserBiz _userBiz;
        private ISignInTokenBiz _signInTokenBiz;
        private IJwtBuilder _jwtBuilder;

        public SignInManager(IOptions<AuthorizationServerOptions> options, ILogger<SignInManager> logger, ISignInTokenBiz signInTokenBiz, IUserBiz userBiz, IJwtBuilder jwtBuilder) 
        {
            _options = options.Value;
            _signInOptions = _options.SignInOptions;

            _userBiz = userBiz;
            _signInTokenBiz = signInTokenBiz;
            _jwtBuilder = jwtBuilder;
        }

        public async Task SignOutAsync(SignOutContext context)
        {
            string userTokenIdentifier = context.HttpContext.User.GetUserTokenIdentifier();

            await _signInTokenBiz.DeleteBySignInTokenIdentifierAsync(userTokenIdentifier).ConfigureAwait(false);
        }

        public async Task<SignInResult> SignInAsync(SignInContext context)
        {
            //TODO: 以后，可以考虑User.IsActivated逻辑

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

                user = await _userBiz.GetUserByMobileAsync(context.Mobile).ConfigureAwait(false);
            }
            else if (context.SignInType == SignInType.ByMobileAndPassword)
            {
                if (string.IsNullOrEmpty(context.Mobile) || string.IsNullOrEmpty(context.Password))
                {
                    return SignInResult.ArgumentError();
                }

                user = await _userBiz.GetUserByMobileAsync(context.Mobile).ConfigureAwait(false);
            }
            else if (context.SignInType == SignInType.ByUserNameAndPassword)
            {
                if (string.IsNullOrEmpty(context.UserName) || string.IsNullOrEmpty(context.Password))
                {
                    return SignInResult.ArgumentError();
                }

                user = await _userBiz.GetUserByUserNameAsync(context.UserName).ConfigureAwait(false);
            }

            #endregion

            #region New User 

            bool newUserCreated = false;

            if (user == null && context.SignInType == SignInType.BySms)
            {
                IdentityResult identityResult = await _userBiz.CreateUserByMobileAsync(context.UserType, context.Mobile, context.UserName, context.Password, true).ConfigureAwait(false);

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
                await _signInTokenBiz.DeleteAppClientTokenByUserIdAsync(user.Id).ConfigureAwait(false);
            }

            #endregion

            #region Create User Token

            SignInToken userToken = await _signInTokenBiz.CreateNewTokenAsync(
                user.Id, 
                context.ClientId, 
                context.ClientType.GetDescription(), 
                context.ClientVersion, 
                context.ClientAddress, 
                context.ClientIp, 
                context.RememberMe? _signInOptions.RefreshTokenLongExpireTimeSpan : _signInOptions.RefreshTokenShortExpireTimeSpan).ConfigureAwait(false);

            if (userToken == null)
            {
                return SignInResult.AuthtokenCreatedFailed();
            }

            #endregion

            #region Construct Jwt

            result.AccessToken = await _jwtBuilder.BuildJwtAsync(user, userToken, context.SignToWhere).ConfigureAwait(false);
            result.RefreshToken = userToken.RefreshToken;
            result.NewUserCreated = newUserCreated;
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
                await _userBiz.SetLockoutAsync(user.Id, false).ConfigureAwait(false);
            }

            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                await _userBiz.SetAccessFailedCountAsync(user.Id, 0).ConfigureAwait(false);
            }

            if (_signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }

            return SignInResult.Succeeded();
        }

        private static bool PassowrdCheck(User user, string password)
        {
            string passwordHash = SecurityHelper.EncryptPwdWithSalt(password, user.Guid);
            return passwordHash.Equals(user.PasswordHash, StringComparison.InvariantCulture);
        }

        private async Task OnPasswordCheckFailedAsync(User user)
        {
            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                await _userBiz.SetAccessFailedCountAsync(user.Id, user.AccessFailedCount + 1).ConfigureAwait(false);
            }

            if (_signInOptions.RequiredLockoutCheck)
            {
                if (user.AccessFailedCount + 1 > _signInOptions.LockoutAfterAccessFailedCount)
                {
                    await _userBiz.SetLockoutAsync(user.Id, true, _signInOptions.LockoutTimeSpan).ConfigureAwait(false);
                }
            }

            
        }
    }
}

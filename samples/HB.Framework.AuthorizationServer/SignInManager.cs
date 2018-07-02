using HB.Framework.Business;
using HB.Framework.Common;
using HB.Framework.Common.Validate;
using HB.Framework.Database;
using HB.Framework.KVStore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using HB.Framework.Identity.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Authentication;
using HB.Framework.AuthorizationServer.Abstractions;
using HB.Framework.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace HB.Framework.AuthorizationServer
{
    public class SignInManager : BaseBiz, ISignInManager
    {
        private AuthorizationServerOptions _options;
        private SignInOptions _signInOptions;
        private IUserBiz _userBiz;
        private ISignInTokenBiz _signInTokenBiz;
        private IJwtBuilder _jwtBuilder;

        public SignInManager(IDatabase database, IKVStore kvstore, IDistributedCache cache, ILogger<SignInManager> logger, 
            IOptions<AuthorizationServerOptions> options, ISignInTokenBiz signInTokenBiz, IUserBiz userBiz, IJwtBuilder jwtBuilder) 
            : base(database, kvstore, cache, logger)
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
            await _signInTokenBiz.DeleteBySignInTokenIdentifierAsync(userTokenIdentifier);
        }

        public async Task<SignInResult> SignInAsync(SignInContext context)
        {
            //TODO: 以后，可以考虑User.IsActivated逻辑

            #region Retrieve User

            User user = null;

            if (!context.IsValid())
            {
                return SignInResult.ArgumentError;
            }

            if (context.SignInType == SignInType.BySms)
            {
                if (string.IsNullOrEmpty(context.Mobile))
                {
                    return SignInResult.ArgumentError;
                }

                user = await _userBiz.GetUserByMobileAsync(context.Mobile);
            }
            else if (context.SignInType == SignInType.ByMobileAndPassword)
            {
                if (string.IsNullOrEmpty(context.Mobile) || string.IsNullOrEmpty(context.Password))
                {
                    return SignInResult.ArgumentError;
                }

                user = await _userBiz.GetUserByMobileAsync(context.Mobile);
            }
            else if (context.SignInType == SignInType.ByUserNameAndPassword)
            {
                if (string.IsNullOrEmpty(context.UserName) || string.IsNullOrEmpty(context.Password))
                {
                    return SignInResult.ArgumentError;
                }

                user = await _userBiz.GetUserByUserNameAsync(context.UserName);
            }

            #endregion

            #region New User 

            bool newUserCreated = false;

            if (user == null && context.SignInType == SignInType.BySms)
            {
                IdentityResult identityResult = await _userBiz.CreateUserByMobileAsync(context.Mobile, context.UserName, context.Password, true);

                if (identityResult == IdentityResult.Failed)
                {
                    return SignInResult.NewUserCreateFailed;
                }
                else if (identityResult == IdentityResult.EmailAlreadyTaken)
                {
                    return SignInResult.NewUserCreateFailedEmailAlreadyTaken;
                }
                else if (identityResult == IdentityResult.MobileAlreadyTaken)
                {
                    return SignInResult.NewUserCreateFailedMobileAlreadyTaken;
                }
                else if (identityResult == IdentityResult.UserNameAlreadyTaken)
                {
                    return SignInResult.NewUserCreateFailedUserNameAlreadyTaken;
                }

                newUserCreated = true;

                user = identityResult.User;
            }

            if (user == null)
            {
                return SignInResult.NoSuchUser;
            }

            #endregion

            #region Password Check

            if (context.SignInType == SignInType.ByMobileAndPassword || context.SignInType == SignInType.ByUserNameAndPassword)
            {
                if (!PassowrdCheck(user, context.Password))
                {
                    await OnPasswordCheckFailedAsync(user);
                    return SignInResult.PasswordWrong;
                }
            }

            #endregion

            #region Pre Sign Check 

            SignInResult result = await PreSignInCheckAsync(user);

            if (result != SignInResult.Success)
            {
                return result;
            }

            #endregion

            #region Logoff App Client

            if (context.ClientType != ClientType.Web && _signInOptions.AllowOnlyOneAppClient)
            {
                await _signInTokenBiz.DeleteAppClientTokenByUserIdAsync(user.Id);
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
                context.RememberMe? _signInOptions.RefreshTokenLongExpireTimeSpan : _signInOptions.RefreshTokenShortExpireTimeSpan);

            if (userToken == null)
            {
                return SignInResult.AuthtokenCreatedFailed;
            }

            #endregion

            #region Construct Jwt

            result.AccessToken = await _jwtBuilder.BuildJwtAsync(user, userToken, context.SignToWhere);
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
                return SignInResult.NoSuchUser;
            }

            //2, 手机验证
            if (_signInOptions.RequireMobileConfirmed && !user.MobileConfirmed)
            {
                return SignInResult.MobileNotConfirmed;
            }

            //3, 邮件验证
            if (_signInOptions.RequireEmailConfirmed && !user.EmailConfirmed)
            {
                return SignInResult.EmailNotConfirmed;
            }

            //4, Lockout 检查
            if (_signInOptions.RequiredLockoutCheck && user.LockoutEnabled && user.LockoutEndDate > DateTimeOffset.UtcNow)
            {
                return SignInResult.LockedOut;
            }

            //5, 一天内,最大失败数检测
            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                if (DateTimeOffset.UtcNow - user.AccessFailedLastTime < TimeSpan.FromDays(_signInOptions.AccessFailedRecoveryDays))
                {
                    if (user.AccessFailedCount > _signInOptions.MaxFailedCount)
                    {
                        return SignInResult.OverMaxFailedCount;
                    }
                }
            }

            if (_signInOptions.RequiredLockoutCheck)
            {
                await _userBiz.SetLockoutAsync(user.Id, false);
            }

            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                await _userBiz.SetAccessFailedCountAsync(user.Id, 0);
            }

            if (_signInOptions.RequireTwoFactorCheck && user.TwoFactorEnabled)
            {
                //TODO: 后续加上twofactor验证. 即登录后,再验证手机或者邮箱
            }

            return SignInResult.Success;
        }

        private bool PassowrdCheck(User user, string password)
        {
            string passwordHash = SecurityHelper.EncryptPwdWithSalt(password, user.Guid);
            return passwordHash.Equals(user.PasswordHash);
        }

        private async Task OnPasswordCheckFailedAsync(User user)
        {
            if (_signInOptions.RequiredMaxFailedCountCheck)
            {
                await _userBiz.SetAccessFailedCountAsync(user.Id, user.AccessFailedCount + 1);
            }

            if (_signInOptions.RequiredLockoutCheck)
            {
                if (user.AccessFailedCount + 1 > _signInOptions.LockoutAfterAccessFailedCount)
                {
                    await _userBiz.SetLockoutAsync(user.Id, true, _signInOptions.LockoutTimeSpan);
                }
            }

            
        }
    }
}

using HB.Component.Authorization.Abstractions;
using HB.Component.Identity.Entities;

using Microsoft.IdentityModel.Tokens;

using System;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    public interface IAuthorizationService
    {
        Task<string> RefreshAccessTokenAsync<TUser, TUserClaim, TRole, TRoleOfUser>(RefreshContext context, string lastUser)
            where TUser : User, new()
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new();

        Task<SignInResult> SignInAsync<TUser, TUserClaim, TRole, TRoleOfUser>(SignInContext context, string lastUser)
            where TUser : User, new()
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signInTokenGuid"></param>
        /// <returns></returns>
        Task SignOutAsync(string signInTokenGuid, string lastUser);

        /// <exception cref="FileNotFoundException">证书文件不存在</exception>
        /// <exception cref="ArgumentException">Json无法解析</exception>
        JsonWebKeySet GetJsonWebKeySet();
        Task SignOutAsync(string userGuid, DeviceIdiom idiom, LogOffType logOffType, string lastUser);
    }
}
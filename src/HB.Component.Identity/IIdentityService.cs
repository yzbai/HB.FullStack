using HB.Component.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public interface IIdentityService
    {
        Task<TUser?> GetUserBySecurityStampAsync<TUser>(string userGuid, string? securityStamp) where TUser : User, new();
        Task<TUser?> GetUserByMobileAsync<TUser>(string mobile) where TUser : User, new();
        Task<TUser?> GetUserByLoginNameAsync<TUser>(string loginName) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task<TUser> CreateUserByMobileAsync<TUser>(string mobile, string? loginName, string? password, bool mobileConfirmed, string lastUser) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task SetLockoutAsync<TUser>(string userGuid, bool lockout, string lastUser, TimeSpan? lockoutTimeSpan = null) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task SetAccessFailedCountAsync<TUser>(string userGuid, long count, string lastUser) where TUser : User, new();

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<Claim>> GetUserClaimAsync<TUserClaim, TRole, TRoleOfUser>(User user)
            where TUserClaim : UserClaim, new()
            where TRole : Role, new()
            where TRoleOfUser : RoleOfUser, new();
    }
}

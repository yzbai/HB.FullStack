using HB.Component.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public interface IIdentityService
    {
        Task<TUser?> GetUserBySecurityStampAsync<TUser>(string userGuid, string? securityStamp) where TUser : IdentityUser, new();
        Task<TUser?> GetUserByMobileAsync<TUser>(string mobile) where TUser : IdentityUser, new();
        Task<TUser?> GetUserByLoginNameAsync<TUser>(string loginName) where TUser : IdentityUser, new();

        /// <exception cref="DatabaseException"></exception>
        Task<TUser> CreateUserByMobileAsync<TUser>(string mobile, string? loginName, string? password, bool mobileConfirmed, string lastUser) where TUser : IdentityUser, new();

        /// <exception cref="DatabaseException"></exception>
        Task SetLockoutAsync<TUser>(string userGuid, bool lockout, string lastUser, TimeSpan? lockoutTimeSpan = null) where TUser : IdentityUser, new();

        /// <exception cref="DatabaseException"></exception>
        Task SetAccessFailedCountAsync<TUser>(string userGuid, long count, string lastUser) where TUser : IdentityUser, new();

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<Claim>> GetUserClaimAsync<TUserClaim, TRole, TRoleOfUser>(IdentityUser user)
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new();
    }
}

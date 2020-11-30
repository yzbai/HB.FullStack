using HB.Component.Identity.Entities;
using HB.FullStack.Database;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public interface IIdentityService
    {
        Task<User?> GetUserByMobileAsync(string mobile);

        Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser);

        Task SetLockoutAsync(string userGuid, bool lockout, string lastUser, TimeSpan? lockoutTimeSpan = null);

        Task SetAccessFailedCountAsync(string userGuid, long count, string lastUser);

        Task<User?> GetUserByUserGuidAsync(string userGuid);

        Task<IEnumerable<Claim>> CreateUserClaimAsync(User user);
    }
}

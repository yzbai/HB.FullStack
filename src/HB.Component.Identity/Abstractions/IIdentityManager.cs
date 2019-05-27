using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Component.Identity.Entity;

namespace HB.Component.Identity
{
    public interface IIdentityManager
    {
        Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp);
        Task<User> GetUserByMobileAsync(string mobile);
        Task<User> GetUserByUserNameAsync(string userName);
        Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed);
        Task<IdentityResult> SetLockoutAsync(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null);
        Task<IdentityResult> SetAccessFailedCountAsync(string userGuid, long count);
    }
}

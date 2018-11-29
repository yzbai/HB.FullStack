using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Component.Identity.Entity;

namespace HB.Component.Identity.Abstractions
{
    public interface IIdentityManager
    {
        Task<User> ValidateSecurityStampAsync(long userId, string securityStamp);
        Task<User> GetUserByMobileAsync(string mobile);
        Task<User> GetUserByUserNameAsync(string userName);
        Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed);
        Task SetLockoutAsync(long userId, bool lockout, TimeSpan? lockoutTimeSpan = null);
        Task SetAccessFailedCountAsync(long userId, long count);
    }
}

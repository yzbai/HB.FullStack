using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public class IdentityManager : IIdentityManager
    {
        public IdentityManager()
        {

        }

        public Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByMobileAsync(string mobile)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByUserNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public Task SetAccessFailedCountAsync(long userId, long count)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutAsync(long userId, bool lockout, TimeSpan? lockoutTimeSpan = null)
        {
            throw new NotImplementedException();
        }

        public Task<User> ValidateSecurityStampAsync(long userId, string securityStamp)
        {
            throw new NotImplementedException();
        }
    }
}

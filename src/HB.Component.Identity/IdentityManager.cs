using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    public class IdentityManager : IIdentityManager
    {
        private readonly IdentityOptions _options;
        private readonly IUserBiz _userBiz;

        public IdentityManager(IOptions<IdentityOptions> options, IUserBiz userBiz)
        {
            _options = options.Value;
            _userBiz = userBiz;
        }

        public Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed)
        {
            return _userBiz.CreateUserByMobileAsync(userType, mobile, userName, password, mobileConfirmed);
        }

        public Task<User> GetUserByMobileAsync(string mobile)
        {
            return _userBiz.GetUserByMobileAsync(mobile);
        }

        public Task<User> GetUserByUserNameAsync(string userName)
        {
            return _userBiz.GetUserByUserNameAsync(userName);
        }

        public Task<IdentityResult> SetAccessFailedCountAsync(long userId, long count)
        {
            return _userBiz.SetAccessFailedCountAsync(userId, count);
        }

        public Task<IdentityResult> SetLockoutAsync(long userId, bool lockout, TimeSpan? lockoutTimeSpan = null)
        {
            return _userBiz.SetLockoutAsync(userId, lockout, lockoutTimeSpan);
        }

        public Task<User> ValidateSecurityStampAsync(long userId, string securityStamp)
        {
            return _userBiz.ValidateSecurityStampAsync(userId, securityStamp);
        }
    }
}

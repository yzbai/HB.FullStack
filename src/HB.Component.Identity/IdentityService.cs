using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database.Transaction;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class IdentityService : IIdentityService
    {
        private readonly IdentityOptions _options;
        private readonly IUserBiz _userBiz;

        public IdentityService(IOptions<IdentityOptions> options, IUserBiz userBiz)
        {
            _options = options.Value;
            _userBiz = userBiz;
        }

        public Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed)
        {
            TransactionContext transactionContext = 
            return _userBiz.CreateByMobileAsync(userType, mobile, userName, password, mobileConfirmed);
        }

        public Task<User> GetUserByMobileAsync(string mobile)
        {
            return _userBiz.GetByMobileAsync(mobile);
        }

        public Task<User> GetUserByUserNameAsync(string userName)
        {
            return _userBiz.GetByUserNameAsync(userName);
        }

        public Task<IdentityResult> SetAccessFailedCountAsync(string userGuid, long count)
        {
            return _userBiz.SetAccessFailedCountAsync(userGuid, count);
        }

        public Task<IdentityResult> SetLockoutAsync(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null)
        {
            return _userBiz.SetLockoutAsync(userGuid, lockout, lockoutTimeSpan);
        }

        public Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp)
        {
            return _userBiz.ValidateSecurityStampAsync(userGuid, securityStamp);
        }
    }
}

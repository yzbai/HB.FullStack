using HB.Component.Identity.Entity;
using HB.Framework.Database;
using HB.Framework.Database.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    internal interface IUserBiz
    {
        Task<IdentityResult> CreateByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed, TransactionContext transContext);
        Task<IList<User>> GetAsync(IEnumerable<string> userGuids, TransactionContext transContext = null);
        Task<User> GetAsync(string userGuid, TransactionContext transContext = null);
        Task<User> GetByEmailAsync(string email, TransactionContext transContext = null);
        Task<User> GetByMobileAsync(string mobile, TransactionContext transContext = null);
        Task<User> GetByUserNameAsync(string userName, TransactionContext transContext = null);
        Task<IdentityResult> SetAccessFailedCountAsync(string userGuid, long count, TransactionContext transContext);
        Task<IdentityResult> SetLockoutAsync(string userGuid, bool lockout, TransactionContext transContext, TimeSpan? lockoutTimeSpan = null);
        Task<IdentityResult> SetPasswordByMobileAsync(string mobile, string newPassword, TransactionContext transContext);
        Task<IdentityResult> SetUserNameAsync(string userGuid, string userName, TransactionContext transContext);
        Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp, TransactionContext transContext = null);
    }


}
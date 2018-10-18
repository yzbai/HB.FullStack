using HB.Component.Identity.Entity;
using HB.Framework.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    public interface IUserBiz
    {
        Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed, DbTransactionContext transContext = null);
        Task<long?> GetIdByUserNameAsync(string userName, DbTransactionContext transContext = null);
        Task<User> GetUserByEmailAsync(string email, DbTransactionContext transContext = null);
        Task<User> GetUserByIdAsync(long userId, DbTransactionContext transContext = null);
        Task<User> GetUserByGuidAsync(string guidStr, DbTransactionContext transContext = null);
        Task<User> GetUserByMobileAsync(string mobile, DbTransactionContext transContext = null);
        Task<User> GetUserByUserNameAsync(string userName, DbTransactionContext transContext = null);
        Task<IList<User>> GetUsersByIdsAsync(IEnumerable<long> userIds, DbTransactionContext transContext = null);
        Task<IdentityResult> SetAccessFailedCountAsync(long userId, long count, DbTransactionContext transContext = null);
        Task<IdentityResult> SetLockoutAsync(long userId, bool lockout, TimeSpan? lockoutTimeSpan = null, DbTransactionContext transContext = null);
        Task<IdentityResult> SetUserNameAsync(long userId, string userName, DbTransactionContext transContext = null);
        Task<IdentityResult> SetUserPasswordByMobileAsync(string mobile, string newPassword, DbTransactionContext transContext = null);
        Task<User> ValidateSecurityStampAsync(long userId, string securityStamp, DbTransactionContext transContext = null);
    }


}
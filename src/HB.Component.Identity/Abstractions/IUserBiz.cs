using HB.Component.Identity.Entity;
using HB.Framework.Database;
using HB.Framework.Database.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HB.Component.Identity.Abstractions
{
    public interface IUserBiz
    {
        Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed, DatabaseTransactionContext transContext = null);
        Task<long?> GetIdByUserNameAsync(string userName, DatabaseTransactionContext transContext = null);
        Task<User> GetUserByEmailAsync(string email, DatabaseTransactionContext transContext = null);
        Task<User> GetUserByIdAsync(long userId, DatabaseTransactionContext transContext = null);
        Task<User> GetUserByGuidAsync(string guidStr, DatabaseTransactionContext transContext = null);
        Task<User> GetUserByMobileAsync(string mobile, DatabaseTransactionContext transContext = null);
        Task<User> GetUserByUserNameAsync(string userName, DatabaseTransactionContext transContext = null);
        Task<IList<User>> GetUsersByIdsAsync(IEnumerable<long> userIds, DatabaseTransactionContext transContext = null);
        Task<IdentityResult> SetAccessFailedCountAsync(long userId, long count, DatabaseTransactionContext transContext = null);
        Task<IdentityResult> SetLockoutAsync(long userId, bool lockout, TimeSpan? lockoutTimeSpan = null, DatabaseTransactionContext transContext = null);
        Task<IdentityResult> SetUserNameAsync(long userId, string userName, DatabaseTransactionContext transContext = null);
        Task<IdentityResult> SetUserPasswordByMobileAsync(string mobile, string newPassword, DatabaseTransactionContext transContext = null);
        Task<User> ValidateSecurityStampAsync(long userId, string securityStamp, DatabaseTransactionContext transContext = null);
    }


}
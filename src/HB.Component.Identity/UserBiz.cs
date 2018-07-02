using HB.Framework.Common;
using HB.Framework.Common.Validate;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;

namespace HB.Component.Identity
{
    /// <summary>
    /// 重要改变（比如Password）后，一定要清空对应UserId的Authtoken
    /// </summary>
    public class UserBiz : BizWithDbTransaction, IUserBiz
    {
        private IDatabase _db;
        private IdentityOptions _identityOptions;
        private ILogger _logger;

        public UserBiz(IOptions<IdentityOptions> options, IDatabase database, ILogger<UserBiz> logger)
            : base(database)
        {
            _db = database;
            _identityOptions = options.Value;
            _logger = logger;
        }

        #region Retrieve

        public async Task<User> ValidateSecurityStampAsync(long userId, string securityStamp, DbTransactionContext transContext = null)
        {
            return await _db.ScalarAsync<User>(u => u.Id == userId && u.SecurityStamp == securityStamp, transContext);
        }
        public async Task<long?> GetIdByUserNameAsync(string userName, DbTransactionContext transContext = null)
        {
            if (!ValidationMethods.IsUserName(userName))
            {
                return null;
            }

            //TODO: add some cache

            User u = await _db.ScalarAsync<User>(uu => uu.UserName == userName, transContext);

            return u?.Id;
        }

        public Task<User> GetUserByIdAsync(long userId, DbTransactionContext transContext)
        {
            return _db.ScalarAsync<User>(userId, transContext);
        }

        public Task<User> GetUserByGuidAsync(string guid, DbTransactionContext transContext = null)
        {
            return _db.ScalarAsync<User>(u => u.Guid == guid, transContext);
        }

        public Task<User> GetUserByMobileAsync(string mobile, DbTransactionContext transContext)
        {
            if (!ValidationMethods.IsMobilePhone(mobile))
            {
                return null;
            }

            return _db.ScalarAsync<User>(u => u.Mobile == mobile, transContext);
        }

        public Task<User> GetUserByUserNameAsync(string userName, DbTransactionContext transContext)
        {
            if (!ValidationMethods.IsUserName(userName)) { return null; }

            return _db.ScalarAsync<User>(u => u.UserName == userName, transContext);
        }

        public Task<User> GetUserByEmailAsync(string email, DbTransactionContext transContext = null)
        {
            if (!ValidationMethods.IsEmail(email))
            {
                return null;
            }

            return _db.ScalarAsync<User>(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase), transContext);
        }

        public Task<IList<User>> GetUsersByIdsAsync(IEnumerable<long> userIds, DbTransactionContext transContext = null)
        {
            return _db.RetrieveAsync<User>(u => SQLUtility.In(u.Id, userIds), transContext);
        }

        #endregion

        #region Update

        public async Task<IdentityResult> SetLockoutAsync(long userId, bool lockout, TimeSpan? lockoutTimeSpan = null, DbTransactionContext transContext = null)
        {
            User user = await GetUserByIdAsync(userId, transContext);

            if (user == null)
            {
                return IdentityResult.NotFound();
            }

            user.LockoutEnabled = lockout;

            if (lockout)
            {
                user.LockoutEndDate = DateTimeOffset.UtcNow + (lockoutTimeSpan == null ?lockoutTimeSpan.Value:TimeSpan.FromDays(1));
            }

            return (await _db.UpdateAsync(user, transContext)).ToIdentityResult();
        }

        public async Task<IdentityResult> SetAccessFailedCountAsync(long userId, long count, DbTransactionContext transContext)
        {
            User user = await GetUserByIdAsync(userId, transContext);

            if (user == null)
            {
                return IdentityResult.NotFound();
            }

            if (count != 0)
            {
                user.AccessFailedLastTime = DateTime.UtcNow;
            }

            user.AccessFailedCount = count;

            return (await _db.UpdateAsync(user, transContext)).ToIdentityResult();
        }

        public async Task<IdentityResult> SetUserNameAsync(long userId, string userName, DbTransactionContext transContext)
        {
            User user = await GetUserByIdAsync(userId, transContext);

            if (user == null)
            {
                return IdentityResult.NotFound();
            }

            if (!user.UserName.Equals(userName) && 0 != await _db.CountAsync<User>(u => u.UserName == userName, transContext))
            {
                return IdentityResult.AlreadyExists();
            }

            user.UserName = userName;

            await ChangeSecurityStampAsync(user);
            //ResetUserAuthtoken(userId);

            return (await _db.UpdateAsync(user, transContext)).ToIdentityResult();
        }

        public async Task<IdentityResult> SetUserPasswordByMobileAsync(string mobile, string newPassword, DbTransactionContext transContext)
        {
            if (!ValidationMethods.IsMobilePhone(mobile) || !ValidationMethods.IsPassword(newPassword))
            {
                return IdentityResult.ArgumentError();
            }

            User user = await GetUserByMobileAsync(mobile, transContext);

            if (user == null)
            {
                return IdentityResult.NotFound();
            }
            user.PasswordHash = SecurityHelper.EncryptPwdWithSalt(newPassword, user.Guid);

            await ChangeSecurityStampAsync(user);

            return (await _db.UpdateAsync(user, transContext)).ToIdentityResult();
        }

        private async Task ChangeSecurityStampAsync(User user)
        {
            user.SecurityStamp = SecurityHelper.CreateUniqueToken();

            if (_identityOptions.Events != null)
            {
                IdentitySecurityStampChangeContext context = new IdentitySecurityStampChangeContext(user.Id);
                await _identityOptions.Events.SecurityStampChanged(context);
            }
        }

        #endregion

        #region Register

        private User InitNewUser(string mobile, string userName, string password)
        {
            User user = new User
            {
                Mobile = mobile,
                Guid = SecurityHelper.CreateUniqueToken(),
                SecurityStamp = SecurityHelper.CreateUniqueToken(),
                IsActivated = true,
                AccessFailedCount = 0,
                UserName = userName,
                TwoFactorEnabled = false,
                ImageUrl = string.Empty
            };

            user.PasswordHash = SecurityHelper.EncryptPwdWithSalt(password, user.Guid);

            return user;
        }

        public async Task<IdentityResult> CreateUserByMobileAsync(string mobile, string userName, string password, bool mobileConfirmed, DbTransactionContext transContext = null)
        {
            #region Argument Check

            if (!string.IsNullOrEmpty(mobile) && !ValidationMethods.IsMobilePhone(mobile))
            {
                _logger.LogDebug("In Mobile Check, Failed, Mobile :" + mobile);
                return IdentityResult.Failed();
            }

            if (!string.IsNullOrEmpty(userName) && !ValidationMethods.IsUserName(userName))
            {
                _logger.LogDebug("In UserName Check, Failed, UserName :" + userName);
                return IdentityResult.Failed();
            }

            #endregion

            #region Existense Check

            User user = await GetUserByMobileAsync(mobile, transContext);

            if (user != null)
            {
                return IdentityResult.MobileAlreadyTaken();
            }

            if (!string.IsNullOrEmpty(userName))
            {
                User tmpUser = await GetUserByUserNameAsync(userName, transContext);

                if (tmpUser != null)
                {
                    return IdentityResult.UserNameAlreadyTaken();
                }
            }

            #endregion

            user = InitNewUser(mobile, userName, password);
            user.MobileConfirmed = mobileConfirmed;

            IdentityResult result = user.IsValid() ? (await _db.AddAsync(user, transContext)).ToIdentityResult() : IdentityResult.ArgumentError();
       
            if (result.IsSucceeded())
            {
                result.User = user;

                return result;
            }

            _logger.LogDebug("In User Adding, Failed, User :" + DataConverter.ToJson(user));

            return result;
        }

        #endregion
    }
}

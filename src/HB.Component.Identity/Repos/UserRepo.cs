
using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;
using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.SQL;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// 所有的User这个Entity的增删改查都要经过这里
    /// </summary>
    internal class UserRepo : Repository<User>
    {
        private readonly IdentityOptions _identityOptions;
        private readonly IDatabaseReader _databaseReader;

        public UserRepo(IOptions<IdentityOptions> identityOptions, ILogger<UserRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager)
        {
            _identityOptions = identityOptions.Value;
            _databaseReader = databaseReader;

            EntityUpdating += (sender, args) =>
            {
                sender.SecurityStamp = SecurityUtil.CreateUniqueToken();
                return Task.CompletedTask;
            };
        }

        #region Read 所有的查询都要经过这里

        public async Task<User?> GetByIdAsync(long userId, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                dimensionKeyName: nameof(User.Id),
                dimensionKeyValue: userId.ToString(CultureInfo.InvariantCulture),
                dbRetrieve: db =>
                {
                    return db.ScalarAsync<User>(userId, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<long> userIds, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.Id),
                userIds,
                db =>
                {
                    return db.RetrieveAsync<User>(u => SqlStatement.In(u.Id, true, userIds), transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByMobileAsync(string mobile, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.Mobile),
                mobile,
                db =>
                {
                    return db.ScalarAsync<User>(u => u.Mobile == mobile, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByLoginNameAsync(string loginName, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.LoginName),
                loginName,
                db =>
                {
                    return db.ScalarAsync<User>(u => u.LoginName == loginName, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByEmailAsync(string email, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.Email),
                email,
                db =>
                {
                    return db.ScalarAsync<User>(u => u.Email == email, transContext);
                }).ConfigureAwait(false);
        }

        public Task<long> CountUserAsync(string? loginName, string? mobile, string? email, TransactionContext? transContext)
        {
            WhereExpression<User> where = _databaseReader.Where<User>(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            return _databaseReader.CountAsync(where, transContext);
        }

        #endregion

        #region Write

        public async Task<User> CreateAsync(string? mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NotMobile(mobile, nameof(mobile), true);
            ThrowIf.NotEmail(email, nameof(email), true);
            ThrowIf.NotLoginName(loginName, nameof(loginName), true);
            ThrowIf.NotPassword(password, nameof(password), true);

            #region 查重

            if (mobile == null && email == null && loginName == null)
            {
                throw new FrameworkException(ErrorCode.IdentityMobileEmailLoginNameAllNull);
            }

            if (!mobileConfirmed && !emailConfirmed && password == null)
            {
                throw new FrameworkException(ErrorCode.IdentityNothingConfirmed);
            }

            long count = await CountUserAsync(loginName, mobile, email, transContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw new IdentityException(ErrorCode.IdentityAlreadyTaken, $"userType:{typeof(User)}, mobile:{mobile}, email:{email}, loginName:{loginName}");
            }

            #endregion

            User user = new User
            {
                SecurityStamp = SecurityUtil.CreateUniqueToken(),
                LoginName = loginName,
                Mobile = mobile,
                Email = email,
                //PasswordHash = password == null ? null : SecurityUtil.EncryptPwdWithSalt(password, user.Guid),
                MobileConfirmed = mobileConfirmed,
                EmailConfirmed = emailConfirmed
            };

            user.PasswordHash = password == null ? null : SecurityUtil.EncryptPwdWithSalt(password, user.SecurityStamp);

            await AddAsync(user, lastUser, transContext).ConfigureAwait(false);

            return user;
        }
        #endregion
    }
}

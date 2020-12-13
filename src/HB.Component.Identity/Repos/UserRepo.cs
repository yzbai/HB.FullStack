﻿
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

        public async Task<User?> GetByGuidAsync(string userGuid, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                dimensionKeyName: nameof(User.Guid),
                dimensionKeyValue: userGuid,
                dbRetrieve: db =>
                {
                    return db.ScalarAsync<User>(u => u.Guid == userGuid, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<User>> GetByGuidsAsync(IEnumerable<string> userGuids, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.Guid),
                userGuids,
                db =>
                {
                    return db.RetrieveAsync<User>(u => SqlStatement.In(u.Guid, true, userGuids.ToArray()), transContext);
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
            WhereExpression<User> where = _databaseReader.Where<User>().Where(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            return _databaseReader.CountAsync(where, transContext);
        }

        #endregion

        #region Write

        //public async Task UpdateLoginNameAsync(string userGuid, string loginName, string lastUser, TransactionContext transContext)
        //{
        //    ThrowIf.NotLoginName(loginName, nameof(loginName), false);

        //    #region Existense Check

        //    long count = await CountUserByLoginNameAsync(loginName, transContext).ConfigureAwait(false);

        //    if (count != 0)
        //    {
        //        throw new IdentityException(ErrorCode.IdentityAlreadyExists, $"userGuid:{userGuid}, loginName:{loginName}");
        //    }

        //    #endregion

        //    User? user = await GetByGuidAsync(userGuid, transContext).ConfigureAwait(false);

        //    if (user == null)
        //    {
        //        throw new IdentityException(ErrorCode.IdentityNotFound, $"userGuid:{userGuid}");
        //    }

        //    user.LoginName = loginName;

        //    await UpdateAsync(user, lastUser, transContext).ConfigureAwait(false);
        //}

        //public async Task UpdatePasswordByMobileAsync(string mobile, string newPassword, string lastUser, TransactionContext transContext)
        //{
        //    ThrowIf.NotMobile(mobile, nameof(mobile), false);
        //    ThrowIf.NotPassword(mobile, nameof(newPassword), false);

        //    User? user = await GetByMobileAsync(mobile, transContext).ConfigureAwait(false);

        //    if (user == null)
        //    {
        //        throw new IdentityException(ErrorCode.IdentityNotFound, $"mobile:{mobile}");
        //    }

        //    user.PasswordHash = SecurityUtil.EncryptPwdWithSalt(newPassword, user.Guid);

        //    await UpdateAsync(user, lastUser, transContext).ConfigureAwait(false);

        //}

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

            user.PasswordHash = password == null ? null : SecurityUtil.EncryptPwdWithSalt(password, user.Guid);

            await AddAsync(user, lastUser, transContext).ConfigureAwait(false);

            return user;
        }
        #endregion
    }
}
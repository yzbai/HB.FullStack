using AsyncAwaitBestPractices;
using HB.Component.Identity.Entities;
using HB.Framework.Business;
using HB.Framework.Cache;
using HB.Framework.Common;
using HB.Framework.Common.Utility;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using HB.Framework.DistributedLock;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class UserBiz : EntityBaseBiz<User>
    {
        private readonly IdentityOptions _identityOptions;
        private readonly IDatabaseReader _db;

        public UserBiz(IOptions<IdentityOptions> identityOptions, ILogger<UserBiz> logger, IDatabaseReader database, ICache cache, IDistributedLockManager lockManager) : base(logger, cache, lockManager)
        {
            _identityOptions = identityOptions.Value;
            _db = database;

            EntityUpdating += (sender, args) =>
            {
                sender.SecurityStamp = SecurityUtil.CreateUniqueToken();
                return Task.CompletedTask;
            };
        }

        #region Read

        public async Task<User?> GetByGuidAsync(string userGuid, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                dimensionKeyName: nameof(User.Guid),
                dimensionKeyValue: userGuid,
                retrieve: () =>
                {
                    return _db.ScalarAsync<User>(u => u.Guid == userGuid, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<User>> GetByGuidsAsync(IEnumerable<string> userGuids, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.Guid),
                userGuids,
                () =>
                {
                    return _db.RetrieveAsync<User>(u => SQLUtil.In(u.Guid, true, userGuids.ToArray()), transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByMobileAsync(string mobile, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.Mobile),
                mobile,
                () =>
                {
                    return _db.ScalarAsync<User>(u => u.Mobile == mobile, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByLoginNameAsync(string loginName, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.LoginName),
                loginName,
                () =>
                {
                    return _db.ScalarAsync<User>(u => u.LoginName == loginName, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByEmailAsync(string email, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(User.Email),
                email,
                () =>
                {
                    return _db.ScalarAsync<User>(u => u.Email == email, transContext);
                }).ConfigureAwait(false);
        }

        #endregion

        #region Update

        public async Task UpdateLoginNameAsync(string userGuid, string loginName, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NotLoginName(loginName, nameof(loginName), false);

            # region Existense Check

            long count = await _db.CountAsync<User>(u => u.LoginName == loginName, transContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw new IdentityException(ErrorCode.IdentityAlreadyExists, $"userGuid:{userGuid}, loginName:{loginName}");
            }

            //if (_bloomFilter.Exists(bloomFilterName: _identityOptions.BloomFilterName, loginName))
            //{
            //    throw new IdentityException(ErrorCode.IdentityAlreadyExists, $"userGuid:{userGuid}, loginName:{loginName}");
            //}

            #endregion

            User? user = await GetByGuidAsync(userGuid, transContext).ConfigureAwait(false);

            try
            {
                if (user == null)
                {
                    throw new IdentityException(ErrorCode.IdentityNotFound, $"userGuid:{userGuid}");
                }

                string? oldLoginName = user.LoginName;

                user.LoginName = loginName;

                await _db.UpdateAsync(user, OnEntityUpdatingAsync, OnEntityUpdatedAsync, lastUser, transContext).ConfigureAwait(false);

                //update bloomFilter
                //_bloomFilter.Add(_identityOptions.BloomFilterName, loginName);
                //_bloomFilter.Delete(_identityOptions.BloomFilterName, oldLoginName);
            }
            catch
            {
                //有可能从cache中获取了旧数据，导致update失败
                await OnEntityUpdateFailedAsync(user).ConfigureAwait(false);
                throw;
            }
        }

        public async Task UpdatePasswordByMobileAsync(string mobile, string newPassword, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NotMobile(mobile, nameof(mobile), false);
            ThrowIf.NotPassword(mobile, nameof(newPassword), false);

            User? user = await GetByMobileAsync(mobile, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(ErrorCode.IdentityNotFound, $"mobile:{mobile}");
            }

            try
            {
                user.PasswordHash = SecurityUtil.EncryptPwdWithSalt(newPassword, user.Guid);

                await _db.UpdateAsync(user, OnEntityUpdatingAsync, OnEntityUpdatedAsync, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnEntityUpdateFailedAsync(user).ConfigureAwait(false);
                throw;
            }
        }

        #endregion

        #region Create

        public async Task<User> CreateAsync(string? mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext transContext)
        {
            ThrowIf.NotMobile(mobile, nameof(mobile), true);
            ThrowIf.NotEmail(email, nameof(email), true);
            ThrowIf.NotLoginName(loginName, nameof(loginName), true);
            ThrowIf.NotPassword(password, nameof(password), true);

            #region Existense Check

            if (mobile == null && email == null && loginName == null)
            {
                throw new FrameworkException(ErrorCode.IdentityMobileEmailLoginNameAllNull);
            }

            if (!mobileConfirmed && !emailConfirmed && password == null)
            {
                throw new FrameworkException(ErrorCode.IdentityNothingConfirmed);
            }

            //if (_bloomFilter.ExistAny(_identityOptions.BloomFilterName, new string?[] { mobile, email, loginName }))
            //{
            //    throw new IdentityException(ErrorCode.IdentityAlreadyTaken, $"userType:{typeof(TUser)}, mobile:{mobile}, email:{email}, loginName:{loginName}");
            //}

            WhereExpression<User> where = _db.Where<User>().Where(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            long count = await _db.CountAsync(where, transContext).ConfigureAwait(false);

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

            await _db.AddAsync(user, OnEntityAddingAsync, OnEntityAddedAsync, lastUser, transContext).ConfigureAwait(false);

            return user;
        }

        #endregion
    }
}

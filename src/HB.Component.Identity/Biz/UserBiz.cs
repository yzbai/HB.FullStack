using AsyncAwaitBestPractices;
using HB.Component.Identity.Entities;
using HB.Framework.Business;
using HB.Framework.Common;
using HB.Framework.Common.Utility;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    /// <summary>
    /// 重要改变（比如Password）后，一定要清空对应UserId的Authtoken
    /// </summary>
    internal class UserBiz : SingleEntityBaseBiz<User>
    {
        private readonly IdentityOptions _identityOptions;
        private readonly IDatabase _db;

        public UserBiz(IOptions<IdentityOptions> identityOptions, IDatabase database, IDistributedCache cache) : base(cache)
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
            return await CacheAsideAsync(() => {
                return _db.ScalarAsync<User>(u => u.Guid == userGuid, transContext);
            }, null, userGuid).ConfigureAwait(false);
        }

        public Task<IEnumerable<TUser>> GetByGuidAsync<TUser>(IEnumerable<string> userGuids, TransactionContext? transContext = null) where TUser : User, new()
        {
            //TODO:扩展DIstributedCache
            return _db.RetrieveAsync<TUser>(u => SQLUtil.In(u.Guid, true, userGuids.ToArray()), transContext);
        }

        public Task<TUser?> GetByMobileAsync<TUser>(string mobile, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.ScalarAsync<TUser>(u => u.Mobile == mobile, transContext);
        }

        public Task<TUser?> GetByLoginNameAsync<TUser>(string loginName, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.ScalarAsync<TUser>(u => u.LoginName == loginName, transContext);
        }

        public Task<TUser?> GetByEmailAsync<TUser>(string email, TransactionContext? transContext = null) where TUser : User, new()
        {
            return _db.ScalarAsync<TUser>(u => u.Email == email, transContext);
        }

        #endregion

        #region Update

        public async Task UpdateLoginNameAsync<TUser>(string userGuid, string loginName, string lastUser, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.NotLoginName(loginName, nameof(loginName), false);

            # region Existense Check

            long count = await _db.CountAsync<TUser>(u => u.LoginName == loginName, transContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw new IdentityException(ErrorCode.IdentityAlreadyExists, $"userGuid:{userGuid}, loginName:{loginName}");
            }

            //if (_bloomFilter.Exists(bloomFilterName: _identityOptions.BloomFilterName, loginName))
            //{
            //    throw new IdentityException(ErrorCode.IdentityAlreadyExists, $"userGuid:{userGuid}, loginName:{loginName}");
            //}

            #endregion

            TUser? user = await GetByGuidAsync<TUser>(userGuid, transContext).ConfigureAwait(false);

            try
            {
                if (user == null)
                {
                    throw new IdentityException(ErrorCode.IdentityNotFound, $"userGuid:{userGuid}");
                }

                string? oldLoginName = user.LoginName;

                user.LoginName = loginName;

                await _db.UpdateAsync(user, OnUserUpdatingAsync, OnUserUpdatedAsync, lastUser, transContext).ConfigureAwait(false);

                //update bloomFilter
                //_bloomFilter.Add(_identityOptions.BloomFilterName, loginName);
                //_bloomFilter.Delete(_identityOptions.BloomFilterName, oldLoginName);
            }
            catch
            {
                //有可能从cache中获取了旧数据，导致update失败
                await OnUserUpdateFailedAsync(user).ConfigureAwait(false);
                throw;
            }
        }

        public async Task UpdatePasswordByMobileAsync<TUser>(string mobile, string newPassword, string lastUser, TransactionContext transContext) where TUser : User, new()
        {
            ThrowIf.NotMobile(mobile, nameof(mobile), false);
            ThrowIf.NotPassword(mobile, nameof(newPassword), false);

            TUser? user = await GetByMobileAsync<TUser>(mobile, transContext).ConfigureAwait(false);

            if (user == null)
            {
                throw new IdentityException(ErrorCode.IdentityNotFound, $"mobile:{mobile}");
            }

            try
            {
                user.PasswordHash = SecurityUtil.EncryptPwdWithSalt(newPassword, user.Guid);

                await _db.UpdateAsync(user, OnUserUpdatingAsync, OnUserUpdatedAsync, lastUser, transContext).ConfigureAwait(false);
            }
            catch
            {
                await OnUserUpdateFailedAsync(user).ConfigureAwait(false);
                throw;
            }
        }

        #endregion

        #region Create

        public async Task<TUser> CreateAsync<TUser>(string? mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext transContext) where TUser : User, new()
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

            WhereExpression<TUser> where = _db.Where<TUser>().Where(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            long count = await _db.CountAsync(where, transContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw new IdentityException(ErrorCode.IdentityAlreadyTaken, $"userType:{typeof(TUser)}, mobile:{mobile}, email:{email}, loginName:{loginName}");
            }

            #endregion

            TUser user = new TUser
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

            await _db.AddAsync(user, OnUserAddingAsync, OnUserAddedAsync, lastUser, transContext).ConfigureAwait(false);

            return user;
        }

        #endregion
    }
}

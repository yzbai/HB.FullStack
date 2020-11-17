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
    internal class UserBiz : BaseBiz
    {
        private readonly WeakAsyncEventManager _asyncEventManager = new WeakAsyncEventManager();
        private readonly IdentityOptions _identityOptions;
        private readonly IDatabase _db;
        private readonly IDistributedCache _cache;
        //private readonly IBloomFilter _bloomFilter;

        private readonly DistributedCacheEntryOptions _distributedCacheEntryOptions;

        public UserBiz(IOptions<IdentityOptions> identityOptions, IDatabase database, IDistributedCache cache/*, IBloomFilter bloomFilter*/)
        {
            _identityOptions = identityOptions.Value;
            _db = database;
            _cache = cache;
            //_bloomFilter = bloomFilter;

            _distributedCacheEntryOptions = new DistributedCacheEntryOptions();
        }

        public event AsyncEventHandler<IdentityUser, EventArgs> UserUpdating
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IdentityUser, EventArgs> UserUpdated
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IdentityUser?, EventArgs> UserUpdateFailed
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IdentityUser, EventArgs> UserAdding
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        public event AsyncEventHandler<IdentityUser, EventArgs> UserAdded
        {
            add => _asyncEventManager.Add(value);
            remove => _asyncEventManager.Remove(value);
        }

        #region Read

        public async Task<TUser?> GetByGuidAsync<TUser>(string userGuid, TransactionContext? transContext = null) where TUser : IdentityUser, new()
        {
            (TUser? cached, bool exists) = await _cache.GetAsync<TUser>(userGuid).ConfigureAwait(false);

            if (exists)
            {
                return cached;
            }

            //TODO: Log Missed!

            TUser? fromDb = await _db.ScalarAsync<TUser>(u => u.Guid == userGuid, transContext).ConfigureAwait(false);

            _cache.SetAsync<TUser>(userGuid, fromDb, _distributedCacheEntryOptions).Fire();

            return fromDb;
        }

        public Task<IEnumerable<TUser>> GetByGuidAsync<TUser>(IEnumerable<string> userGuids, TransactionContext? transContext = null) where TUser : IdentityUser, new()
        {
            //TODO:扩展DIstributedCache
            return _db.RetrieveAsync<TUser>(u => SQLUtil.In(u.Guid, true, userGuids.ToArray()), transContext);
        }

        public Task<TUser?> GetByMobileAsync<TUser>(string mobile, TransactionContext? transContext = null) where TUser : IdentityUser, new()
        {
            return _db.ScalarAsync<TUser>(u => u.Mobile == mobile, transContext);
        }

        public Task<TUser?> GetByLoginNameAsync<TUser>(string loginName, TransactionContext? transContext = null) where TUser : IdentityUser, new()
        {
            return _db.ScalarAsync<TUser>(u => u.LoginName == loginName, transContext);
        }

        public Task<TUser?> GetByEmailAsync<TUser>(string email, TransactionContext? transContext = null) where TUser : IdentityUser, new()
        {
            return _db.ScalarAsync<TUser>(u => u.Email == email, transContext);
        }

        #endregion

        #region Update

        public async Task UpdateLoginNameAsync<TUser>(string userGuid, string loginName, string lastUser, TransactionContext transContext) where TUser : IdentityUser, new()
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

        public async Task UpdatePasswordByMobileAsync<TUser>(string mobile, string newPassword, string lastUser, TransactionContext transContext) where TUser : IdentityUser, new()
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

        public async Task<TUser> CreateAsync<TUser>(string? mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext transContext) where TUser : IdentityUser, new()
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

        #region Events

        private async Task OnUserUpdatingAsync<TUser>(TUser user) where TUser : IdentityUser, new()
        {
            //Inner Logic
            user.SecurityStamp = SecurityUtil.CreateUniqueToken();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(UserUpdating), user, EventArgs.Empty).ConfigureAwait(false);
        }

        private async Task OnUserUpdatedAsync<TUser>(TUser user) where TUser : IdentityUser, new()
        {
            //Cache
            _cache.SetAsync(user.Guid, user, _distributedCacheEntryOptions).Fire();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(UserUpdated), user, EventArgs.Empty).ConfigureAwait(false);
        }

        private async Task OnUserUpdateFailedAsync<TUser>(TUser? user) where TUser : IdentityUser, new()
        {
            //Cache
            if (user != null)
            {
                _cache.RemoveAsync(user.Guid).Fire();
            }

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(UserUpdateFailed), user, EventArgs.Empty).ConfigureAwait(false);
        }

        private async Task OnUserAddingAsync<TUser>(TUser user) where TUser : IdentityUser, new()
        {
            //events
            await _asyncEventManager.RaiseEventAsync(nameof(UserAdding), user, EventArgs.Empty).ConfigureAwait(false);
        }

        private async Task OnUserAddedAsync<TUser>(TUser user) where TUser : IdentityUser, new()
        {
            //BloomFilter
            //_bloomFilter.Add(_identityOptions.BloomFilterName, new string?[] { user.Mobile, user.Email, user.LoginName });

            //Cache
            _cache.SetAsync(user.Guid, user, _distributedCacheEntryOptions).Fire();

            //Events
            await _asyncEventManager.RaiseEventAsync(nameof(UserAdded), user, EventArgs.Empty).ConfigureAwait(false);
        }

        #endregion
    }
}

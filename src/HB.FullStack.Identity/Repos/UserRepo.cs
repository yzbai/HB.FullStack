
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
    internal class UserRepo : DbEntityRepository<User>
    {
        public UserRepo(ILogger<UserRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager)
        {
            EntityUpdating += (sender, args) =>
            {
                sender.SecurityStamp = SecurityUtil.CreateUniqueToken();
                return Task.CompletedTask;
            };
        }

        protected override Task InvalidateCacheItemsOnChanged(User sender, DatabaseWriteEventArgs args) => Task.CompletedTask;

        #region Read 所有的查询都要经过这里

        /// <summary>
        /// GetByIdAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<User?> GetByIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                dimensionKeyName: nameof(User.Id),
                dimensionKeyValue: userId.ToString(),
                dbRetrieve: db =>
                {
                    return db.ScalarAsync<User>(userId, transContext);
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// GetByIdsAsync
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// GetByMobileAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// GetByLoginNameAsync
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// GetByEmailAsync
        /// </summary>
        /// <param name="email"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
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

        /// <summary>
        /// CountUserAsync
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public Task<long> CountUserAsync(string? loginName, string? mobile, string? email, TransactionContext? transContext)
        {
            WhereExpression<User> where = DatabaseReader.Where<User>(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            return DatabaseReader.CountAsync(where, transContext);
        }

        #endregion
    }
}

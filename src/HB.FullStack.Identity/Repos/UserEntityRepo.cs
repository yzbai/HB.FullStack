
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
    internal class UserEntityRepo : DbEntityRepository<UserEntity>
    {
        private readonly IDatabaseReader _databaseReader;

        public UserEntityRepo(ILogger<UserEntityRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager)
        {
            _databaseReader = databaseReader;

            EntityUpdating += (sender, args) =>
            {
                sender.SecurityStamp = SecurityUtil.CreateUniqueToken();
                return Task.CompletedTask;
            };
        }

        #region Read 所有的查询都要经过这里

        /// <summary>
        /// GetByIdAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="transContext"></param>
        /// <returns></returns>
        /// <exception cref="CacheException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<UserEntity?> GetByIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                dimensionKeyName: nameof(UserEntity.Id),
                dimensionKeyValue: userId.ToString(),
                dbRetrieve: db =>
                {
                    return db.ScalarAsync<UserEntity>(userId, transContext);
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
        public async Task<IEnumerable<UserEntity>> GetByIdsAsync(IEnumerable<long> userIds, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(UserEntity.Id),
                userIds,
                db =>
                {
                    return db.RetrieveAsync<UserEntity>(u => SqlStatement.In(u.Id, true, userIds), transContext);
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
        public async Task<UserEntity?> GetByMobileAsync(string mobile, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(UserEntity.Mobile),
                mobile,
                db =>
                {
                    return db.ScalarAsync<UserEntity>(u => u.Mobile == mobile, transContext);
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
        public async Task<UserEntity?> GetByLoginNameAsync(string loginName, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(UserEntity.LoginName),
                loginName,
                db =>
                {
                    return db.ScalarAsync<UserEntity>(u => u.LoginName == loginName, transContext);
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
        public async Task<UserEntity?> GetByEmailAsync(string email, TransactionContext? transContext = null)
        {
            return await TryCacheAsideAsync(
                nameof(UserEntity.Email),
                email,
                db =>
                {
                    return db.ScalarAsync<UserEntity>(u => u.Email == email, transContext);
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
            WhereExpression<UserEntity> where = _databaseReader.Where<UserEntity>(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            return _databaseReader.CountAsync(where, transContext);
        }

        #endregion
    }
}

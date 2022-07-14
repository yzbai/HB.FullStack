using HB.FullStack.Identity.Models;
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
using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// 所有的User这个Model的增删改查都要经过这里
    /// 所有通过User来使用与User相关的关系表的，都经过这里
    /// </summary>
    public class UserRepo : ModelRepository<User>
    {
        public UserRepo(ILogger<UserRepo> logger, IDatabaseReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager)
        {
            ModelUpdating += (sender, args) =>
            {
                if (sender is IEnumerable<User> users)
                {
                    foreach (var user in users)
                    {
                        //NOTICE: SecurityStamp主要用来加盐，每次User实体改动，SecurityStamp改动提高安全性
                        //从另一个角度提供了类似Version/timestamp的作用
                        user.SecurityStamp = SecurityUtil.CreateUniqueToken();
                    }
                }

                return Task.CompletedTask;
            };
        }

        protected override Task InvalidateCacheItemsOnChanged(IEnumerable<DBModel> sender, DBChangedEventArgs args)
        {
            if (sender is IEnumerable<UserRole> userRoles)
            {
                foreach (UserRole userRole in userRoles)
                {
                    //User-Role发生变化，就Invalidate
                    //比如：为用户添加角色，删除角色
                    InvalidateCache(new CachedRolesByUserId(userRole.UserId));
                }
            }

            return Task.CompletedTask;
        }

        #region 主表 Read 所有的查询都要经过这里

        public async Task<User?> GetByIdAsync(Guid userId, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                keyName: nameof(User.Id),
                keyValue: userId.ToString(),
                dbRetrieve: db =>
                {
                    return db.ScalarAsync<User>(userId, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<long> userIds, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(User.Id),
                userIds,
                db =>
                {
                    return db.RetrieveAsync<User>(u => SqlStatement.In(u.Id, true, userIds), transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByMobileAsync(string mobile, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(User.Mobile),
                mobile,
                db =>
                {
                    return db.ScalarAsync<User>(u => u.Mobile == mobile, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByLoginNameAsync(string loginName, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(User.LoginName),
                loginName,
                db =>
                {
                    return db.ScalarAsync<User>(u => u.LoginName == loginName, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User?> GetByEmailAsync(string email, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(User.Email),
                email,
                db =>
                {
                    return db.ScalarAsync<User>(u => u.Email == email, transContext);
                }).ConfigureAwait(false);
        }

        public Task<long> CountUserAsync(string? loginName, string? mobile, string? email, TransactionContext? transContext)
        {
            WhereExpression<User> where = DbReader.Where<User>(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            return DbReader.CountAsync(where, transContext);
        }

        #endregion

        #region 关系表 User-Role CRUD

        private FromExpression<User>? _fromUserToRole;
        private FromExpression<User> FromUserToRole => _fromUserToRole ??= DbReader
                                                                            .From<User>()
                                                                            .LeftJoin<User, UserRole>((u, ur) => u.Id == ur.UserId)
                                                                            .LeftJoin<UserRole, Role>((ur, r) => ur.RoleId == r.Id);

        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, TransactionContext? transactionContext)
        {
            return await GetUsingCacheAsideAsync<Role>(
                new CachedRolesByUserId(userId),
                dbReader => GetRolesByUserIdCoreAsync(userId, transactionContext)).ConfigureAwait(false);

            async Task<IEnumerable<Role>> GetRolesByUserIdCoreAsync(Guid userId, TransactionContext? transactionContext)
            {
                var where = DbReader.Where<User>(u => u.Id == userId);

                IEnumerable<Tuple<User, UserRole?, Role?>> result = await DbReader.RetrieveAsync<User, UserRole, Role>(
                    FromUserToRole,
                    where,
                    transactionContext).ConfigureAwait(false);

                return result.Where(t => t.Item3 != null).Select(t => t.Item3!).ToList();
            }
        }

        /// <summary>
        /// 如果已经存在其中一些，则报错
        /// </summary>
        public async Task AddRolesByUserIdAsync(Guid userId, IEnumerable<Role> roles, string lastUser, TransactionContext transactionContext)
        {
            ThrowIf.Null(transactionContext, nameof(transactionContext));

            long count = await DbReader.CountAsync<UserRole>(
                ur => ur.UserId == userId && SqlStatement.In(ur.RoleId, false, roles.Select(r => r.Id)),
                transactionContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw IdentityExceptions.AlreadyHaveRoles(userId, roles, lastUser);
            }

            List<UserRole> userRoles = roles.Select(r => new UserRole(userId, r.Id)).ToList();

            _ = await AddAsync(userRoles, lastUser, transactionContext).ConfigureAwait(false);
        }

        public async Task RemoveRolesByUserIdAsync(Guid userId, IEnumerable<Role> roles, string lastUser, TransactionContext transactionContext)
        {
            ThrowIf.Null(transactionContext, nameof(transactionContext));

            IEnumerable<UserRole> userRoles = await DbReader.RetrieveAsync<UserRole>(
                ur => ur.UserId == userId && SqlStatement.In(ur.RoleId, false, roles.Select(r => r.Id)),
                transactionContext).ConfigureAwait(false);

            if (!userRoles.Any())
            {
                return;
            }

            await DeleteAsync(userRoles, lastUser, transactionContext).ConfigureAwait(false);
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Cache;
using HB.FullStack.Database;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Lock.Memory;
using HB.FullStack.Repository;

using Microsoft.Extensions.Logging;
using HB.FullStack.KVStore;
using HB.FullStack.Common;

namespace HB.FullStack.Server.Identity
{
    /// <summary>
    /// 所有的User这个Model的增删改查都要经过这里
    /// 所有通过User来使用与User相关的关系表的，都经过这里
    /// </summary>
    public class UserRepo<TId> : DbModelRepository<User<TId>>
    {
        public UserRepo(ILogger<UserRepo<TId>> logger, IDbReader databaseReader, ICache cache, IMemoryLockManager memoryLockManager)
            : base(logger, databaseReader, cache, memoryLockManager)
        {
            ModelUpdating += (sender, args) =>
            {
                if (sender is IEnumerable<User<TId>> users)
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

        protected override Task InvalidateCacheItemsOnChanged(object sender, ModelChangeEventArgs args)
        {
            if (sender is IEnumerable<UserRole<TId>> userRoles)
            {
                foreach (UserRole<TId> userRole in userRoles)
                {
                    //User-Role发生变化，就Invalidate
                    //比如：为用户添加角色，删除角色
                    InvalidateCache(new CachedRolesByUserId<TId>(userRole.UserId));
                }
            }

            return Task.CompletedTask;
        }

        #region 主表 Read 所有的查询都要经过这里

        public async Task<User<TId>?> GetByIdAsync(TId userId, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                keyName: nameof(IDbModel.Id),
                keyValue: userId!,
                dbRetrieve: db =>
                {
                    return db.ScalarAsync<User<TId>>(userId!, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<IList<User<TId>>> GetByIdsAsync(IList<long> userIds, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(IDbModel.Id),
                userIds,
                db =>
                {
                    return db.RetrieveAsync<User<TId>>(u => SqlStatement.In(u.Id, true, userIds), transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User<TId>?> GetByMobileAsync(string mobile, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(User<TId>.Mobile),
                mobile,
                db =>
                {
                    return db.ScalarAsync<User<TId>>(u => u.Mobile == mobile, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User<TId>?> GetByLoginNameAsync(string loginName, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(User<TId>.LoginName),
                loginName,
                db =>
                {
                    return db.ScalarAsync<User<TId>>(u => u.LoginName == loginName, transContext);
                }).ConfigureAwait(false);
        }

        public async Task<User<TId>?> GetByEmailAsync(string email, TransactionContext? transContext = null)
        {
            return await GetUsingCacheAsideAsync(
                nameof(User<TId>.Email),
                email,
                db =>
                {
                    return db.ScalarAsync<User<TId>>(u => u.Email == email, transContext);
                }).ConfigureAwait(false);
        }

        public Task<long> CountUserAsync(string? loginName, string? mobile, string? email, TransactionContext? transContext)
        {
            WhereExpression<User<TId>> where = DbReader.Where<User<TId>>(u => u.Mobile == mobile).Or(u => u.LoginName == loginName).Or(u => u.Email == email);
            return DbReader.CountAsync(where, transContext);
        }

        #endregion

        #region 关系表 User-Role CRUD

        private FromExpression<User<TId>>? _fromUserToRole;
        private FromExpression<User<TId>> FromUserToRole => _fromUserToRole ??= DbReader
                                                                            .From<User<TId>>()
                                                                            .LeftJoin<User<TId>, UserRole<TId>>((u, ur) => u.Id!.Equals(ur.UserId))
                                                                            .LeftJoin<UserRole<TId>, Role<TId>>((ur, r) => ur.RoleId!.Equals(r.Id));

        public async Task<IList<Role<TId>>> GetRolesByUserIdAsync(TId userId, TransactionContext? transactionContext)
        {
            return await GetUsingCacheAsideAsync<Role<TId>>(
                new CachedRolesByUserId<TId>(userId),
                dbReader => GetRolesByUserIdCoreAsync(userId, transactionContext)).ConfigureAwait(false);

            async Task<IList<Role<TId>>> GetRolesByUserIdCoreAsync(TId userId, TransactionContext? transactionContext)
            {
                var where = DbReader.Where<User<TId>>(u => u.Id!.Equals(userId));

                IList<Tuple<User<TId>, UserRole<TId>?, Role<TId>?>> result = await DbReader.RetrieveAsync<User<TId>, UserRole<TId>, Role<TId>>(
                    FromUserToRole,
                    where,
                    transactionContext).ConfigureAwait(false);

                return result.Where(t => t.Item3 != null).Select(t => t.Item3!).ToList();
            }
        }

        /// <summary>
        /// 如果已经存在其中一些，则报错
        /// </summary>
        public async Task AddRolesByUserIdAsync(TId userId, IEnumerable<Role<TId>> roles, string lastUser, TransactionContext transactionContext)
        {
            ThrowIf.Null(transactionContext, nameof(transactionContext));

            long count = await DbReader.CountAsync<UserRole<TId>>(
                ur => ur.UserId!.Equals(userId) && SqlStatement.In(ur.RoleId, false, roles.Select(r => r.Id)),
                transactionContext).ConfigureAwait(false);

            if (count != 0)
            {
                throw IdentityExceptions.AlreadyHaveRoles(userId!, roles, lastUser);
            }

            List<UserRole<TId>> userRoles = roles.Select(r => new UserRole<TId>(userId, r.Id)).ToList();

            await AddAsync<UserRole<TId>>(userRoles, lastUser, transactionContext).ConfigureAwait(false);
        }

        public async Task RemoveRolesByUserIdAsync(TId userId, IEnumerable<Role<TId>> roles, string lastUser, TransactionContext transactionContext)
        {
            ThrowIf.Null(transactionContext, nameof(transactionContext));

            IList<UserRole<TId>> userRoles = await DbReader.RetrieveAsync<UserRole<TId>>(
                ur => ur.UserId!.Equals(userId) && SqlStatement.In(ur.RoleId, false, roles.Select(r => r.Id)),
                transactionContext).ConfigureAwait(false);

            if (!userRoles.Any())
            {
                return;
            }

            await DeleteAsync<UserRole<TId>>(userRoles, lastUser, transactionContext).ConfigureAwait(false);
        }

        #endregion
    }
}
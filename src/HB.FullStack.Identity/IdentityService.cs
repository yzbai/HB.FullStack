using HB.FullStack.Identity.Entities;
using HB.FullStack.Database;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace HB.FullStack.Identity
{
    internal class IdentityService : IIdentityService
    {
        private readonly ITransaction _transaction;
        private readonly UserRepo _userRepo;
        private readonly RoleOfUserRepo _roleOfUserRepo;

        public IdentityService(ITransaction transaction, UserRepo userRepo, RoleOfUserRepo roleOfUserRepo)
        {
            _userRepo = userRepo;
            _roleOfUserRepo = roleOfUserRepo;
            _transaction = transaction;
        }

        #region User

        /// <summary>
        /// CreateUserAsync
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <param name="mobileConfirmed"></param>
        /// <param name="emailConfirmed"></param>
        /// <param name="lastUser"></param>
        /// <param name="transactionContext"></param>
        /// <returns></returns>
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser, TransactionContext? transactionContext = null)
        {
            ThrowIf.NotMobile(mobile, nameof(mobile), true);
            ThrowIf.NotEmail(email, nameof(email), true);
            ThrowIf.NotLoginName(loginName, nameof(loginName), true);
            ThrowIf.NotPassword(password, nameof(password), true);

            if (mobile == null && email == null && loginName == null)
            {
                throw new IdentityException(IdentityErrorCode.IdentityMobileEmailLoginNameAllNull);
            }

            if (!mobileConfirmed && !emailConfirmed && password == null)
            {
                throw new IdentityException(IdentityErrorCode.IdentityNothingConfirmed);
            }

            bool ownTrans = transactionContext == null;

            TransactionContext transContext = transactionContext ?? await _transaction.BeginTransactionAsync<User>().ConfigureAwait(false);

            try
            {
                long count = await _userRepo.CountUserAsync(loginName, mobile, email, transContext).ConfigureAwait(false);

                if (count != 0)
                {
                    throw new IdentityException(IdentityErrorCode.IdentityAlreadyTaken, $"userType:{typeof(User)}, mobile:{mobile}, email:{email}, loginName:{loginName}");
                }

                User user = new User(loginName, mobile, email, password, mobileConfirmed, emailConfirmed);

                await _userRepo.AddAsync(user, lastUser, transContext).ConfigureAwait(false);

                if (ownTrans)
                {
                    await transContext.CommitAsync().ConfigureAwait(false);
                }

                return user;
            }
            catch
            {
                if (ownTrans)
                {
                    await transContext.RollbackAsync().ConfigureAwait(false);
                }

                throw;
            }
        }

        #endregion

        #region Role

        /// <summary>
        /// AddRolesToUserAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task AddRolesToUserAsync(long userId, long roleId, string lastUser)
        {
            ThrowIf.NotLongId(userId, nameof(userId));
            ThrowIf.NotLongId(roleId, nameof(roleId));

            TransactionContext trans = await _transaction.BeginTransactionAsync<RoleOfUser>().ConfigureAwait(false);
            try
            {
                //查重
                long count = await _roleOfUserRepo.CountByUserIdAndRoleIdAsync(userId, roleId, trans).ConfigureAwait(false);

                if (count != 0)
                {
                    throw new IdentityException(IdentityErrorCode.FoundTooMuch, $"已经有相同的角色. UserId:{userId}, RoleId:{roleId}");
                }

                RoleOfUser ru = new RoleOfUser(userId, roleId);

                await _roleOfUserRepo.UpdateAsync(ru, lastUser, trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// RemoveRoleFromUserAsync
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        /// <exception cref="IdentityException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public async Task RemoveRoleFromUserAsync(long userId, long roleId, string lastUser)
        {
            ThrowIf.NotLongId(userId, nameof(userId));
            ThrowIf.NotLongId(roleId, nameof(roleId));

            TransactionContext trans = await _transaction.BeginTransactionAsync<RoleOfUser>().ConfigureAwait(false);
            try
            {
                //查重
                RoleOfUser? stored = await _roleOfUserRepo.GetByUserIdAndRoleIdAsync(userId, roleId, trans).ConfigureAwait(false);

                if (stored == null)
                {
                    throw new IdentityException(IdentityErrorCode.NotFound, $"没有找到这样的角色. UserId:{userId}, RoleId:{roleId}");
                }

                await _roleOfUserRepo.DeleteAsync(stored, lastUser, trans).ConfigureAwait(false);

                await trans.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await trans.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        #endregion
    }
}

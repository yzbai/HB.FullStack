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
        private readonly UserRoleRepo _userRoleRepo;

        public IdentityService(ITransaction transaction, UserRepo userRepo, UserRoleRepo userRoleRepo)
        {
            _userRepo = userRepo;
            _userRoleRepo = userRoleRepo;
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
                throw Exceptions.IdentityMobileEmailLoginNameAllNull();
            }

            if (!mobileConfirmed && !emailConfirmed && password == null)
            {
                throw Exceptions.IdentityNothingConfirmed();
            }

            bool ownTrans = transactionContext == null;

            TransactionContext transContext = transactionContext ?? await _transaction.BeginTransactionAsync<User>().ConfigureAwait(false);

            try
            {
                long count = await _userRepo.CountUserAsync(loginName, mobile, email, transContext).ConfigureAwait(false);

                if (count != 0)
                {
                    throw Exceptions.IdentityAlreadyTaken(mobile: mobile, email: email, loginName: loginName);
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
        public async Task AddRolesToUserAsync(Guid userId, Guid roleId, string lastUser)
        {
            ThrowIf.Empty(ref userId, nameof(userId));
            ThrowIf.Empty(ref roleId, nameof(roleId));

            TransactionContext trans = await _transaction.BeginTransactionAsync<UserRole>().ConfigureAwait(false);
            try
            {
                //查重
                long count = await _userRoleRepo.CountByUserIdAndRoleIdAsync(userId, roleId, trans).ConfigureAwait(false);

                if (count != 0)
                {
                    throw Exceptions.FoundTooMuch(userId: userId, roleId: roleId, cause: "已经有相同的角色");
                }

                UserRole ru = new UserRole(userId, roleId);

                await _userRoleRepo.UpdateAsync(ru, lastUser, trans).ConfigureAwait(false);

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
        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string lastUser)
        {
            ThrowIf.Empty(ref userId, nameof(userId));
            ThrowIf.Empty(ref roleId, nameof(roleId));

            TransactionContext trans = await _transaction.BeginTransactionAsync<UserRole>().ConfigureAwait(false);
            try
            {
                //查重
                UserRole? stored = await _userRoleRepo.GetByUserIdAndRoleIdAsync(userId, roleId, trans).ConfigureAwait(false);

                if (stored == null)
                {
                    throw Exceptions.NotFound(userId: userId, roleId: roleId, cause: "没有找到这样的角色");
                }

                await _userRoleRepo.DeleteAsync(stored, lastUser, trans).ConfigureAwait(false);

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

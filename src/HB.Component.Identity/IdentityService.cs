using HB.Component.Identity.Biz;
using HB.Component.Identity.Entities;
using HB.FullStack.Database;

using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class IdentityService : IIdentityService
    {
        private readonly ITransaction _transaction;
        private readonly UserBiz _userBiz;
        private readonly ClaimsPrincipalFactory _claimsFactory;
        private readonly UserLoginControlBiz _userLoginControlBiz;

        public IdentityService(ITransaction transaction, UserBiz userBiz, UserLoginControlBiz userLoginControlBiz, ClaimsPrincipalFactory claimsFactory)
        {
            _userBiz = userBiz;
            _transaction = transaction;
            _claimsFactory = claimsFactory;
            _userLoginControlBiz = userLoginControlBiz;
        }

        public async Task<User> CreateUserAsync(string mobile, string? email, string? loginName, string? password, bool mobileConfirmed, bool emailConfirmed, string lastUser)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                User user = await _userBiz.CreateAsync(mobile, email, loginName, password, mobileConfirmed, emailConfirmed, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return user;
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public Task<User?> GetUserByMobileAsync(string mobile)
        {
            return _userBiz.GetByMobileAsync(mobile);
        }

        public Task<User?> GetUserByLoginNameAsync(string loginName)
        {
            return _userBiz.GetByLoginNameAsync(loginName);
        }

        public async Task SetAccessFailedCountAsync(string userGuid, long count, string lastUser)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<User>().ConfigureAwait(false);
            try
            {
                await _userLoginControlBiz.SetAccessFailedCountAsync(userGuid, count, lastUser).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public async Task SetLockoutAsync(string userGuid, bool lockout, string lastUser, TimeSpan? lockoutTimeSpan = null)
        {
            await _userLoginControlBiz.SetLockoutAsync(userGuid, lockout, lastUser, lockoutTimeSpan).ConfigureAwait(false);
        }

        public async Task<User?> GetUserByUserGuidAsync(string userGuid)
        {
            return await _userBiz.GetByGuidAsync(userGuid, null).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Claim>> CreateUserClaimAsync(User user)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<UserClaim>().ConfigureAwait(false);
            try
            {
                IEnumerable<Claim> claims = await _claimsFactory.CreateClaimsAsync(user, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return claims;
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }
    }
}

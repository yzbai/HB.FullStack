using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entities;
using HB.Framework.Database;
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
        private readonly IUserBiz _userBiz;
        private readonly IClaimsPrincipalFactory _claimsFactory;

        public IdentityService(ITransaction transaction, IUserBiz userBiz, IClaimsPrincipalFactory claimsFactory)
        {
            _userBiz = userBiz;
            _transaction = transaction;
            _claimsFactory = claimsFactory;
        }

        public async Task<TUser> CreateUserByMobileAsync<TUser>(string mobile, string? loginName, string? password, bool mobileConfirmed, string lastUser) where TUser : IdentityUser, new()
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<TUser>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                TUser user = await _userBiz.CreateByMobileAsync<TUser>(mobile, loginName, password, mobileConfirmed, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);

                return user;
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public Task<TUser?> GetUserByMobileAsync<TUser>(string mobile) where TUser : IdentityUser, new()
        {
            return _userBiz.GetByMobileAsync<TUser>(mobile);
        }

        public Task<TUser?> GetUserByLoginNameAsync<TUser>(string loginName) where TUser : IdentityUser, new()
        {
            return _userBiz.GetByLoginNameAsync<TUser>(loginName);
        }

        public async Task SetAccessFailedCountAsync<TUser>(string userGuid, long count, string lastUser) where TUser : IdentityUser, new()
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<TUser>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                await _userBiz.SetAccessFailedCountAsync<TUser>(userGuid, count, lastUser, transactionContext).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public async Task SetLockoutAsync<TUser>(string userGuid, bool lockout, string lastUser, TimeSpan? lockoutTimeSpan = null) where TUser : IdentityUser, new()
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<TUser>(IsolationLevel.ReadCommitted).ConfigureAwait(false);

            try
            {
                await _userBiz.SetLockoutAsync<TUser>(userGuid, lockout, lastUser, transactionContext, lockoutTimeSpan).ConfigureAwait(false);

                await _transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await _transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        public Task<TUser?> GetUserBySecurityStampAsync<TUser>(string userGuid, string? securityStamp) where TUser : IdentityUser, new()
        {
            return _userBiz.GetUserBySecurityStampAsync<TUser>(userGuid, securityStamp);
        }

        public async Task<IEnumerable<Claim>> GetUserClaimAsync<TUserClaim, TRole, TRoleOfUser>(IdentityUser user)
            where TUserClaim : IdentityUserClaim, new()
            where TRole : IdentityRole, new()
            where TRoleOfUser : IdentityRoleOfUser, new()
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<TUserClaim>(IsolationLevel.ReadCommitted).ConfigureAwait(false);
            try
            {
                IEnumerable<Claim> claims = await _claimsFactory.CreateClaimsAsync<TUserClaim, TRole, TRoleOfUser>(user, transactionContext).ConfigureAwait(false);

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

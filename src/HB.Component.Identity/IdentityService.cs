using HB.Component.Identity.Abstractions;
using HB.Component.Identity.Entity;
using HB.Framework.Database.Transaction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HB.Component.Identity
{
    internal class IdentityService : IIdentityService
    {
        private readonly IdentityOptions _options;
        private readonly IUserBiz _userBiz;
        private readonly IClaimsPrincipalFactory claimsFactory;
        private readonly ITransaction transaction;
        private readonly ILogger logger;

        public IdentityService(IOptions<IdentityOptions> options, ITransaction transaction, ILogger<IdentityService> logger, IUserBiz userBiz, IClaimsPrincipalFactory claimsFactory)
        {
            _options = options.Value;
            _userBiz = userBiz;
            this.transaction = transaction;
            this.logger = logger;
            this.claimsFactory = claimsFactory;
        }

        public async Task<IdentityResult> CreateUserByMobileAsync(string userType, string mobile, string userName, string password, bool mobileConfirmed)
        {
            TransactionContext transactionContext = transaction.BeginTransaction<User>();
            try
            {
                IdentityResult result = await _userBiz.CreateByMobileAsync(userType, mobile, userName, password, mobileConfirmed, transactionContext).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    transaction.Rollback(transactionContext);
                    return result;
                }

                transaction.Commit(transactionContext);

                return result;
            }
            catch(Exception ex)
            {
                transaction.Rollback(transactionContext);
                logger.LogCritical(ex, $"UserType :{userType}, Mobile:{mobile}");
                return IdentityResult.Throwed();
            }
        }

        public Task<User> GetUserByMobileAsync(string mobile)
        {
            return _userBiz.GetByMobileAsync(mobile);
        }

        public Task<User> GetUserByUserNameAsync(string userName)
        {
            return _userBiz.GetByUserNameAsync(userName);
        }

        public async Task<IdentityResult> SetAccessFailedCountAsync(string userGuid, long count)
        {
            TransactionContext transactionContext = transaction.BeginTransaction<User>();
            try
            {
                IdentityResult result = await _userBiz.SetAccessFailedCountAsync(userGuid, count, transactionContext).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    transaction.Rollback(transactionContext);
                    return result;
                }

                transaction.Commit(transactionContext);

                return result;
            }
            catch(Exception ex)
            {
                transaction.Rollback(transactionContext);
                logger.LogCritical(ex, $"UserGuid:{userGuid}, Count:{count}");
                return IdentityResult.Throwed();
            }
        }

        public async Task<IdentityResult> SetLockoutAsync(string userGuid, bool lockout, TimeSpan? lockoutTimeSpan = null)
        {
            TransactionContext transactionContext = transaction.BeginTransaction<User>();

            try
            {
                IdentityResult result = await _userBiz.SetLockoutAsync(userGuid, lockout, transactionContext, lockoutTimeSpan).ConfigureAwait(false);

                if (!result.IsSucceeded())
                {
                    transaction.Rollback(transactionContext);
                    return result;
                }

                transaction.Commit(transactionContext);

                return result;
            }
            catch (Exception ex)
            {
                transaction.Rollback(transactionContext);
                logger.LogCritical(ex, $"UserGuid:{userGuid}");
                return IdentityResult.Throwed();
            }
        }

        public Task<User> ValidateSecurityStampAsync(string userGuid, string securityStamp)
        {
            return _userBiz.ValidateSecurityStampAsync(userGuid, securityStamp);
        }

        public async Task<IList<Claim>> GetUserClaimAsync(User user)
        {
            TransactionContext transactionContext = transaction.BeginTransaction<User>();
            try
            {
                IList<Claim> claims = await claimsFactory.CreateClaimsAsync(user, transactionContext).ConfigureAwait(false);

                transaction.Commit(transactionContext);

                return claims;
            }
            catch (Exception ex)
            {
                transaction.Rollback(transactionContext);
                logger.LogCritical(ex, $"User :{JsonUtil.ToJson(user)}");
                return new List<Claim>();
            }
        }
    }
}

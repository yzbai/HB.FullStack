using HB.FullStack.Identity.Entities;
using HB.FullStack.Database;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HB.FullStack.Identity
{
    internal class IdentityService : IIdentityService
    {
        private readonly ITransaction _transaction;
        private readonly UserBiz _userBiz;

        public IdentityService(ITransaction transaction, UserBiz userBiz)
        {
            _userBiz = userBiz;
            _transaction = transaction;
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
    }
}

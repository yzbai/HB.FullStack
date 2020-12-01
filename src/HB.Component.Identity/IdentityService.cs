using HB.FullStack.Identity.Entities;
using HB.FullStack.Database;
using System.Threading.Tasks;

namespace HB.FullStack.Identity
{
    internal class IdentityService : IIdentityService
    {
        private readonly ITransaction _transaction;
        private readonly UserBiz _userBiz;
        private readonly UserLoginControlBiz _userLoginControlBiz;

        public IdentityService(ITransaction transaction, UserBiz userBiz, UserLoginControlBiz userLoginControlBiz)
        {
            _userBiz = userBiz;
            _transaction = transaction;
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

        public async Task<User?> GetUserByUserGuidAsync(string userGuid)
        {
            return await _userBiz.GetByGuidAsync(userGuid, null).ConfigureAwait(false);
        }



    }
}

using System;
using System.Threading.Tasks;

using HB.FullStack.Database;



namespace HB.FullStack.Client.Services.KVManager
{
    internal class DbKVManager : IKVManager
    {
        private readonly ITransaction _transaction;
        private readonly KVRepo _kvRepo;

        public DbKVManager(ITransaction transaction, KVRepo kvRepo)
        {
            _transaction = transaction;
            _kvRepo = kvRepo;
        }

        public async Task SetAsync<T>(string key, T? value, TimeSpan? aliveTime)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<KV>().ConfigureAwait(false);

            try
            {
                await _kvRepo.SetAsync(key, value, aliveTime, transactionContext).ConfigureAwait(false);

                await transactionContext.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transactionContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<KV>().ConfigureAwait(false);

            try
            {
                T? stored = await _kvRepo.GetAsync<T>(key, transactionContext).ConfigureAwait(false);

                await transactionContext.CommitAsync().ConfigureAwait(false);

                return stored;
            }
            catch
            {
                await transactionContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task DeleteAsync(string key)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<KV>().ConfigureAwait(false);

            try
            {
                await _kvRepo.DeleteAsync(key, transactionContext).ConfigureAwait(false);

                await transactionContext.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transactionContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
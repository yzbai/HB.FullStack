using System;
using System.Threading.Tasks;
using HB.FullStack.Database;

using Microsoft.Extensions.Logging;


namespace HB.FullStack.Client.Components.KVManager
{
    internal class DbSimpleLocker : IDbSimpleLocker
    {
        private readonly ITransaction _transaction;
        private readonly KVRepo _kvRepo;
        private readonly ILogger<DbSimpleLocker> _logger;

        public DbSimpleLocker(ITransaction transaction, KVRepo kvRepo, ILogger<DbSimpleLocker> logger)
        {
            _transaction = transaction;
            _kvRepo = kvRepo;
            _logger = logger;
        }

        /// <summary>
        /// 返回是否成功，如果没有成功，返回
        /// </summary>
        public async Task<bool> NoWaitLockAsync(string resourceType, string resource, TimeSpan availableTime)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<KV>().ConfigureAwait(false);

            try
            {
                bool result;

                string key = GetKey(resourceType, resource);

                string? storedValue = await _kvRepo.GetAsync<string>(key, transactionContext).ConfigureAwait(false);

                if (storedValue == null)
                {
                    //上锁
                    await _kvRepo.SetAsync(key, "empty", availableTime, transactionContext).ConfigureAwait(false);

                    result = true;
                }
                else
                {
                    result = false;
                }

                await transactionContext.CommitAsync().ConfigureAwait(false);

                return result;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogDbSimpleLockerNoWaitLockFailed(resourceType, resource, availableTime, ex);

                //抢占失败
                await transactionContext.RollbackAsync().ConfigureAwait(false);

                return false;
            }
        }

        public async Task<bool> UnLockAsync(string resourceType, string resource)
        {
            TransactionContext transactionContext = await _transaction.BeginTransactionAsync<KV>().ConfigureAwait(false);

            try
            {
                string key = GetKey(resourceType, resource);

                await _kvRepo.DeleteAsync(key, transactionContext).ConfigureAwait(false);

                await transactionContext.CommitAsync().ConfigureAwait(false);

                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogDbSimpleLockerUnLockFailed(resourceType, resource, ex);
                await transactionContext.RollbackAsync().ConfigureAwait(false);
                return false;
            }
        }

        private static string GetKey(string resourceType, string resource)
        {
            return resourceType + resource;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database;

namespace HB.FullStack.Client.KeyValue
{
    /// <summary>
    /// //TODO: 简单的KV加过期系统，后期添加泛型，删除过期
    /// </summary>
    public class KVManager
    {
        private readonly IDatabase _database;

        public KVManager(IDatabase database)
        {
            _database = database;
        }

        public async Task SetAsync<T>(string key, T? value, TimeSpan? aliveTime, TransactionContext? transactionContext)
        {
            await _database.DeleteAsync<KV>(kv => kv.Key == key, "", transactionContext, true).ConfigureAwait(false);

            KV kv = new KV { Key = key, Value = SerializeUtil.ToJson(value), ExpiredAt = aliveTime.HasValue ? TimeUtil.UtcNow + aliveTime.Value : DateTimeOffset.MaxValue };

            await _database.AddAsync(kv, "", transactionContext).ConfigureAwait(false);
        }

        public async Task<T?> GetAsync<T>(string key, TransactionContext? transactionContext)
        {
            KV? kv = await _database.ScalarAsync<KV>(kv => kv.Key == key, transactionContext).ConfigureAwait(false);

            if (kv != null && kv.ExpiredAt > TimeUtil.UtcNow)
            {
                return SerializeUtil.FromJson<T>(kv.Value);
            }

            return default;
        }

        public async Task DeleteAsync(string key, TransactionContext? transactionContext)
        {
            await _database.DeleteAsync<KV>(kv => kv.Key == key, "", transactionContext, true).ConfigureAwait(false);
        }
    }
}
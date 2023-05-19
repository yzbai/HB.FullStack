using System;
using System.Threading.Tasks;

using HB.FullStack.Database;

namespace HB.FullStack.Client.Components.KVManager
{
    /// <summary>
    /// //TODO: 简单的KV加过期系统，后期添加泛型，删除过期
    /// </summary>
    public class KVRepo
    {
        private readonly IDatabase _database;

        public KVRepo(IDatabase database)
        {
            _database = database;
        }

        public async Task SetAsync<T>(string key, T? value, TimeSpan? aliveTime, TransactionContext? transactionContext)
        {
            KV kv = new KV
            {
                Id = key,
                Value = SerializeUtil.ToJson(value),
                ExpiredAt = aliveTime.HasValue ? (TimeUtil.UtcNow + aliveTime.Value).Ticks : long.MaxValue
            };

            await _database.AddOrUpdateByIdAsync(kv, "", transactionContext).ConfigureAwait(false);
        }

        public async Task<T?> GetAsync<T>(string key, TransactionContext? transactionContext)
        {
            KV? kv = await _database.ScalarAsync().ScalarAsync<KV>(kv => kv.Key == key, transactionContext).ConfigureAwait(false);

            if (kv != null && !kv.IsExpired())
            {
                return SerializeUtil.FromJson<T>(kv.Value);
            }

            return default;
        }

        public async Task DeleteAsync(string key, TransactionContext transactionContext)
        {
            await _database.DeleteAsync<KV>(kv => kv.Key == key, "", transactionContext).ConfigureAwait(false);
        }
    }
}
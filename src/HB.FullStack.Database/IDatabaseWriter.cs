using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;

namespace HB.FullStack.Database
{
    public interface IDatabaseWriter
    {
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity2, new();

        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity2, new();

        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity2, new();

        Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity2, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity2, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity2, new();
    }
}
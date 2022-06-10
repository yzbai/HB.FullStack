/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Database.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Database
{
    public interface IDatabaseWriter
    {
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task UpdateFieldsAsync<T>(object id, int version, string lastUser, IDictionary<string, object?> propertyValues, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();
    }
}
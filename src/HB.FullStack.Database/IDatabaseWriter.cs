using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using HB.FullStack.Database.Def;

namespace HB.FullStack.Database
{
    public interface IDatabaseWriter
    {
        /// <exception cref="DatabaseException"></exception>
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <exception cref="DatabaseException"></exception>
        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();
    }
}
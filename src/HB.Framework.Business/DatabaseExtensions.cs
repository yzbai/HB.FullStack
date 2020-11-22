using System;
using System.Threading.Tasks;
using HB.Framework.Common.Entities;

namespace HB.Framework.Database
{
    public static class DatabaseExtensions
    {
        public static async Task UpdateAsync<T>(this IDatabase database, T entity, Func<T, Task> entityUpdating, Func<T, Task> entityUpdated, string lastUser, TransactionContext? transContext) where T : Entity, new()
        {
            await entityUpdating(entity).ConfigureAwait(false);
            await database.UpdateAsync(entity, lastUser, transContext).ConfigureAwait(false);
            await entityUpdated(entity).ConfigureAwait(false);
        }

        public static async Task AddAsync<T>(this IDatabase database, T entity, Func<T, Task> entityAdding, Func<T, Task> entityAdded, string lastUser, TransactionContext? transContext) where T : Entity, new()
        {
            await entityAdding(entity).ConfigureAwait(false);
            await database.AddAsync(entity, lastUser, transContext).ConfigureAwait(false);
            await entityAdded(entity).ConfigureAwait(false);
        }

        public static async Task DeleteAsync<T>(this IDatabase database, T entity, Func<T, Task> entityDeleting, Func<T, Task> entityDeleted, string lastUser, TransactionContext? transContext) where T : Entity, new()
        {
            await entityDeleting(entity).ConfigureAwait(false);
            await database.DeleteAsync(entity, lastUser, transContext).ConfigureAwait(false);
            await entityDeleted(entity).ConfigureAwait(false);
        }


    }
}

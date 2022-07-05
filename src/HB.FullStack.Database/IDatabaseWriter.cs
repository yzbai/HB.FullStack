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
        /// <summary>
        /// Add成功后，Version为0
        /// </summary>
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task UpdateAsync<T>(T item, int updateToVersion, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <summary>
        /// Version自动加1
        /// </summary>
        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <summary>
        /// Version自动加1. 通过version来实现行粒度的乐观锁
        /// </summary>
        Task UpdateFieldsAsync<T>(object id, int curVersion, string lastUser, IList<(string, object?)> propertyNameValues, TransactionContext? transContext) where T : DatabaseEntity, new();

        /// <summary>
        /// Version自动加1，通过比较oldvalue来实现字段力度乐观锁
        /// </summary>
        /// <param name="propertyOldNewValues">(propertyName-oldvalue-newvalue）</param>
        Task UpdateFieldsAsync<T>(object id, string lastUser, IList<(string, object?, object?)> propertyNameOldNewValues, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task<IEnumerable<object>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext? transContext) where T : DatabaseEntity, new();
    }
}
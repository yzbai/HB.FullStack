using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HB.Framework.Common.Entities;

namespace HB.Framework.Database
{
    public interface IDatabaseWriter
    {
        Task AddAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();

        Task UpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();

        /// <summary>
        /// Base on Guid字段，检测重复，Entity有多个Unique索引的，禁止使用
        /// </summary>
        Task AddOrUpdateAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();

        Task DeleteAsync<T>(T item, string lastUser, TransactionContext? transContext) where T : Entity, new();

        /// <summary>
        /// 返回每一个数据对应的Version
        /// Base on Guid字段，检测重复，Entity有多个Unique索引的，禁止使用
        /// </summary>
        Task<IEnumerable<Tuple<long, int>>> BatchAddOrUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transaction) where T : Entity, new();

        Task<IEnumerable<long>> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new();

        Task BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new();

        Task BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : Entity, new();
    }
}
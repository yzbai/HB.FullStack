using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.KVStore.Entity;

namespace HB.Framework.KVStore
{
    public interface IKVStoreAsync
    {
        Task<T> GetByIdAsync<T>(object keyValue) where T : KVStoreEntity, new();

        Task<T> GetByIdAsync<T>(T t) where T : KVStoreEntity, new();

        Task<IEnumerable<T>> GetByIdsAsync<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new();

        Task<IEnumerable<T>> GetByIdsAsync<T>(IEnumerable<T> ts) where T : KVStoreEntity, new();

        Task<IEnumerable<T>> GetAllAsync<T>() where T : KVStoreEntity, new();

        Task<KVStoreResult> AddAsync<T>(T item) where T : KVStoreEntity, new();

        Task<KVStoreResult> AddAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        /// <summary>
        /// item的Version会被改变
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<KVStoreResult> UpdateAsync<T>(T item) where T : KVStoreEntity, new();

        /// <summary>
        /// item的Version会被改变
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<KVStoreResult> UpdateAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        Task<KVStoreResult> DeleteAsync<T>(T item) where T : KVStoreEntity, new();

        Task<KVStoreResult> DeleteAllAsync<T>() where T : KVStoreEntity, new();

        Task<KVStoreResult> DeleteByIdAsync<T>(object keyValue, int version) where T : KVStoreEntity, new();

        Task<KVStoreResult> DeleteByIdsAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new();

    }
}

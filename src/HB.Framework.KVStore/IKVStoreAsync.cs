using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.KVStore.Entity;

namespace HB.Framework.KVStore
{
    public interface IKVStoreAsync
    {
        Task<T> GetByKeyAsync<T>(object keyValue) where T : KVStoreEntity, new();

        Task<T> GetByKeyAsync<T>(T t) where T : KVStoreEntity, new();

        Task<IEnumerable<T>> GetByKeysAsync<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new();

        Task<IEnumerable<T>> GetByKeysAsync<T>(IEnumerable<T> ts) where T : KVStoreEntity, new();

        Task<IEnumerable<T>> GetAllAsync<T>() where T : KVStoreEntity, new();

        Task AddAsync<T>(T item) where T : KVStoreEntity, new();

        Task AddAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        /// <summary>
        /// item的Version会被改变
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        Task UpdateAsync<T>(T item) where T : KVStoreEntity, new();

        /// <summary>
        /// item的Version会被改变
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        Task UpdateAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        Task DeleteAsync<T>(T item) where T : KVStoreEntity, new();

        Task DeleteAllAsync<T>() where T : KVStoreEntity, new();

        Task DeleteByKeyAsync<T>(object keyValue, int version) where T : KVStoreEntity, new();

        Task DeleteByKeysAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new();

    }
}

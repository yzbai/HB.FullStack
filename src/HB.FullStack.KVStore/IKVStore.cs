
using HB.FullStack.KVStore.KVStoreModels;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace HB.FullStack.KVStore
{
    public interface IKVStore
    {

        async Task<T?> GetAsync<T>(object key) where T : class, IKVStoreModel
        {
            IEnumerable<T?> ts = await GetAsync<T>(new object[] { key }).ConfigureAwait(false);

            return ts.Any() ? ts.ElementAt(0) : null;
        }


        Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<object> keys) where T : class, IKVStoreModel;


        Task<IEnumerable<T?>> GetAllAsync<T>() where T : class, IKVStoreModel;


        Task AddAsync<T>(T item, string lastUser) where T : class, IKVStoreModel => AddAsync(new T[] { item }, lastUser);

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task AddAsync<T>(IEnumerable<T> items, string lastUser) where T : class, IKVStoreModel;


        Task UpdateAsync<T>(T item, string lastUser) where T : class, IKVStoreModel => UpdateAsync(new T[] { item }, lastUser);

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task UpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : class, IKVStoreModel;


        Task DeleteAsync<T>(T item) where T: class, IKVStoreModel;

        Task DeleteAsync<T>(object key, long timestamp) where T : class, IKVStoreModel => DeleteAsync<T>(new object[] { key }, new long[] { timestamp });

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task DeleteAsync<T>(IEnumerable<object> keys, IEnumerable<long> timestamps) where T : class, IKVStoreModel;


        Task DeleteAllAsync<T>() where T : class, IKVStoreModel;


        //string GetModelKey<T>(T item) where T : class, IKVStoreModel;
    }
}
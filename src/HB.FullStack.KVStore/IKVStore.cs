
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
        
        Task<T?> GetAsync<T>(string key) where T : KVStoreModel, new();

        
        Task<T?> GetAsync<T>(long key) where T : KVStoreModel, new()
        {
            return GetAsync<T>(key.ToString(GlobalSettings.Culture));
        }

        
        Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<string> keys) where T : KVStoreModel, new();

        
        Task<IEnumerable<T?>> GetAllAsync<T>() where T : KVStoreModel, new();

        
        Task AddAsync<T>(T item, string lastUser) where T : KVStoreModel, new();

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task AddAsync<T>(IEnumerable<T> items, string lastUser) where T : KVStoreModel, new();

        
        Task UpdateAsync<T>(T item, string lastUser) where T : KVStoreModel, new();

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task UpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : KVStoreModel, new();

        
        Task DeleteAsync<T>(string key, long timestamp) where T : KVStoreModel, new();

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        Task DeleteAsync<T>(IEnumerable<string> keys, IEnumerable<long> timestamps) where T : KVStoreModel, new();


        
        Task DeleteAllAsync<T>() where T : KVStoreModel, new();


        string GetModelKey<T>(T item) where T : KVStoreModel, new();
    }
}
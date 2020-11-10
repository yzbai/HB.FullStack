﻿using HB.Framework.Common.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace HB.Framework.KVStore
{
    public interface IKVStore
    {
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task<T?> GetByKeyAsync<T>(object keyValue) where T : Entity, new();

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task<T?> GetByKeyAsync<T>(T t) where T : Entity, new();

        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task<IEnumerable<T?>> GetByKeysAsync<T>(IEnumerable<object> keyValues) where T : Entity, new();

        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task<IEnumerable<T?>> GetByKeysAsync<T>(IEnumerable<T> ts) where T : Entity, new();

        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task<IEnumerable<T?>> GetAllAsync<T>() where T : Entity, new();

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task AddAsync<T>(T item) where T : Entity, new();

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task AddAsync<T>(IEnumerable<T> items) where T : Entity, new();

        /// <summary>
        /// item的Version会被改变
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        Task UpdateAsync<T>(T item) where T : Entity, new();

        /// <summary>
        /// item的Version会被改变
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        Task UpdateAsync<T>(IEnumerable<T> items) where T : Entity, new();

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task DeleteAsync<T>(T item) where T : Entity, new();

        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task DeleteAllAsync<T>() where T : Entity, new();

        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task DeleteByKeyAsync<T>(object keyValue, int version) where T : Entity, new();

        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        Task DeleteByKeysAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : Entity, new();
    }
}
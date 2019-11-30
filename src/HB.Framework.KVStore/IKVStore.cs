using HB.Framework.KVStore.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace HB.Framework.KVStore
{
    public interface IKVStore : IKVStoreAsync
    {
        T GetByKey<T>(object keyValue) where T : KVStoreEntity, new();

        T GetByKey<T>(T t) where T : KVStoreEntity, new();

        IEnumerable<T> GetByKeys<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new();

        IEnumerable<T> GetByKeys<T>(IEnumerable<T> ts) where T : KVStoreEntity, new();

        IEnumerable<T> GetAll<T>() where T : KVStoreEntity, new();

        void Add<T>(T item) where T : KVStoreEntity, new();

        void Add<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        void Update<T>(T item) where T : KVStoreEntity, new();

        void Update<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        void Delete<T>(T item) where T : KVStoreEntity, new();

        void DeleteAll<T>() where T : KVStoreEntity, new();

        void DeleteByKey<T>(object keyValue, int version) where T : KVStoreEntity, new();

        void DeleteByKeys<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new();
    }
}
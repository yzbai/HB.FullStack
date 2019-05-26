using HB.Framework.KVStore.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace HB.Framework.KVStore
{
    public interface IKVStore : IKVStoreAsync
    {
        T GetByGuid<T>(object keyValue) where T : KVStoreEntity, new();

        T GetByGuid<T>(T t) where T : KVStoreEntity, new();

        IEnumerable<T> GetByGuids<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new();

        IEnumerable<T> GetByGuids<T>(IEnumerable<T> ts) where T : KVStoreEntity, new();

        IEnumerable<T> GetAll<T>() where T : KVStoreEntity, new();

        KVStoreResult Add<T>(T item) where T : KVStoreEntity, new();

        KVStoreResult Add<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        KVStoreResult Update<T>(T item) where T : KVStoreEntity, new();

        KVStoreResult Update<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        KVStoreResult Delete<T>(T item) where T : KVStoreEntity, new();

        KVStoreResult DeleteAll<T>() where T : KVStoreEntity, new();

        KVStoreResult DeleteByGuid<T>(object keyValue, int version) where T : KVStoreEntity, new();

        KVStoreResult DeleteByGuids<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new();
    }
}
using HB.Framework.KVStore.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace HB.Framework.KVStore
{
    public interface IKVStore : IKVStoreAsync
    {
        T GetById<T>(object keyValue) where T : KVStoreEntity, new();

        IEnumerable<T> GetByIds<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new();

        IEnumerable<T> GetAll<T>() where T : KVStoreEntity, new();

        KVStoreResult Add<T>(T item) where T : KVStoreEntity, new();

        KVStoreResult Add<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        KVStoreResult Update<T>(T item) where T : KVStoreEntity, new();

        KVStoreResult Update<T>(IEnumerable<T> items) where T : KVStoreEntity, new();

        KVStoreResult Delete<T>(T item) where T : KVStoreEntity, new();

        KVStoreResult DeleteAll<T>() where T : KVStoreEntity, new();

        KVStoreResult DeleteById<T>(object keyValue, int version) where T : KVStoreEntity, new();

        KVStoreResult DeleteByIds<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new();
    }
}
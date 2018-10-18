using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.Framework.KVStore.Entity;
using HB.Framework.KVStore.Engine;
using Microsoft.Extensions.Options;
using HB.Framework.Common;
using HB.Framework.Common.Entity;

namespace HB.Framework.KVStore
{
    public class DefaultKVStore : IKVStore
    {
        private readonly KVStoreOptions _options;
        private IKVStoreEngine _engine;
        private IKVStoreEntityDefFactory _entityDefFactory;

        public DefaultKVStore(IOptions<KVStoreOptions> options, IKVStoreEngine kvstoreEngine, IKVStoreEntityDefFactory kvstoreEntityDefFactory)
        {
            _options = options.Value;
            _engine = kvstoreEngine;
            _entityDefFactory = kvstoreEntityDefFactory;
        }

        #region Private

        private static int entityVersion<T>(T item) where T : KVStoreEntity, new()
        {
            return item.Version;
        }

        private static string storeName(KVStoreEntityDef entityDef)
        {
            return entityDef.KVStoreName;
        }

        private static int storeIndex(KVStoreEntityDef entityDef)
        {
            return entityDef.KVStoreIndex;
        }

        private static string entityName(KVStoreEntityDef entityDef)
        {
            return entityDef.EntityFullName;
        }

        private static string entityKey(object keyValue)
        {
            return DataConverter.GetObjectValueStringStatement(keyValue);
        }

        private static string entityKey<T>(T item, KVStoreEntityDef entityDef) where T : KVStoreEntity, new()
        {
            return DataConverter.GetObjectValueStringStatement(entityDef.KeyPropertyInfo.GetValue(item));
        }

        private static IEnumerable<string> entityKey<T>(IEnumerable<T> items, KVStoreEntityDef entityDef) where T : KVStoreEntity, new()
        {
            return items.Select(t => DataConverter.GetObjectValueStringStatement(entityDef.KeyPropertyInfo.GetValue(t)));
        }

        private static IEnumerable<string> entityKey(IEnumerable<object> keyValues)
        {
            return keyValues.Select(obj => DataConverter.GetObjectValueStringStatement(obj));
        }

        private static byte[] entityValue<T>(T item) where T : KVStoreEntity, new()
        {
            return DataConverter.Serialize<T>(item);
        }

        private static IEnumerable<byte[]> entityValue<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            return items.Select(t => DataConverter.Serialize<T>(t));
        }

        private static T deSerialize<T>(byte[] value) where T : KVStoreEntity, new()
        {
            return DataConverter.DeSerialize<T>(value);
        }

        private static IEnumerable<T> deSerialize<T>(IEnumerable<byte[]> values) where T : KVStoreEntity, new()
        {
            return values?.Select(bytes => DataConverter.DeSerialize<T>(bytes));
        }

        private bool checkEntities<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (items == null || items.Count() == 0)
            {
                return true;
            }

            return items.All(t => t.IsValid());
        }

        private bool checkEntityVersions<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (items == null || items.Count() == 0)
            {
                return true;
            }

            return items.All(t => t.Version == 0);
        }

        #endregion

        #region sync

        public T GetById<T>(object keyValue) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            byte[] value = _engine.EntityGet(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(keyValue));

            return deSerialize<T>(value);
        }

        public IEnumerable<T> GetByIds<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            IEnumerable<byte[]> values = _engine.EntityGet(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(keyValues));

            return deSerialize<T>(values);
        }

        public IEnumerable<T> GetAll<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            IEnumerable<byte[]> values = _engine.EntityGetAll(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef));

            return deSerialize<T>(values);
        }

        public KVStoreResult Add<T>(T item) where T : KVStoreEntity, new()
        {
            return Add<T>(new T[] { item });
        }

        public KVStoreResult Add<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!checkEntities<T>(items))
            {
                return KVStoreResult.Fail("item is not valid.");
            }

            if (!checkEntityVersions<T>(items))
            {
                return KVStoreResult.Fail("item wanted to be added, version should be 0.");
            }

            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityAdd(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(items, entityDef),
                entityValue(items)
                );
        }

        public KVStoreResult Update<T>(T item) where T : KVStoreEntity, new()
        {
            return Update<T>(new T[] { item });
        }

        public KVStoreResult Update<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!checkEntities<T>(items))
            {
                return KVStoreResult.Fail("items is not valid.");
            }

            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            IEnumerable<int> originalVersions = items.Select(t => t.Version).ToArray();

            foreach (T item in items)
            {
                item.Version++;
            }

            KVStoreResult result = _engine.EntityUpdate(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(items, entityDef),
                entityValue(items),
                originalVersions
                );

            if (!result.IsSucceeded())
            {
                foreach (T item in items)
                {
                    item.Version--;
                }
            }

            return result;
        }

        public KVStoreResult Delete<T>(T item) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return DeleteByIds<T>(new object[] { entityKey(item, entityDef) }, new int[] { item.Version });
        }

        public KVStoreResult DeleteAll<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityDeleteAll(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef)
                );
        }

        public KVStoreResult DeleteById<T>(object keyValue, int version) where T : KVStoreEntity, new()
        {
            return DeleteByIds<T>(new List<object>() { keyValue }, new List<int> { version });
        }

        public KVStoreResult DeleteByIds<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityDelete(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(keyValues),
                versions
                );
        }

        #endregion

        #region async

        public Task<T> GetByIdAsync<T>(object keyValue) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityGetAsync(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(keyValue))
                .ContinueWith(t=>deSerialize<T>(t.Result), TaskScheduler.Default);
        }

        public Task<IEnumerable<T>> GetByIdsAsync<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityGetAsync(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(keyValues))
                .ContinueWith(t=>deSerialize<T>(t.Result), TaskScheduler.Default);
        }

        public Task<IEnumerable<T>> GetAllAsync<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityGetAllAsync(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef))
                .ContinueWith(t=>deSerialize<T>(t.Result), TaskScheduler.Default);
        }

        public Task<KVStoreResult> AddAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return AddAsync<T>(new T[] { item });
        }

        public Task<KVStoreResult> AddAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!checkEntities<T>(items))
            {
                return Task.FromResult(KVStoreResult.Fail("item is not valid."));
            }

            if (!checkEntityVersions<T>(items))
            {
                return Task.FromResult(KVStoreResult.Fail("item wanted to be added, version should be 0."));
            }

            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return  _engine.EntityAddAsync(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(items, entityDef),
                entityValue(items)
                );
        }

        public Task<KVStoreResult> UpdateAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return UpdateAsync<T>(new T[] { item });
        }

        public Task<KVStoreResult> UpdateAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!checkEntities<T>(items))
            {
                return Task.FromResult(KVStoreResult.Fail("items is not valid."));
            }

            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            IEnumerable<int> originalVersions = items.Select(t => t.Version).ToArray();

            foreach (T item in items)
            {
                item.Version++;
            }

            return _engine.EntityUpdateAsync(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(items, entityDef),
                entityValue(items),
                originalVersions)
                .ContinueWith(t=> {
                    if (!t.Result.IsSucceeded())
                    {
                        foreach (T item in items)
                        {
                            item.Version--;
                        }
                    }

                    return t.Result;
                }, TaskScheduler.Default);
        }

        public Task<KVStoreResult> DeleteAsync<T>(T item) where T : KVStoreEntity, new()
        {
            if (!item.IsValid())
            {
                return Task.FromResult(KVStoreResult.Fail("item is not valid."));
            }

            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return DeleteByIdsAsync<T>(new object[] { entityKey(item, entityDef) }, new int[] { item.Version });
        }

        public Task<KVStoreResult> DeleteAllAsync<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityDeleteAllAsync(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef)
                );
        }

        public Task<KVStoreResult> DeleteByIdAsync<T>(object keyValue, int version) where T : KVStoreEntity, new()
        {
            return DeleteByIdsAsync<T>(new object[] { keyValue }, new int[] { version });
        }

        public Task<KVStoreResult> DeleteByIdsAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.Get<T>();

            return _engine.EntityDeleteAsync(
                storeName(entityDef),
                storeIndex(entityDef),
                entityName(entityDef),
                entityKey(keyValues),
                versions
                );
        }

        #endregion
    }
}

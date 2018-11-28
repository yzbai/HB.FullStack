using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.Framework.KVStore.Entity;
using HB.Framework.KVStore.Engine;
using Microsoft.Extensions.Options;
using HB.Framework.Common;
using HB.Framework.Common.Entity;
using System.Text;

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

        private static int EntityVersion<T>(T item) where T : KVStoreEntity, new()
        {
            return item.Version;
        }

        private static string StoreName(KVStoreEntityDef entityDef)
        {
            return entityDef.KVStoreName;
        }

        private static int StoreIndex(KVStoreEntityDef entityDef)
        {
            return entityDef.KVStoreIndex;
        }

        private static string EntityName(KVStoreEntityDef entityDef)
        {
            return entityDef.EntityFullName;
        }

        private static string EntityKey(object keyValue)
        {
            return DataConverter.GetObjectValueStringStatement(keyValue);
        }

        private static string EntityKey<T>(T item, KVStoreEntityDef entityDef) where T : KVStoreEntity, new()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < entityDef.KeyPropertyInfos.Count; ++i)
            {
                builder.Append(DataConverter.GetObjectValueStringStatement(entityDef.KeyPropertyInfos[i].GetValue(item)));
                builder.Append("_");
            }

            return builder.ToString();
        }

        private static IEnumerable<string> EntityKey<T>(IEnumerable<T> items, KVStoreEntityDef entityDef) where T : KVStoreEntity, new()
        {
            return items.Select(t=>EntityKey(t, entityDef));
        }

        private static IEnumerable<string> EntityKey(IEnumerable<object> keyValues)
        {
            return keyValues.Select(obj => DataConverter.GetObjectValueStringStatement(obj));
        }

        private static byte[] EntityValue<T>(T item) where T : KVStoreEntity, new()
        {
            return DataConverter.Serialize<T>(item);
        }

        private static IEnumerable<byte[]> EntityValue<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            return items.Select(t => DataConverter.Serialize<T>(t));
        }

        private static T DeSerialize<T>(byte[] value) where T : KVStoreEntity, new()
        {
            return DataConverter.DeSerialize<T>(value);
        }

        private static IEnumerable<T> DeSerialize<T>(IEnumerable<byte[]> values) where T : KVStoreEntity, new()
        {
            return values?.Select(bytes => DataConverter.DeSerialize<T>(bytes));
        }

        private static bool CheckEntities<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (items == null || items.Count() == 0)
            {
                return true;
            }

            return items.All(t => t.IsValid());
        }

        private static bool CheckEntityVersions<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
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
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            byte[] value = _engine.EntityGet(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(keyValue));

            return DeSerialize<T>(value);
        }

        public IEnumerable<T> GetByIds<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<byte[]> values = _engine.EntityGet(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues));

            return DeSerialize<T>(values);
        }

        public IEnumerable<T> GetAll<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<byte[]> values = _engine.EntityGetAll(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef));

            return DeSerialize<T>(values);
        }

        public KVStoreResult Add<T>(T item) where T : KVStoreEntity, new()
        {
            return Add<T>(new T[] { item });
        }

        public KVStoreResult Add<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!CheckEntities<T>(items))
            {
                return KVStoreResult.Fail("item is not valid.");
            }

            if (!CheckEntityVersions<T>(items))
            {
                return KVStoreResult.Fail("item wanted to be added, version should be 0.");
            }

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityAdd(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(items, entityDef),
                EntityValue(items)
                );
        }

        public KVStoreResult Update<T>(T item) where T : KVStoreEntity, new()
        {
            return Update<T>(new T[] { item });
        }

        public KVStoreResult Update<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!CheckEntities<T>(items))
            {
                return KVStoreResult.Fail("items is not valid.");
            }

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<int> originalVersions = items.Select(t => t.Version).ToArray();

            foreach (T item in items)
            {
                item.Version++;
            }

            KVStoreResult result = _engine.EntityUpdate(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(items, entityDef),
                EntityValue(items),
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
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return DeleteByIds<T>(new object[] { EntityKey(item, entityDef) }, new int[] { item.Version });
        }

        public KVStoreResult DeleteAll<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityDeleteAll(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef)
                );
        }

        public KVStoreResult DeleteById<T>(object keyValue, int version) where T : KVStoreEntity, new()
        {
            return DeleteByIds<T>(new List<object>() { keyValue }, new List<int> { version });
        }

        public KVStoreResult DeleteByIds<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityDelete(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues),
                versions
                );
        }

        #endregion

        #region async

        public Task<T> GetByIdAsync<T>(object keyValue) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityGetAsync(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(keyValue))
                .ContinueWith(t=>DeSerialize<T>(t.Result), TaskScheduler.Default);
        }

        public Task<IEnumerable<T>> GetByIdsAsync<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityGetAsync(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues))
                .ContinueWith(t=>DeSerialize<T>(t.Result), TaskScheduler.Default);
        }

        public Task<IEnumerable<T>> GetAllAsync<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityGetAllAsync(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef))
                .ContinueWith(t=>DeSerialize<T>(t.Result), TaskScheduler.Default);
        }

        public Task<KVStoreResult> AddAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return AddAsync<T>(new T[] { item });
        }

        public Task<KVStoreResult> AddAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!CheckEntities<T>(items))
            {
                return Task.FromResult(KVStoreResult.Fail("item is not valid."));
            }

            if (!CheckEntityVersions<T>(items))
            {
                return Task.FromResult(KVStoreResult.Fail("item wanted to be added, version should be 0."));
            }

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return  _engine.EntityAddAsync(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(items, entityDef),
                EntityValue(items)
                );
        }

        public Task<KVStoreResult> UpdateAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return UpdateAsync<T>(new T[] { item });
        }

        public Task<KVStoreResult> UpdateAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (!CheckEntities<T>(items))
            {
                return Task.FromResult(KVStoreResult.Fail("items is not valid."));
            }

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<int> originalVersions = items.Select(t => t.Version).ToArray();

            foreach (T item in items)
            {
                item.Version++;
            }

            return _engine.EntityUpdateAsync(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(items, entityDef),
                EntityValue(items),
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

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return DeleteByIdsAsync<T>(new object[] { EntityKey(item, entityDef) }, new int[] { item.Version });
        }

        public Task<KVStoreResult> DeleteAllAsync<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityDeleteAllAsync(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef)
                );
        }

        public Task<KVStoreResult> DeleteByIdAsync<T>(object keyValue, int version) where T : KVStoreEntity, new()
        {
            return DeleteByIdsAsync<T>(new object[] { keyValue }, new int[] { version });
        }

        public Task<KVStoreResult> DeleteByIdsAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _engine.EntityDeleteAsync(
                StoreName(entityDef),
                StoreIndex(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues),
                versions
                );
        }

        #endregion
    }
}

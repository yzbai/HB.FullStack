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
            int count = entityDef.KeyPropertyInfos.Count;

            for (int i = 0; i < count; ++i)
            {
                builder.Append(DataConverter.GetObjectValueStringStatement(entityDef.KeyPropertyInfos[i].GetValue(item)));

                if (i != count - 1)
                {
                    builder.Append("_");
                }
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

       

        

        //private static T DeSerialize<T>(byte[] value) where T : KVStoreEntity, new()
        //{
        //    return DataConverter.DeSerialize<T>(value);
        //}

        //private static IEnumerable<T> DeSerialize<T>(IEnumerable<byte[]> values) where T : KVStoreEntity, new()
        //{
        //    return values?.Select(bytes => DataConverter.DeSerialize<T>(bytes));
        //}

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

            string jsonValue = _engine.EntityGet(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(keyValue));

            return DataConverter.FromJson<T>(jsonValue);
        }

        public T GetById<T>(T t) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            string jsonValue = _engine.EntityGet(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(t, entityDef));

            return DataConverter.FromJson<T>(jsonValue);
        }

        public IEnumerable<T> GetByIds<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<string> values = _engine.EntityGet(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues));

            return values.Select(json => DataConverter.FromJson<T>(json));
        }

        public IEnumerable<T> GetByIds<T>(IEnumerable<T> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<string> values = _engine.EntityGet(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues, entityDef));

            return values.Select(json => DataConverter.FromJson<T>(json));
        }

        public IEnumerable<T> GetAll<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<string> values = _engine.EntityGetAll(
                StoreName(entityDef),
                EntityName(entityDef));

            return values.Select(json => DataConverter.FromJson<T>(json));
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
                EntityName(entityDef),
                EntityKey(items, entityDef),
                items.Select(t=>DataConverter.ToJson(t))
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
                EntityName(entityDef),
                EntityKey(items, entityDef),
                items.Select(t=>DataConverter.ToJson(t)),
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
                EntityName(entityDef),
                EntityKey(keyValues),
                versions
                );
        }

        #endregion

        #region async

        public async Task<T> GetByIdAsync<T>(object keyValue) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            string json = await _engine.EntityGetAsync(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(keyValue)).ConfigureAwait(false);

            return DataConverter.FromJson<T>(json);
        }

        public async Task<T> GetByIdAsync<T>(T t) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            string json = await _engine.EntityGetAsync(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(t, entityDef)).ConfigureAwait(false);

            return DataConverter.FromJson<T>(json);
        }

        public async Task<IEnumerable<T>> GetByIdsAsync<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<string> jsons = await _engine.EntityGetAsync(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues)).ConfigureAwait(false);

            return jsons.Select(t => DataConverter.FromJson<T>(t));
        }

        public async Task<IEnumerable<T>> GetByIdsAsync<T>(IEnumerable<T> keyValues) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<string> jsons = await _engine.EntityGetAsync(
                StoreName(entityDef),
                EntityName(entityDef),
                EntityKey(keyValues, entityDef)).ConfigureAwait(false);

            return jsons.Select(t => DataConverter.FromJson<T>(t));
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            IEnumerable<string> jsons = await _engine.EntityGetAllAsync(
                StoreName(entityDef),
                EntityName(entityDef)).ConfigureAwait(false);

            return jsons.Select(t => DataConverter.FromJson<T>(t));
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
                EntityName(entityDef),
                EntityKey(items, entityDef),
                items.Select(t=>DataConverter.ToJson(t))
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
                EntityName(entityDef),
                EntityKey(items, entityDef),
                items.Select(t=>DataConverter.ToJson(t)),
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
                EntityName(entityDef),
                EntityKey(keyValues),
                versions
                );
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Common.Entities;
using HB.Framework.KVStore.Engine;
using HB.Framework.KVStore.Entities;
using HB.Framework.KVStore.Properties;

namespace HB.Framework.KVStore
{
    internal partial class DefaultKVStore : IKVStore
    {
        private readonly IKVStoreEngine _engine;
        private readonly IKVStoreEntityDefFactory _entityDefFactory;

        public DefaultKVStore(IKVStoreEngine kvstoreEngine, IKVStoreEntityDefFactory kvstoreEntityDefFactory)
        {
            _engine = kvstoreEngine;
            _entityDefFactory = kvstoreEntityDefFactory;
        }

        #region Private

        private static string StoreName(KVStoreEntityDef entityDef)
        {
            return entityDef.KVStoreName;
        }

        private static string EntityName(KVStoreEntityDef entityDef)
        {
            return entityDef.EntityType.FullName;
        }

        private static string EntityKey(object keyValue)
        {
            return ValueConverterUtil.TypeValueToStringValue(keyValue)!;
        }

        private static string EntityKey<T>(T item, KVStoreEntityDef entityDef) where T : Entity, new()
        {
            StringBuilder builder = new StringBuilder();
            int count = entityDef.KeyPropertyInfos.Count;

            for (int i = 0; i < count; ++i)
            {
                builder.Append(ValueConverterUtil.TypeValueToStringValue(entityDef.KeyPropertyInfos[i].GetValue(item)));

                if (i != count - 1)
                {
                    builder.Append('_');
                }
            }

            return builder.ToString();
        }

        private static IEnumerable<string> EntityKey<T>(IEnumerable<T> items, KVStoreEntityDef entityDef) where T : Entity, new()
        {
            return items.Select(t => EntityKey(t, entityDef));
        }

        private static IEnumerable<string> EntityKey(IEnumerable<object> keyValues)
        {
            return keyValues.Select(obj => ValueConverterUtil.TypeValueToStringValue(obj)!);
        }

        #endregion

        public async Task<T?> GetByKeyAsync<T>(object keyValue) where T : Entity, new()
        {
            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                string json = await _engine.EntityGetAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(keyValue)).ConfigureAwait(false);

                return SerializeUtil.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }

                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Key:{EntityKey(keyValue)}", ex);
            }
        }

        public async Task<T?> GetByKeyAsync<T>(T t) where T : Entity, new()
        {
            ThrowIf.NotValid(t);

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                string json = await _engine.EntityGetAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(t, entityDef)).ConfigureAwait(false);

                return SerializeUtil.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }

                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Key:{SerializeUtil.ToJson(t)}", ex);
            }
        }

        public async Task<IEnumerable<T?>> GetByKeysAsync<T>(IEnumerable<object> keyValues) where T : Entity, new()
        {
            //ThrowIf.AnyNull(keyValues, nameof(keyValues));

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                IEnumerable<string> jsons = await _engine.EntityGetAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(keyValues)).ConfigureAwait(false);

                return jsons.Select(t => SerializeUtil.FromJson<T>(t));
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }

                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Key:{SerializeUtil.ToJson(EntityKey(keyValues))}", ex);
            }
        }

        public async Task<IEnumerable<T?>> GetByKeysAsync<T>(IEnumerable<T> keyValues) where T : Entity, new()
        {
            //ThrowIf.AnyNull(keyValues, nameof(keyValues));
            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                IEnumerable<string> jsons = await _engine.EntityGetAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(keyValues, entityDef)).ConfigureAwait(false);

                return jsons.Select(t => SerializeUtil.FromJson<T>(t));
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }
                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"key:{SerializeUtil.ToJson(keyValues)}", ex);
            }
        }

        public async Task<IEnumerable<T?>> GetAllAsync<T>() where T : Entity, new()
        {
            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                IEnumerable<string> jsons = await _engine.EntityGetAllAsync(
                    StoreName(entityDef),
                    EntityName(entityDef)).ConfigureAwait(false);

                return jsons.Select(t => SerializeUtil.FromJson<T>(t));
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }

                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, null, ex);
            }
        }

        public Task AddAsync<T>(T item, string lastUser) where T : Entity, new()
        {
            return AddAsync<T>(new T[] { item }, lastUser);
        }

        /// <summary>
        /// 反应Version变化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        public async Task AddAsync<T>(IEnumerable<T> items, string lastUser) where T : Entity, new()
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items);

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                await _engine.EntityAddAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(items, entityDef),
                    items.Select(t =>
                    {
                        t.LastUser = lastUser;
                        return SerializeUtil.ToJson(t);
                    })
                    ).ConfigureAwait(false);

                //version 变化
                foreach (T item in items)
                {
                    item.Version = 0;
                }
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }

                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(items)}", ex);
            }
        }

        public Task UpdateAsync<T>(T item, string lastUser) where T : Entity, new()
        {
            return UpdateAsync<T>(new T[] { item }, lastUser);
        }


        /// <summary>
        /// 反应Version变化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        public async Task UpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : Entity, new()
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items);

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                IEnumerable<int> originalVersions = items.Select(t => t.Version).ToArray();

                foreach (T item in items)
                {
                    item.LastUser = lastUser;
                }

                await _engine.EntityUpdateAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(items, entityDef),
                    items.Select(t =>
                    {
                        return SerializeUtil.ToJson(t);
                    }),
                    originalVersions).ConfigureAwait(false);


                //反应Version变化
                foreach (T item in items)
                {
                    item.Version++;
                }
            }

            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(items)}", ex);
            }
        }

        public Task DeleteAsync<T>(T item) where T : Entity, new()
        {
            ThrowIf.NotValid(item);

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return DeleteByKeysAsync<T>(new object[] { EntityKey(item, entityDef) }, new int[] { item.Version });
        }

        public async Task DeleteAllAsync<T>() where T : Entity, new()
        {
            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                await _engine.EntityDeleteAllAsync(
                   StoreName(entityDef),
                   EntityName(entityDef)
                   ).ConfigureAwait(false);
            }
            catch (KVStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }

                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, null, ex);
            }
        }

        public Task DeleteByKeyAsync<T>(object keyValue, int version) where T : Entity, new()
        {
            return DeleteByKeysAsync<T>(new object[] { keyValue }, new int[] { version });
        }

        public async Task DeleteByKeysAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : Entity, new()
        {
            ThrowIf.NullOrEmpty(versions, nameof(versions));

            if (keyValues.Count() != versions.Count())
            {
                throw new ArgumentException(Resources.VersionsKeysNotEqualErrorMessage);
            }

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                await _engine.EntityDeleteAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(keyValues),
                    versions
                    ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is KVStoreException)
                {
                    throw;
                }

                KVStoreException exception = new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"keyValues:{SerializeUtil.ToJson(keyValues)}, versions:{SerializeUtil.ToJson(versions)}", ex);


                throw exception;
            }
        }

        public Task<int> AddOrUpdateAsync<T>(T item, string lastUser) where T : Entity, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 返回最新的Versions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        public async Task<IEnumerable<int>> AddOrUpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : Entity, new()
        {
            if (!items.Any())
            {
                return new List<int>();
            }

            ThrowIf.NotValid(items);

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                IEnumerable<int> versions = await _engine.EntityAddOrUpdateAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(items, entityDef),
                    items.Select(t =>
                    {
                        t.LastUser = lastUser;
                        return SerializeUtil.ToJson(t);
                    })
                    ).ConfigureAwait(false);

                if (items.Count() != versions.Count())
                {
                    throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(items)}");
                }

                for (int i = 0; i < items.Count(); ++i)
                {
                    items.ElementAt(i).Version = versions.ElementAt(i);
                }

                return versions;
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(items)}", ex);
            }
        }
    }
}

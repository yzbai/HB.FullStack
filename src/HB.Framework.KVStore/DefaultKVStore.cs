using HB.Framework.KVStore.Engine;
using HB.Framework.KVStore.Entity;
using HB.Framework.KVStore.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.KVStore
{
    internal partial class DefaultKVStore : IKVStore
    {
        private readonly ILogger _logger;
        private readonly IKVStoreEngine _engine;
        private readonly IKVStoreEntityDefFactory _entityDefFactory;

        public DefaultKVStore(IKVStoreEngine kvstoreEngine, IKVStoreEntityDefFactory kvstoreEntityDefFactory, ILogger<DefaultKVStore> logger)
        {
            _logger = logger;
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
            return ValueConverterUtil.TypeValueToStringValue(keyValue);
        }

        private static string EntityKey<T>(T item, KVStoreEntityDef entityDef) where T : KVStoreEntity, new()
        {
            StringBuilder builder = new StringBuilder();
            int count = entityDef.KeyPropertyInfos.Count;

            for (int i = 0; i < count; ++i)
            {
                builder.Append(ValueConverterUtil.TypeValueToStringValue(entityDef.KeyPropertyInfos[i].GetValue(item)));

                if (i != count - 1)
                {
                    builder.Append("_");
                }
            }

            return builder.ToString();
        }

        private static IEnumerable<string> EntityKey<T>(IEnumerable<T> items, KVStoreEntityDef entityDef) where T : KVStoreEntity, new()
        {
            return items.Select(t => EntityKey(t, entityDef));
        }

        private static IEnumerable<string> EntityKey(IEnumerable<object> keyValues)
        {
            return keyValues.Select(obj => ValueConverterUtil.TypeValueToStringValue(obj));
        }


        private static bool CheckEntityVersions<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            if (items == null || !items.Any())
            {
                return true;
            }

            return items.All(t => t.Version == 0);
        }

        #endregion

        /// <summary>
        /// GetByKeyAsync
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public async Task<T?> GetByKeyAsync<T>(object keyValue) where T : KVStoreEntity, new()
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
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"key:{EntityKey(keyValue)}");

                _logger.LogError(exception, $"DefaultKVStore::GetByKeyAsync, key:{keyValue}");

                throw exception;
            }
        }

        /// <summary>
        /// GetByKeyAsync
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public async Task<T?> GetByKeyAsync<T>(T t) where T : KVStoreEntity, new()
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
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"key:{SerializeUtil.ToJson(t)}");

                _logger.LogError(exception, $"DefaultKVStore::GetByKeyAsync<T>, key:{SerializeUtil.ToJson(t)}");

                throw exception;
            }
        }

        /// <summary>
        /// GetByKeysAsync
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public async Task<IEnumerable<T?>> GetByKeysAsync<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
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
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"key:{SerializeUtil.ToJson(EntityKey(keyValues))}");

                _logger.LogError(exception, $"DefaultKVStore::GetByKeysAsync keys:{string.Join(',', keyValues.ToArray())}");

                throw exception;
            }
        }

        /// <summary>
        /// GetByKeysAsync
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public async Task<IEnumerable<T?>> GetByKeysAsync<T>(IEnumerable<T> keyValues) where T : KVStoreEntity, new()
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
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"key:{SerializeUtil.ToJson(keyValues)}");

                _logger.LogError(exception, $"DefaultKVStore::GetByKeysAsync<T> keys:{SerializeUtil.ToJson(keyValues)}");

                throw exception;
            }
        }

        /// <summary>
        /// GetAllAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public async Task<IEnumerable<T?>> GetAllAsync<T>() where T : KVStoreEntity, new()
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
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"");

                _logger.LogError(exception, $"DefaultKVStore::GetAllAsync,Type:{typeof(T).FullName}");

                throw exception;
            }
        }

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public Task AddAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return AddAsync<T>(new T[] { item });
        }

        /// <summary>
        /// AddAsync
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public Task AddAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            ThrowIf.NotValid(items);

            if (!CheckEntityVersions<T>(items))
            {
                throw new KVStoreException(KVStoreError.VersionNotMatched, typeof(T).FullName, Resources.AddedItemVersionErrorMessage);
            }

            try
            {

                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                return _engine.EntityAddAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(items, entityDef),
                    items.Select(t => SerializeUtil.ToJson(t))
                    );
            }
            catch (Exception ex)
            {
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(items)}");

                _logger.LogError(exception, "");

                throw exception;
            }
        }

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public Task UpdateAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return UpdateAsync<T>(new T[] { item });
        }

        /// <summary>
        /// UpdateAsync
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public async Task UpdateAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            ThrowIf.NotValid(items);
            bool versionChanged = false;

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                IEnumerable<int> originalVersions = items.Select(t => t.Version).ToArray();

                foreach (T item in items)
                {
                    item.Version++;
                }

                versionChanged = true;

                await _engine.EntityUpdateAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(items, entityDef),
                    items.Select(t => SerializeUtil.ToJson(t)),
                    originalVersions).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (versionChanged)
                {
                    foreach (T item in items)
                    {
                        item.Version--;
                    }
                }

                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(items)}");

                _logger.LogError(exception, "");

                throw exception;
            }
        }

        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public Task DeleteAsync<T>(T item) where T : KVStoreEntity, new()
        {
            ThrowIf.NotValid(item);

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return DeleteByKeysAsync<T>(new object[] { EntityKey(item, entityDef) }, new int[] { item.Version });
        }


        /// <summary>
        /// DeleteAllAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        public Task DeleteAllAsync<T>() where T : KVStoreEntity, new()
        {
            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                return _engine.EntityDeleteAllAsync(
                    StoreName(entityDef),
                    EntityName(entityDef)
                    );
            }
            catch (Exception ex)
            {
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"");

                _logger.LogError(exception, "");

                throw exception;
            }
        }

        public Task DeleteByKeyAsync<T>(object keyValue, int version) where T : KVStoreEntity, new()
        {
            return DeleteByKeysAsync<T>(new object[] { keyValue }, new int[] { version });
        }

        /// <summary>
        /// DeleteByKeysAsync
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="versions"></param>
        /// <returns></returns>
        /// <exception cref="HB.Framework.KVStore.KVStoreException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public Task DeleteByKeysAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new()
        {
            ThrowIf.NullOrEmpty(versions, nameof(versions));

            if (keyValues.Count() != versions.Count())
            {
                throw new ArgumentException(Resources.VersionsKeysNotEqualErrorMessage);
            }

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                return _engine.EntityDeleteAsync(
                    StoreName(entityDef),
                    EntityName(entityDef),
                    EntityKey(keyValues),
                    versions
                    );
            }
            catch (Exception ex)
            {
                KVStoreException exception = new KVStoreException(ex, typeof(T).FullName, $"keyValues:{SerializeUtil.ToJson(keyValues)}, versions:{SerializeUtil.ToJson(versions)}");

                _logger.LogError(exception, "");

                throw exception;
            }
        }

    }
}

using HB.Framework.KVStore.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.KVStore
{
    internal partial class DefaultKVStore
    {
        #region async

        public async Task<T> GetByKeyAsync<T>(object keyValue) where T : KVStoreEntity, new()
        {
            ThrowIf.Null(keyValue, nameof(keyValue));

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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public async Task<T> GetByKeyAsync<T>(T t) where T : KVStoreEntity, new()
        {
            ThrowIf.NullOrNotValid(t, nameof(t));
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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public async Task<IEnumerable<T>> GetByKeysAsync<T>(IEnumerable<object> keyValues) where T : KVStoreEntity, new()
        {
            ThrowIf.AnyNull(keyValues, nameof(keyValues));

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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public async Task<IEnumerable<T>> GetByKeysAsync<T>(IEnumerable<T> keyValues) where T : KVStoreEntity, new()
        {
            ThrowIf.AnyNull(keyValues, nameof(keyValues));
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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : KVStoreEntity, new()
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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public Task AddAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return AddAsync<T>(new T[] { item });
        }

        public Task AddAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            ThrowIf.NullOrNotValid(items, nameof(items));

            if (!CheckEntityVersions<T>(items))
            {
                throw new KVStoreException(KVStoreError.VersionNotMatched, typeof(T).FullName, "item wanted to be added, version should be 0.");
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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public Task UpdateAsync<T>(T item) where T : KVStoreEntity, new()
        {
            return UpdateAsync<T>(new T[] { item });
        }

        public async Task UpdateAsync<T>(IEnumerable<T> items) where T : KVStoreEntity, new()
        {
            ThrowIf.NullOrNotValid(items, nameof(items));
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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public Task DeleteAsync<T>(T item) where T : KVStoreEntity, new()
        {
            ThrowIf.NullOrNotValid(item, nameof(item));

            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return DeleteByKeysAsync<T>(new object[] { EntityKey(item, entityDef) }, new int[] { item.Version });
        }

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

                _logger.LogException(exception);

                throw exception;
            }
        }

        public Task DeleteByKeyAsync<T>(object keyValue, int version) where T : KVStoreEntity, new()
        {
            return DeleteByKeysAsync<T>(new object[] { keyValue }, new int[] { version });
        }

        public Task DeleteByKeysAsync<T>(IEnumerable<object> keyValues, IEnumerable<int> versions) where T : KVStoreEntity, new()
        {
            ThrowIf.AnyNull(keyValues, nameof(keyValues));
            ThrowIf.NullOrEmpty(versions, nameof(versions));

            if (keyValues.Count() != versions.Count())
            {
                throw new ArgumentException("versions.count is not equal keyValues.count");
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

                _logger.LogException(exception);

                throw exception;
            }
        }

        #endregion
    }
}

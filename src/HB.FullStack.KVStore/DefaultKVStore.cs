using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using HB.FullStack.KVStore.Engine;
using HB.FullStack.KVStore.Entities;

namespace HB.FullStack.KVStore
{
    internal partial class DefaultKVStore : IKVStore
    {
        private readonly IKVStoreEngine _engine;

        public DefaultKVStore(IKVStoreEngine kvstoreEngine)
        {
            _engine = kvstoreEngine;
            EntityDefFactory.Initialize(kvstoreEngine);
        }

        private static string GetEntityKey<T>(T item, KVStoreEntityDef entityDef) where T : KVStoreEntity, new()
        {
            StringBuilder builder = new StringBuilder();

            int count = entityDef.KeyPropertyInfos.Count;

            for (int i = 0; i < count; ++i)
            {
                builder.Append(entityDef.KeyPropertyInfos[i].GetValue(item));

                if (i != count - 1)
                {
                    builder.Append('_');
                }
            }

            return builder.ToString();


        }

        public string GetEntityKey<T>(T item) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = EntityDefFactory.GetDef<T>();

            return GetEntityKey(item, entityDef);
        }

        /// <summary>
        /// 反应Version变化
        /// </summary>
        public async Task<T?> GetAsync<T>(string key) where T : KVStoreEntity, new()
        {
            IEnumerable<T?> ts = await GetAsync<T>(new string[] { key }).ConfigureAwait(false);

            return ts.Any() ? ts.ElementAt(0) : null;
        }

        /// <summary>
        /// 反应Version变化
        /// </summary>
        /// <exception cref="KVStoreException"></exception>
        public async Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<string> keys) where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = EntityDefFactory.GetDef<T>();

            try
            {
                IEnumerable<Tuple<string?, int>> tuples = await _engine.EntityGetAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName!,
                    keys).ConfigureAwait(false);

                return MapTupleToEntity<T>(tuples);
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw Exceptions.Unkown(type: typeof(T).FullName, storeName: entityDef.KVStoreName, key: keys, innerException: ex);
            }
        }

        /// <summary>
        /// 反应Version变化
        /// </summary>
        /// <exception cref="KVStoreException"></exception>
        public async Task<IEnumerable<T?>> GetAllAsync<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = EntityDefFactory.GetDef<T>();
            try
            {
                IEnumerable<Tuple<string?, int>> tuples = await _engine.EntityGetAllAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName!).ConfigureAwait(false);

                return MapTupleToEntity<T>(tuples);
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw Exceptions.Unkown(type: typeof(T).FullName, storeName: entityDef.KVStoreName, key: null, innerException: ex);
            }
        }

        /// <summary>
        /// 反应Version变化
        /// </summary>
        public Task AddAsync<T>(T item, string lastUser) where T : KVStoreEntity, new()
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
        /// <exception cref="KVStoreException"></exception>
        public async Task AddAsync<T>(IEnumerable<T> items, string lastUser) where T : KVStoreEntity, new()
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            KVStoreEntityDef entityDef = EntityDefFactory.GetDef<T>();

            try
            {

                foreach (var t in items)
                {
                    t.LastUser = lastUser;
                    t.LastTime = TimeUtil.UtcNow;
                }

                await _engine.EntityAddAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName!,
                    items.Select(t => GetEntityKey(t, entityDef)),
                    items.Select(t => SerializeUtil.ToJson(t))
                    ).ConfigureAwait(false);

                //version 变化
                foreach (var t in items)
                {
                    t.Version = 0;
                }
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw Exceptions.Unkown(entityDef.EntityType.FullName, entityDef.KVStoreName, items, ex);
            }
        }

        /// <summary>
        /// 反应Version变化
        /// </summary>
        public Task UpdateAsync<T>(T item, string lastUser) where T : KVStoreEntity, new()
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
        /// <exception cref="KVStoreException"></exception>
        public async Task UpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : KVStoreEntity, new()
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            KVStoreEntityDef entityDef = EntityDefFactory.GetDef<T>();

            try
            {

                IEnumerable<int> originalVersions = items.Select(t => t.Version).ToArray();

                foreach (var t in items)
                {
                    t.LastUser = lastUser;
                    t.LastTime = TimeUtil.UtcNow;
                }

                await _engine.EntityUpdateAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName!,
                    items.Select(t => GetEntityKey(t, entityDef)),
                    items.Select(t => SerializeUtil.ToJson(t)),
                    originalVersions).ConfigureAwait(false);

                //反应Version变化
                foreach (var t in items)
                {
                    t.Version++;
                }
            }

            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw Exceptions.Unkown(entityDef.EntityType.FullName, entityDef.KVStoreName, items, ex);
            }
        }

        /// <summary>
        /// DeleteAllAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KVStoreException"></exception>
        public async Task DeleteAllAsync<T>() where T : KVStoreEntity, new()
        {
            KVStoreEntityDef entityDef = EntityDefFactory.GetDef<T>();

            try
            {

                await _engine.EntityDeleteAllAsync(
                   entityDef.KVStoreName,
                   entityDef.EntityType.FullName!
                   ).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw Exceptions.Unkown(entityDef.EntityType.FullName, entityDef.KVStoreName, null, ex);
            }
        }

        public Task DeleteAsync<T>(string key, int version) where T : KVStoreEntity, new()
        {
            return DeleteAsync<T>(new string[] { key }, new int[] { version });
        }

        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="versions"></param>
        /// <returns></returns>
        /// <exception cref="KVStoreException"></exception>
        public async Task DeleteAsync<T>(IEnumerable<string> keys, IEnumerable<int> versions) where T : KVStoreEntity, new()
        {
            ThrowIf.NullOrEmpty(versions, nameof(versions));

            if (keys.Count() != versions.Count())
            {
                throw Exceptions.VersionsKeysNotEqualError();
            }

            KVStoreEntityDef entityDef = EntityDefFactory.GetDef<T>();

            try
            {

                await _engine.EntityDeleteAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName!,
                    keys,
                    versions
                    ).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw Exceptions.Unkown(entityDef.EntityType.FullName, entityDef.KVStoreName, keys: keys, values: versions, innerException: ex);
            }
        }

        private static IEnumerable<T?> MapTupleToEntity<T>(IEnumerable<Tuple<string?, int>> tuples) where T : KVStoreEntity, new()
        {
            List<T?> rt = new List<T?>();

            foreach (var t in tuples)
            {
                T? item = SerializeUtil.FromJson<T>(t.Item1);
                if (item == null)
                {
                    rt.Add(null);
                }
                else
                {
                    item.Version = t.Item2;
                    rt.Add(item);
                }
            }

            return rt;
        }


    }
}

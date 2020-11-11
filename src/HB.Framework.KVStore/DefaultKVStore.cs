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

        public async Task<T?> GetAsync<T>(string guid) where T : Entity, new()
        {
            IEnumerable<T?> ts = await GetAsync<T>(new string[] { guid }).ConfigureAwait(false);

            return ts.Any() ? ts.ElementAt(0) : null;
        }
        public async Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<string> guids) where T : Entity, new()
        {
            KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();
            
            try
            {
                IEnumerable<Tuple<string?, int>> tuples = await _engine.EntityGetAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName,
                    guids).ConfigureAwait(false);

                List<T?> rt = new List<T?>();

                tuples.ForEach(t =>
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
                });

                return rt;
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"StoreName:{entityDef.KVStoreName}, EntityName: { entityDef.EntityType.FullName}, Key:{SerializeUtil.ToJson(guids)}", ex);
            }
        }

        public async Task<IEnumerable<T?>> GetAllAsync<T>() where T : Entity, new()
        {
            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                IEnumerable<string> jsons = await _engine.EntityGetAllAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName).ConfigureAwait(false);

                return jsons.Select(t => SerializeUtil.FromJson<T>(t));
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
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

                items.ForEach(t => t.LastUser = lastUser);

                await _engine.EntityAddAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName,
                    items.Select(t=>t.Guid),
                    items.Select(t =>SerializeUtil.ToJson(t))
                    ).ConfigureAwait(false);

                //version 变化
                items.ForEach(t => t.Version = 0);
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
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

                items.ForEach(t => t.LastUser = lastUser);

                await _engine.EntityUpdateAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName,
                    items.Select(t=>t.Guid),
                    items.Select(t =>SerializeUtil.ToJson(t)),
                    originalVersions).ConfigureAwait(false);

                //反应Version变化
                items.ForEach(t => t.Version++);
            }

            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(items)}", ex);
            }
        }

        public async Task DeleteAllAsync<T>() where T : Entity, new()
        {
            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                await _engine.EntityDeleteAllAsync(
                   entityDef.KVStoreName,
                   entityDef.EntityType.FullName
                   ).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, null, ex);
            }
        }

        public Task DeleteAsync<T>(string guid, int version) where T : Entity, new()
        {
            return DeleteAsync<T>(new string[] { guid}, new int[] { version });
        }

        public async Task DeleteAsync<T>(IEnumerable<string> guids, IEnumerable<int> versions) where T : Entity, new()
        {
            ThrowIf.NullOrEmpty(versions, nameof(versions));

            if (guids.Count() != versions.Count())
            {
                throw new ArgumentException(Resources.VersionsKeysNotEqualErrorMessage);
            }

            try
            {
                KVStoreEntityDef entityDef = _entityDefFactory.GetDef<T>();

                await _engine.EntityDeleteAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName,
                    guids,
                    versions
                    ).ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is KVStoreException))
            {
                throw new KVStoreException(ErrorCode.KVStoreError, typeof(T).FullName, $"keyValues:{SerializeUtil.ToJson(guids)}, versions:{SerializeUtil.ToJson(versions)}", ex);
            }
        }

        /// <summary>
        /// 返回最新Version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        public async Task<int> AddOrUpdateAsync<T>(T item, string lastUser) where T : Entity, new()
        {
            IEnumerable<int> results = await AddOrUpdateAsync(new T[] { item }, lastUser).ConfigureAwait(false);

            if (!results.Any())
            {
                throw new KVStoreException(ErrorCode.KVStoreEntityAddOrUpdateError, typeof(T).FullName, $"Items:{SerializeUtil.ToJson(item)}");
            }

            return results.ElementAt(0);
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

                items.ForEach(t => t.LastUser = lastUser);

                IEnumerable<int> versions = await _engine.EntityAddOrUpdateAsync(
                    entityDef.KVStoreName,
                    entityDef.EntityType.FullName,
                    items.Select(t=>t.Guid),
                    items.Select(t =>SerializeUtil.ToJson(t))
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

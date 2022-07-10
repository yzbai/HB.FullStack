using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.KVStore.Engine;
using HB.FullStack.KVStore.KVStoreModels;

namespace HB.FullStack.KVStore
{
    public class DefaultKVStore : IKVStore
    {
        private readonly IKVStoreEngine _engine;

        public DefaultKVStore(IKVStoreEngine kvstoreEngine)
        {
            _engine = kvstoreEngine;
            KVStoreModelDefFactory.Initialize(kvstoreEngine);
        }

        private static string GetModelKey<T>(T item, KVStoreModelDef modelDef) where T : KVStoreModel, new()
        {
            StringBuilder builder = new StringBuilder();

            int count = modelDef.KeyPropertyInfos.Count;

            for (int i = 0; i < count; ++i)
            {
                builder.Append(modelDef.KeyPropertyInfos[i].GetValue(item));

                if (i != count - 1)
                {
                    builder.Append('_');
                }
            }

            return builder.ToString();
        }

        public string GetModelKey<T>(T item) where T : KVStoreModel, new()
        {
            KVStoreModelDef modelDef = KVStoreModelDefFactory.GetDef<T>();

            return GetModelKey(item, modelDef);
        }

        public async Task<T?> GetAsync<T>(string key) where T : KVStoreModel, new()
        {
            IEnumerable<T?> ts = await GetAsync<T>(new string[] { key }).ConfigureAwait(false);

            return ts.Any() ? ts.ElementAt(0) : null;
        }

        public async Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<string> keys) where T : KVStoreModel, new()
        {
            KVStoreModelDef modelDef = KVStoreModelDefFactory.GetDef<T>();

            try
            {
                IEnumerable<Tuple<string?, long>> tuples = await _engine.ModelGetAsync(
                    modelDef.KVStoreName,
                    modelDef.ModelType.FullName!,
                    keys).ConfigureAwait(false);

                return MapTupleToModel<T>(tuples);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw Exceptions.Unkown(type: typeof(T).FullName, storeName: modelDef.KVStoreName, key: keys, innerException: ex);
            }
        }

        public async Task<IEnumerable<T?>> GetAllAsync<T>() where T : KVStoreModel, new()
        {
            KVStoreModelDef modelDef = KVStoreModelDefFactory.GetDef<T>();
            try
            {
                IEnumerable<Tuple<string?, long>> tuples = await _engine.ModelGetAllAsync(
                    modelDef.KVStoreName,
                    modelDef.ModelType.FullName!).ConfigureAwait(false);

                return MapTupleToModel<T>(tuples);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw Exceptions.Unkown(type: typeof(T).FullName, storeName: modelDef.KVStoreName, key: null, innerException: ex);
            }
        }

        public Task AddAsync<T>(T item, string lastUser) where T : KVStoreModel, new()
        {
            return AddAsync(new T[] { item }, lastUser);
        }

        public async Task AddAsync<T>(IEnumerable<T> items, string lastUser) where T : KVStoreModel, new()
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            KVStoreModelDef modelDef = KVStoreModelDefFactory.GetDef<T>();

            try
            {
                long newTimestamp = TimeUtil.UtcNowTicks;

                foreach (var t in items)
                {
                    t.Timestamp = newTimestamp;
                    t.LastUser = lastUser;
                }

                await _engine.ModelAddAsync(
                    modelDef.KVStoreName,
                    modelDef.ModelType.FullName!,
                    items.Select(t => GetModelKey(t, modelDef)),
                    items.Select(t => SerializeUtil.ToJson(t)),
                    newTimestamp
                    ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                //TODO: 要像数据库那样Restore吗？
                throw Exceptions.Unkown(modelDef.ModelType.FullName, modelDef.KVStoreName, items, ex);
            }
        }

        public Task UpdateAsync<T>(T item, string lastUser) where T : KVStoreModel, new()
        {
            return UpdateAsync(new T[] { item }, lastUser);
        }

        public async Task UpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : KVStoreModel, new()
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            KVStoreModelDef modelDef = KVStoreModelDefFactory.GetDef<T>();

            try
            {
                IEnumerable<long> originalVersions = items.Select(t => t.Timestamp).ToArray();
                long newTimestamp = TimeUtil.UtcNowTicks;

                foreach (var t in items)
                {
                    t.Timestamp = newTimestamp;
                    t.LastUser = lastUser;
                }

                await _engine.ModelUpdateAsync(
                    modelDef.KVStoreName,
                    modelDef.ModelType.FullName!,
                    items.Select(t => GetModelKey(t, modelDef)).ToList(),
                    items.Select(t => SerializeUtil.ToJson(t)).ToList(),
                    originalVersions, newTimestamp).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw Exceptions.Unkown(modelDef.ModelType.FullName, modelDef.KVStoreName, items, ex);
            }
        }

        public async Task DeleteAllAsync<T>() where T : KVStoreModel, new()
        {
            KVStoreModelDef modelDef = KVStoreModelDefFactory.GetDef<T>();

            try
            {
                await _engine.ModelDeleteAllAsync(
                   modelDef.KVStoreName,
                   modelDef.ModelType.FullName!
                   ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw Exceptions.Unkown(modelDef.ModelType.FullName, modelDef.KVStoreName, null, ex);
            }
        }

        public Task DeleteAsync<T>(string key, long timestamp) where T : KVStoreModel, new()
        {
            return DeleteAsync<T>(new string[] { key }, new long[] { timestamp });
        }

        public async Task DeleteAsync<T>(IEnumerable<string> keys, IEnumerable<long> timestamps) where T : KVStoreModel, new()
        {
            ThrowIf.NullOrEmpty(timestamps, nameof(timestamps));

            if (keys.Count() != timestamps.Count())
            {
                throw Exceptions.VersionsKeysNotEqualError();
            }

            KVStoreModelDef modelDef = KVStoreModelDefFactory.GetDef<T>();

            try
            {
                await _engine.ModelDeleteAsync(
                    modelDef.KVStoreName,
                    modelDef.ModelType.FullName!,
                    keys,
                    timestamps
                    ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw Exceptions.Unkown(modelDef.ModelType.FullName, modelDef.KVStoreName, keys: keys, values: timestamps, innerException: ex);
            }
        }

        private static IEnumerable<T?> MapTupleToModel<T>(IEnumerable<Tuple<string?, long>> tuples) where T : KVStoreModel, new()
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
                    item.Timestamp = t.Item2;
                    rt.Add(item);
                }
            }

            return rt;
        }
    }
}
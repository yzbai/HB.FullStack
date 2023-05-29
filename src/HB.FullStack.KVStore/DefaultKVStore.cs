using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Convert;
using HB.FullStack.KVStore.Engine;
using HB.FullStack.KVStore.KVStoreModels;

namespace HB.FullStack.KVStore
{
    public class DefaultKVStore : IKVStore
    {
        private readonly IKVStoreEngine _engine;
        private readonly IKVStoreModelDefFactory _modelDefFactory;

        public DefaultKVStore(IKVStoreEngine kvstoreEngine, IKVStoreModelDefFactory kvStoreModelDefFactory)
        {
            _engine = kvstoreEngine;
            _modelDefFactory = kvStoreModelDefFactory;

            _modelDefFactory.Initialize(kvstoreEngine);
        }

        private static string GetModelKey<T>(T item, KVStoreModelDef modelDef) where T : class, IKVStoreModel
        {
            StringBuilder builder = new StringBuilder();

            foreach (var propertyInfo in modelDef.OrderedKeyPropertyInfos)
            {
                builder.Append(StringConvertCenter.ToStringFrom(propertyInfo.PropertyType, propertyInfo.GetValue(item), StringConvertPurpose.NONE)!);
                builder.Append('_');
            }

            return builder.RemoveLast().ToString();
        }

        public string GetModelKey<T>(T item) where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            return GetModelKey(item, modelDef);
        }

        public async Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<object> keys) where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

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

        public async Task<IEnumerable<T?>> GetAllAsync<T>() where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();
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

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        public async Task AddAsync<T>(IEnumerable<T> items, string lastUser) where T : class, IKVStoreModel
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            try
            {
                long newTimestamp = TimeUtil.Timestamp;

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

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        public async Task UpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : class, IKVStoreModel
        {
            if (!items.Any())
            {
                return;
            }

            ThrowIf.NotValid(items, nameof(items));

            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            try
            {
                IEnumerable<long> originalTimestamps = items.Select(t => t.Timestamp).ToArray();
                long newTimestamp = TimeUtil.Timestamp;

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
                    originalTimestamps,
                    newTimestamp).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw Exceptions.Unkown(modelDef.ModelType.FullName, modelDef.KVStoreName, items, ex);
            }
        }

        public async Task DeleteAllAsync<T>() where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

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

        /// <summary>
        /// modelKeys作为一个整体，有一个发生主键冲突，则全部失败
        /// </summary>
        public async Task DeleteAsync<T>(IEnumerable<object> keys, IEnumerable<long> timestamps) where T : class, IKVStoreModel
        {
            ThrowIf.NullOrEmpty(timestamps, nameof(timestamps));

            if (keys.Count() != timestamps.Count())
            {
                throw Exceptions.VersionsKeysNotEqualError();
            }

            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

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

        private static IEnumerable<T?> MapTupleToModel<T>(IEnumerable<Tuple<string?, long>> tuples) where T : class, IKVStoreModel
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
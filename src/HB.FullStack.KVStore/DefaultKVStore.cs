using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Convert;
using HB.FullStack.KVStore.Config;
using HB.FullStack.KVStore.Engine;
using HB.FullStack.KVStore.KVStoreModels;

using Microsoft.Extensions.Options;

namespace HB.FullStack.KVStore
{
    public class DefaultKVStore : IKVStore
    {
        private readonly KVStoreOptions _options;
        private readonly IKVStoreEngine _engine;
        private readonly IKVStoreModelDefFactory _modelDefFactory;

        public DefaultKVStore(IOptions<KVStoreOptions> options, IKVStoreEngine kvstoreEngine, IKVStoreModelDefFactory kvStoreModelDefFactory)
        {
            _options = options.Value;
            _engine = kvstoreEngine;
            _modelDefFactory = kvStoreModelDefFactory;

            _engine.Initialize(_options);
        }

        public string GetKey<T>(T item) where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            return GetKey(item, modelDef);
        }

        private static string GetKey<T>(T item, KVStoreModelDef modelDef) where T : class, IKVStoreModel
        {
            StringBuilder builder = new StringBuilder();

            foreach (var propertyInfo in modelDef.OrderedKeyPropertyInfos)
            {
                builder.Append(StringConvertCenter.ToStringFrom(propertyInfo.PropertyType, propertyInfo.GetValue(item), StringConvertPurpose.NONE)!);
                builder.Append('_');
            }

            return builder.RemoveLast().ToString();
        }

        //TODO: 支持一个Model多个Key的联合输入
        private static IEnumerable<string> GetKeys(IEnumerable<object> keys, KVStoreModelDef modelDef)
        {
            if (modelDef.OrderedKeyPropertyInfos.Count != 1)
            {
                throw new ArgumentException($"{modelDef.FullName} has one than one KVStore key.");
            }

            PropertyInfo keyPropertyInfo = modelDef.OrderedKeyPropertyInfos[0];

            return keys
                .Select(k => StringConvertCenter.ToStringFrom(keyPropertyInfo.PropertyType, k, StringConvertPurpose.NONE))
                .ToList();
        }


        public async Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<object> keys) where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            try
            {
                IEnumerable<Tuple<byte[]?, long>> tuples = await _engine.GetAsync(
                    modelDef.SchemaName,
                    modelDef.ModelType.FullName!,
                    GetKeys(keys, modelDef)).ConfigureAwait(false);

                return MapTupleToModel<T>(tuples);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw KVStoreExceptions.Unkown(type: typeof(T).FullName, storeName: modelDef.SchemaName, key: keys, innerException: ex);
            }
        }

        public async Task<IEnumerable<T?>> GetAllAsync<T>() where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();
            try
            {
                IEnumerable<Tuple<byte[]?, long>> tuples = await _engine.GetAllAsync(
                    modelDef.SchemaName,
                    modelDef.ModelType.FullName!).ConfigureAwait(false);

                return MapTupleToModel<T>(tuples);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw KVStoreExceptions.Unkown(type: typeof(T).FullName, storeName: modelDef.SchemaName, key: null, innerException: ex);
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

                IList<string> keys = new List<string>();
                IList<byte[]?> modelBytes = new List<byte[]?>();

                foreach (var t in items)
                {
                    t.Timestamp = newTimestamp;
                    t.LastUser = lastUser;

                    keys.Add(GetKey(t, modelDef));
                    modelBytes.Add(SerializeUtil.Serialize(t));
                }

                await _engine.AddAsync(
                    modelDef.SchemaName,
                    modelDef.ModelType.FullName!,
                    keys,
                    modelBytes,
                    newTimestamp
                    ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                //TODO: 要像数据库那样Restore吗？
                throw KVStoreExceptions.Unkown(modelDef.ModelType.FullName, modelDef.SchemaName, items, ex);
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

                IList<string> keys = new List<string>();
                IList<byte[]?> modelBytes = new List<byte[]?>();


                foreach (var t in items)
                {
                    t.Timestamp = newTimestamp;
                    t.LastUser = lastUser;

                    keys.Add(GetKey(t, modelDef));
                    modelBytes.Add(SerializeUtil.Serialize(t));
                }

                await _engine.UpdateAsync(
                    modelDef.SchemaName,
                    modelDef.ModelType.FullName!,
                    keys,
                    modelBytes,
                    originalTimestamps,
                    newTimestamp).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw KVStoreExceptions.Unkown(modelDef.ModelType.FullName, modelDef.SchemaName, items, ex);
            }
        }

        public async Task DeleteAsync<T>(T item) where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            var keys = new string[] { GetKey(item, modelDef) };
            var timestamps = new long[] { item.Timestamp };

            try
            {
                await _engine.DeleteAsync(
                    modelDef.SchemaName,
                    modelDef.ModelType.FullName!,
                    keys,
                    timestamps
                    ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw KVStoreExceptions.Unkown(modelDef.ModelType.FullName, modelDef.SchemaName, keys: keys, values: timestamps, innerException: ex);
            }
        }

        public async Task DeleteAllAsync<T>() where T : class, IKVStoreModel
        {
            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            try
            {
                await _engine.DeleteAllAsync(
                   modelDef.SchemaName,
                   modelDef.ModelType.FullName!
                   ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw KVStoreExceptions.Unkown(modelDef.ModelType.FullName, modelDef.SchemaName, null, ex);
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
                throw KVStoreExceptions.VersionsKeysNotEqualError();
            }

            KVStoreModelDef modelDef = _modelDefFactory.GetDef<T>();

            try
            {
                await _engine.DeleteAsync(
                    modelDef.SchemaName,
                    modelDef.ModelType.FullName!,
                    GetKeys(keys, modelDef),
                    timestamps
                    ).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not KVStoreException)
            {
                throw KVStoreExceptions.Unkown(modelDef.ModelType.FullName, modelDef.SchemaName, keys: keys, values: timestamps, innerException: ex);
            }
        }

        private static IEnumerable<T?> MapTupleToModel<T>(IEnumerable<Tuple<byte[]?, long>> tuples) where T : class, IKVStoreModel
        {
            List<T?> rt = new List<T?>();

            foreach (var t in tuples)
            {
                T? item = SerializeUtil.Deserialize<T>(t.Item1);
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
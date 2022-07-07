
using HB.FullStack.KVStore.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HB.FullStack.KVStore.KVStoreModels
{
    internal static class KVStoreModelDefFactory
    {
        private static IDictionary<string, KVStoreModelSchema> _typeSchemaDict = null!;
        private static readonly ConcurrentDictionary<Type, KVStoreModelDef> _defDict = new ConcurrentDictionary<Type, KVStoreModelDef>();
        private static KVStoreSettings _settings = null!;
        private static string? _firstDefaultInstanceName = null!;

        public static void Initialize(IKVStoreEngine kVStoreEngine)
        {
            _settings = kVStoreEngine.Settings;
            _firstDefaultInstanceName = kVStoreEngine.FirstDefaultInstanceName;

            IEnumerable<Type> allModelTypes;

            if (_settings.AssembliesIncludeModel.IsNullOrEmpty())
            {
                allModelTypes = ReflectUtil.GetAllTypeByCondition(kvstoreModelTypeCondition);
            }
            else
            {
                allModelTypes = ReflectUtil.GetAllTypeByCondition(kVStoreEngine.Settings.AssembliesIncludeModel, kvstoreModelTypeCondition);
            }

            _typeSchemaDict = ConstructeSchemaDict(allModelTypes);

            static bool kvstoreModelTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(KVStoreModel)) && !t.IsAbstract;
            }
        }

        private static IDictionary<string, KVStoreModelSchema> ConstructeSchemaDict(IEnumerable<Type> allModelTypes)
        {
            IDictionary<string, KVStoreModelSchema> filedDict = _settings.KVStoreModels.ToDictionary(t => t.ModelTypeFullName);
            IDictionary<string, KVStoreModelSchema> resultDict = new Dictionary<string, KVStoreModelSchema>();

            foreach (var type in allModelTypes)
            {
                KVStoreAttribute? attribute = type.GetCustomAttribute<KVStoreAttribute>();

                filedDict.TryGetValue(type.FullName!, out KVStoreModelSchema? fileConfigured);

                string? instanceName = null;

                if (attribute != null)
                {
                    instanceName = attribute.InstanceName.IsNullOrEmpty() ? _firstDefaultInstanceName : attribute.InstanceName!;
                }

                if (fileConfigured != null)
                {
                    instanceName = fileConfigured.InstanceName;
                }

                if (instanceName.IsNullOrEmpty())
                {
                    instanceName = _firstDefaultInstanceName;
                }

                KVStoreModelSchema modelSchema = new KVStoreModelSchema
                {
                    ModelTypeFullName = type.FullName!,
                    InstanceName = instanceName!
                };

                resultDict.Add(type.FullName!, modelSchema);
            }

            return resultDict;
        }

        public static KVStoreModelDef GetDef<T>()
        {
            return GetDef(typeof(T));
        }

        public static KVStoreModelDef GetDef(Type type)
        {
            return _defDict.GetOrAdd(type, t => CreateModelDef(t));
        }

        private static KVStoreModelDef CreateModelDef(Type type)
        {
            if (!_typeSchemaDict.TryGetValue(type.FullName!, out KVStoreModelSchema? storeModelSchema))
            {
                throw Exceptions.NoModelSchemaFound(type:type.FullName);
            }

            KVStoreModelDef modelDef = new KVStoreModelDef(storeModelSchema.InstanceName, type);

            #region Handle Key Properties

            PropertyInfo[] properties = type.GetTypeInfo().GetProperties();

            PropertyInfo? backupKeyPropertyInfo = null;

            foreach (PropertyInfo info in properties)
            {
                KVStoreKeyAttribute? keyAttr = info.GetCustomAttribute<KVStoreKeyAttribute>();

                if (keyAttr != null)
                {
                    modelDef.KeyPropertyInfos.Add(keyAttr.Order, info);
                }
                else if (info.GetCustomAttribute<KVStoreBackupKeyAttribute>() != null)
                {
                    backupKeyPropertyInfo = info;
                }
            }

            //如果KVStoreKey没有找到，就启用BackupKey

            if (!modelDef.KeyPropertyInfos.Any())
            {
                if (backupKeyPropertyInfo == null)
                {
                    throw Exceptions.LackKVStoreKeyAttributeError(type:modelDef.ModelType.FullName);
                }

                modelDef.KeyPropertyInfos.Add(0, backupKeyPropertyInfo);
            }

            #endregion

            return modelDef;
        }
    }
}

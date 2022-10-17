
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HB.FullStack.Common.Models;
using HB.FullStack.KVStore.Engine;

namespace HB.FullStack.KVStore.KVStoreModels
{
    internal class KVStoreModelDefFactory : IKVStoreModelDefFactory, IModelDefProvider
    {
        private IDictionary<string, KVStoreModelSchema> _typeSchemaDict = null!;
        private readonly ConcurrentDictionary<Type, KVStoreModelDef> _defDict = new ConcurrentDictionary<Type, KVStoreModelDef>();
        private KVStoreSettings _settings = null!;
        private string? _firstDefaultInstanceName = null!;



        public void Initialize(IKVStoreEngine kVStoreEngine)
        {
            _settings = kVStoreEngine.Settings;
            _firstDefaultInstanceName = kVStoreEngine.FirstDefaultInstanceName;

            IEnumerable<Type> allModelTypes;

            if (_settings.AssembliesIncludeModel.IsNullOrEmpty())
            {
                allModelTypes = ReflectionUtil.GetAllTypeByCondition(kvstoreModelTypeCondition);
            }
            else
            {
                allModelTypes = ReflectionUtil.GetAllTypeByCondition(kVStoreEngine.Settings.AssembliesIncludeModel, kvstoreModelTypeCondition);
            }

            _typeSchemaDict = ConstructeSchemaDict(allModelTypes);

            static bool kvstoreModelTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(KVStoreModel)) && !t.IsAbstract;
            }
        }

        private IDictionary<string, KVStoreModelSchema> ConstructeSchemaDict(IEnumerable<Type> allModelTypes)
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

        public KVStoreModelDef GetDef<T>()
        {
            return GetDef(typeof(T));
        }

        public KVStoreModelDef GetDef(Type type)
        {
            return _defDict.GetOrAdd(type, t => CreateModelDef(t));
        }

        private KVStoreModelDef CreateModelDef(Type type)
        {
            if (!_typeSchemaDict.TryGetValue(type.FullName!, out KVStoreModelSchema? storeModelSchema))
            {
                throw Exceptions.NoModelSchemaFound(type: type.FullName);
            }

            KVStoreModelDef modelDef = new KVStoreModelDef(storeModelSchema.InstanceName, type) { Kind = ModelKind.KV };

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
                    throw Exceptions.LackKVStoreKeyAttributeError(type: modelDef.ModelType.FullName);
                }

                modelDef.KeyPropertyInfos.Add(0, backupKeyPropertyInfo);
            }

            #endregion

            return modelDef;
        }

        #region IModelDefProvider Memebers

        ModelKind IModelDefProvider.ModelKind => ModelKind.KV;

        ModelDef? IModelDefProvider.GetModelDef(Type type) => GetDef(type);

        #endregion
    }
}

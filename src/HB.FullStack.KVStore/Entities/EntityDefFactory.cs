
using HB.FullStack.KVStore.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HB.FullStack.KVStore.Entities
{
    internal static class EntityDefFactory
    {
        private static IDictionary<string, KVStoreEntitySchema> _typeSchemaDict = null!;
        private static readonly ConcurrentDictionary<Type, KVStoreEntityDef> _defDict = new ConcurrentDictionary<Type, KVStoreEntityDef>();
        private static KVStoreSettings _settings = null!;
        private static string? _firstDefaultInstanceName = null!;

        public static void Initialize(IKVStoreEngine kVStoreEngine)
        {
            _settings = kVStoreEngine.Settings;
            _firstDefaultInstanceName = kVStoreEngine.FirstDefaultInstanceName;

            IEnumerable<Type> allEntityTypes;

            if (_settings.AssembliesIncludeEntity.IsNullOrEmpty())
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(kvstoreEntityTypeCondition);
            }
            else
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(kVStoreEngine.Settings.AssembliesIncludeEntity, kvstoreEntityTypeCondition);
            }

            _typeSchemaDict = ConstructeSchemaDict(allEntityTypes);

            static bool kvstoreEntityTypeCondition(Type t)
            {
                return t.IsSubclassOf(typeof(KVStoreEntity)) && !t.IsAbstract;
            }
        }

        private static IDictionary<string, KVStoreEntitySchema> ConstructeSchemaDict(IEnumerable<Type> allEntityTypes)
        {
            IDictionary<string, KVStoreEntitySchema> filedDict = _settings.KVStoreEntities.ToDictionary(t => t.EntityTypeFullName);
            IDictionary<string, KVStoreEntitySchema> resultDict = new Dictionary<string, KVStoreEntitySchema>();

            foreach (var type in allEntityTypes)
            {
                KVStoreAttribute? attribute = type.GetCustomAttribute<KVStoreAttribute>();

                filedDict.TryGetValue(type.FullName!, out KVStoreEntitySchema? fileConfigured);

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

                KVStoreEntitySchema entitySchema = new KVStoreEntitySchema
                {
                    EntityTypeFullName = type.FullName!,
                    InstanceName = instanceName!
                };

                resultDict.Add(type.FullName!, entitySchema);
            }

            return resultDict;
        }

        public static KVStoreEntityDef GetDef<T>()
        {
            return GetDef(typeof(T));
        }

        public static KVStoreEntityDef GetDef(Type type)
        {
            return _defDict.GetOrAdd(type, t => CreateEntityDef(t));
        }

        private static KVStoreEntityDef CreateEntityDef(Type type)
        {
            if (!_typeSchemaDict.TryGetValue(type.FullName!, out KVStoreEntitySchema? storeEntitySchema))
            {
                throw Exceptions.NoEntitySchemaFound(type:type.FullName);
            }

            KVStoreEntityDef entityDef = new KVStoreEntityDef(storeEntitySchema.InstanceName, type);

            #region Handle Key Properties

            PropertyInfo[] properties = type.GetTypeInfo().GetProperties();

            PropertyInfo? backupKeyPropertyInfo = null;

            foreach (PropertyInfo info in properties)
            {
                KVStoreKeyAttribute? keyAttr = info.GetCustomAttribute<KVStoreKeyAttribute>();

                if (keyAttr != null)
                {
                    entityDef.KeyPropertyInfos.Add(keyAttr.Order, info);
                }
                else if (info.GetCustomAttribute<KVStoreBackupKeyAttribute>() != null)
                {
                    backupKeyPropertyInfo = info;
                }
            }

            //如果KVStoreKey没有找到，就启用BackupKey

            if (!entityDef.KeyPropertyInfos.Any())
            {
                if (backupKeyPropertyInfo == null)
                {
                    throw Exceptions.LackKVStoreKeyAttributeError(type:entityDef.EntityType.FullName);
                }

                entityDef.KeyPropertyInfos.Add(0, backupKeyPropertyInfo);
            }

            #endregion

            return entityDef;
        }
    }
}

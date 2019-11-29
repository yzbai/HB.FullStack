using HB.Framework.KVStore.Engine;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HB.Framework.KVStore.Entity
{
    internal class DefaultKVStoreModelDefFactory : IKVStoreEntityDefFactory
    {
        private readonly object _lockObj = new object();
        private readonly IKVStoreEngine _kvStoreEngine;
        private readonly KVStoreSettings _settings;
        private readonly IDictionary<string, KVStoreEntitySchema> _typeSchemaDict = new Dictionary<string, KVStoreEntitySchema>();
        private readonly ConcurrentDictionary<Type, KVStoreEntityDef> _defDict = new ConcurrentDictionary<Type, KVStoreEntityDef>();

        public DefaultKVStoreModelDefFactory(IKVStoreEngine kVStoreEngine)
        {
            _kvStoreEngine = kVStoreEngine.ThrowIfNull(nameof(kVStoreEngine));
            _settings = _kvStoreEngine.Settings;

            IEnumerable<Type> allEntityTypes;

            if (_settings.AssembliesIncludeEntity.IsNullOrEmpty())
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(t => t.IsSubclassOf(typeof(KVStoreEntity)));
            }
            else
            {
                allEntityTypes = ReflectUtil.GetAllTypeByCondition(_settings.AssembliesIncludeEntity, t => t.IsSubclassOf(typeof(KVStoreEntity)));
            }

            _typeSchemaDict = ConstructeSchemaDict(allEntityTypes);
        }

        private IDictionary<string, KVStoreEntitySchema> ConstructeSchemaDict(IEnumerable<Type> allEntityTypes)
        {
            IDictionary<string, KVStoreEntitySchema> filedDict = _settings.KVStoreEntities.ToDictionary(t => t.EntityTypeFullName);
            IDictionary<string, KVStoreEntitySchema> resultDict = new Dictionary<string, KVStoreEntitySchema>();

            allEntityTypes.ForEach(type => {
                KVStoreEntitySchemaAttribute attribute = type.GetCustomAttribute<KVStoreEntitySchemaAttribute>();

                filedDict.TryGetValue(type.FullName, out KVStoreEntitySchema fileConfigured);

                KVStoreEntitySchema entitySchema = new KVStoreEntitySchema { EntityTypeFullName = type.FullName };

                if (attribute != null)
                {
                    entitySchema.InstanceName = attribute.InstanceName.IsNullOrEmpty() ? _kvStoreEngine.FirstDefaultInstanceName : attribute.InstanceName;
                }

                if (fileConfigured != null)
                {
                    entitySchema.InstanceName = fileConfigured.InstanceName;
                }

                if (entitySchema.InstanceName.IsNullOrEmpty())
                {
                    entitySchema.InstanceName = _kvStoreEngine.FirstDefaultInstanceName;
                }

                resultDict.Add(type.FullName, entitySchema);
            });

            return resultDict;
        }

        public KVStoreEntityDef GetDef<T>()
        {
            return GetDef(typeof(T));
        }

        public KVStoreEntityDef GetDef(Type type)
        {
            if (!_defDict.ContainsKey(type))
            {
                lock (_lockObj)
                {
                    if (!_defDict.ContainsKey(type))
                    {
                        _defDict[type] = CreateEntityDef(type);
                    }
                }
            }

            return _defDict[type];
        }

        private KVStoreEntityDef CreateEntityDef(Type type)
        {
            KVStoreEntityDef entityDef = new KVStoreEntityDef {
                EntityType = type
            };

            PropertyInfo[] properties = type.GetTypeInfo().GetProperties();

            foreach (PropertyInfo info in properties)
            {
                KVStoreKeyAttribute keyAttr = info.GetCustomAttribute<KVStoreKeyAttribute>();

                if (keyAttr != null)
                {
                    entityDef.KeyPropertyInfos.Add(keyAttr.Order, info);
                }
            }

            if (entityDef.KeyPropertyInfos.Count == 0)
            {
                throw new Exception("lack of KVStoreKeyAttribute.");
            }

            if (_typeSchemaDict.TryGetValue(type.FullName, out KVStoreEntitySchema storeEntitySchema))
            {
                entityDef.KVStoreName = storeEntitySchema.InstanceName;
            }

            return entityDef;
        }
    }
}

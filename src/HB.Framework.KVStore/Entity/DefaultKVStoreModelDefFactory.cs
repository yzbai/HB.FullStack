using HB.Framework.KVStore.Engine;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace HB.Framework.KVStore.Entity
{
    internal class DefaultKVStoreModelDefFactory : IKVStoreEntityDefFactory
    {
        private readonly KVStoreSettings _settings;
        private readonly ConcurrentDictionary<Type, KVStoreEntityDef> _defDict = new ConcurrentDictionary<Type, KVStoreEntityDef>();
        private readonly object _lockObj = new object();

        public DefaultKVStoreModelDefFactory(IKVStoreEngine kVStoreEngine)
        {
            _settings = kVStoreEngine.ThrowIfNull(nameof(kVStoreEngine)).Settings;
        }

        public KVStoreEntityDef GetDef<T>()
        {
            return GetDef(typeof(T));
        }

        public KVStoreEntityDef GetDef(Type type)
        {
            if (!_defDict.ContainsKey(type))
            {
                lock(_lockObj)
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
            KVStoreEntityDef entityDef = new KVStoreEntityDef
            {
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

            KVStoreSchema schema = _options.GetKVStoreSchema(entityDef.EntityFullName);
            entityDef.KVStoreName = schema.InstanceName;

            return entityDef;
        }
    }
}

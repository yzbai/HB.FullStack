using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace HB.Framework.KVStore.Entity
{
    public class DefaultKVStoreModelDefFactory : IKVStoreEntityDefFactory
    {
        private KVStoreOptions _options;
        private ConcurrentDictionary<Type, KVStoreEntityDef> _defDict;
        private readonly object _lockObj;

        public DefaultKVStoreModelDefFactory(IOptions<KVStoreOptions> options)
        {
            _defDict = new ConcurrentDictionary<Type, KVStoreEntityDef>();
            _lockObj = new object();
            _options = options.Value;
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
                EntityFullName = type.FullName,
                EntityType = type
            };

            PropertyInfo[] properties = type.GetTypeInfo().GetProperties();

            foreach (PropertyInfo info in properties)
            {
                KVStoreKeyAttribute keyAttr = info.GetCustomAttribute<KVStoreKeyAttribute>();

                if (keyAttr != null)
                {
                    entityDef.KeyPropertyInfo = info;
                    break;
                }
            }

            if (entityDef.KeyPropertyInfo == null)
            {
                throw new Exception("lack of KVStoreKeyAttribute.");
            }

            KVStoreSchema schema = _options.GetKVStoreSchema(entityDef.EntityFullName);
            entityDef.KVStoreName = schema.KVStoreName;
            entityDef.KVStoreIndex = schema.KVStoreIndex;

            return entityDef;
        }
    }
}

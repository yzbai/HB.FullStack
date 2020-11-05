using System;
using System.Collections.Generic;
using System.Reflection;

namespace HB.Framework.KVStore.Entity
{
    public class KVStoreEntityDef
    {
        public string KVStoreName { get; set; }

        public Type EntityType { get; set; }

        public IDictionary<int, PropertyInfo> KeyPropertyInfos { get; } = new Dictionary<int, PropertyInfo>();

        public KVStoreEntityDef(string kvstoreName, Type type)
        {
            KVStoreName = kvstoreName;
            EntityType = type;
        }

    }
}

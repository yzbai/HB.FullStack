using System;
using System.Collections.Generic;
using System.Reflection;

namespace HB.Framework.KVStore.Entities
{
    public class KVStoreEntityDef
    {
        public string KVStoreName { get; set; }

        public Type EntityType { get; set; }

        public PropertyInfo? KeyPropertyInfo { get; set; }

        public KVStoreEntityDef(string kvstoreName, Type type)
        {
            KVStoreName = kvstoreName;
            EntityType = type;
        }

    }
}

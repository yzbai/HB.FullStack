using System;
using System.Collections.Generic;
using System.Reflection;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public class KVStoreModelDef
    {
        public string KVStoreName { get; set; }

        public Type ModelType { get; set; }

        public IDictionary<int, PropertyInfo> KeyPropertyInfos { get; } = new Dictionary<int, PropertyInfo>();

        public KVStoreModelDef(string kvstoreName, Type type)
        {
            KVStoreName = kvstoreName;
            ModelType = type;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

using HB.FullStack.Common.Models;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public class KVStoreModelDef : ModelDef
    {
        public string KVStoreName { get; set; }


        public IDictionary<int, PropertyInfo> KeyPropertyInfos { get; } = new Dictionary<int, PropertyInfo>();

        public KVStoreModelDef(string kvstoreName, Type type)
        {
            KVStoreName = kvstoreName;
            ModelType = type;
        }

        public override ModelPropertyDef? GetPropertyDef(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}

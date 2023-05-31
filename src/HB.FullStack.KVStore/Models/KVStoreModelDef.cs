using System;
using System.Collections.Generic;
using System.Reflection;

using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.KVStore.Config;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public class KVStoreModelDef : ModelDef
    {
        public KVStoreSchema KVStoreSchema { get; set; } = null!;

        private string? _schemaName;
        
        public string SchemaName => _schemaName ??= KVStoreSchema.Name;

        public IList<PropertyInfo> OrderedKeyPropertyInfos { get; set; } = null!;

        public KVStoreModelDef(Type type)
        {
            Kind = ModelKind.KV;
            ModelType = type;
            FullName = type.FullName!;
            IsPropertyTrackable = type.IsAssignableTo(typeof(IPropertyTrackableObject));
        }

        public override ModelPropertyDef? GetPropertyDef(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Concurrent;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Common.Models
{
    internal class PlainModelDefProvider : IModelDefProvider
    {
        private readonly ConcurrentDictionary<Type, ModelDef?> _defDict = new ConcurrentDictionary<Type, ModelDef?>();
        public ModelKind ModelKind => ModelKind.Plain;

        public ModelDef? GetModelDef(Type type)
        {
            return _defDict.GetOrAdd(type, type => CreateModelDef(type));
        }

        private static ModelDef? CreateModelDef(Type type)
        {
            if (!typeof(Model).IsAssignableFrom(type))
            {
                return null;
            }

            PlainModelDef modelDef = new PlainModelDef
            {
                Kind = ModelKind.Plain,
                ModelFullName = type.FullName!,
                ModelType = type,
                IsPropertyTrackable = type.IsAssignableTo(typeof(IPropertyTrackableObject))
            };

            foreach (var property in type.GetProperties())
            {
                PlainModelPropertyDef propertyDef = new PlainModelPropertyDef
                {
                    Name = property.Name,
                    Type = property.PropertyType
                };

                modelDef.PropertyDict.Add(property.Name, propertyDef);
            }

            return modelDef;
        }
    }
}

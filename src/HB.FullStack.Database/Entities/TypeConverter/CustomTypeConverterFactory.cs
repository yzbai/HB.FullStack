#nullable enable

using System;
using System.Collections.Generic;

namespace HB.FullStack.Database.Entities
{
    internal class CustomTypeConverterFactory : ICustomTypeConverterFactory
    {
        private readonly IDictionary<Type, CustomTypeConverter> _converterDict = new Dictionary<Type, CustomTypeConverter>();

        public CustomTypeConverterFactory() { }

        public CustomTypeConverter? GetTypeConverter(Type type)
        {
            if (!type.IsSubclassOf(typeof(CustomTypeConverter)))
            {
                return null;
            }

            if (_converterDict.ContainsKey(type))
            {
                return _converterDict[type];
            }

            CustomTypeConverter? typeConverter = Activator.CreateInstance(type) as CustomTypeConverter;

            if (typeConverter != null)
            {
                _converterDict.Add(type, typeConverter);
            }

            return typeConverter;
        }
    }
}

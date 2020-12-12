#nullable enable

using System;
using System.Collections.Generic;

namespace HB.FullStack.Database.Entities
{
    internal static class CustomTypeConverterFactory
    {
        private static readonly IDictionary<Type, CustomTypeConverter> _converterDict = new Dictionary<Type, CustomTypeConverter>();

        public static CustomTypeConverter? GetTypeConverter(Type type)
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

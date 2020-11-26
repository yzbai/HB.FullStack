#nullable enable

using System;
using System.Collections.Generic;

namespace HB.FullStack.Database.Entities
{
    internal class DatabaseTypeConverterFactory : IDatabaseTypeConverterFactory
    {
        private readonly IDictionary<Type, DatabaseTypeConverter> _converterDict = new Dictionary<Type, DatabaseTypeConverter>();

        public DatabaseTypeConverterFactory() { }

        public DatabaseTypeConverter? GetTypeConverter(Type type)
        {
            if (!type.IsSubclassOf(typeof(DatabaseTypeConverter)))
            {
                return null;
            }

            if (_converterDict.ContainsKey(type))
            {
                return _converterDict[type];
            }

            DatabaseTypeConverter? typeConverter = Activator.CreateInstance(type) as DatabaseTypeConverter;

            if (typeConverter != null)
            {
                _converterDict.Add(type, typeConverter);
            }

            return typeConverter;
        }
    }
}

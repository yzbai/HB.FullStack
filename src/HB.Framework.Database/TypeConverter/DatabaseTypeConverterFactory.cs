using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database.Entity
{
    internal class DatabaseTypeConverterFactory : IDatabaseTypeConverterFactory
    {
        private readonly IDictionary<Type, DatabaseTypeConverter> converterDict = new Dictionary<Type, DatabaseTypeConverter>();

        public DatabaseTypeConverterFactory()
        {

        }

        public DatabaseTypeConverter GetTypeConverter(Type type)
        {
            if (!type.IsSubclassOf(typeof(DatabaseTypeConverter)))
            {
                return null;
            }

            if (converterDict.ContainsKey(type))
            {
                return converterDict[type];
            }

            DatabaseTypeConverter typeConverter = Activator.CreateInstance(type) as DatabaseTypeConverter;

            converterDict.Add(type, typeConverter);

            return typeConverter;
        }
    }
}

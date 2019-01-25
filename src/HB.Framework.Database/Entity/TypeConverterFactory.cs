using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database.Entity
{
    public class TypeConverterFactory : ITypeConverterFactory
    {
        private IDictionary<Type, TypeConverter> converterDict = new Dictionary<Type, TypeConverter>();

        public TypeConverterFactory()
        {

        }

        public TypeConverter GetTypeConverter(Type type)
        {
            if (!type.IsSubclassOf(typeof(TypeConverter)))
            {
                return null;
            }

            if (converterDict.ContainsKey(type))
            {
                return converterDict[type];
            }

            TypeConverter typeConverter = Activator.CreateInstance(type) as TypeConverter;

            converterDict.Add(type, typeConverter);

            return typeConverter;
        }
    }
}

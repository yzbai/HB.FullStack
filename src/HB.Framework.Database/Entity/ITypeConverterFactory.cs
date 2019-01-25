using System;

namespace HB.Framework.Database.Entity
{
    public interface ITypeConverterFactory
    {
        TypeConverter GetTypeConverter(Type type);
    }
}
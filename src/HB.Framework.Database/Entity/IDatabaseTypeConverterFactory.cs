using System;

namespace HB.Framework.Database.Entity
{
    public interface IDatabaseTypeConverterFactory
    {
        DatabaseTypeConverter GetTypeConverter(Type type);
    }
}
using System;

namespace HB.Framework.Database.Entity
{
    internal interface IDatabaseTypeConverterFactory
    {
        DatabaseTypeConverter GetTypeConverter(Type type);
    }
}
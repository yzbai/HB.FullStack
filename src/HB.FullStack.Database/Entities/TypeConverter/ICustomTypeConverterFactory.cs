#nullable enable

using System;

namespace HB.FullStack.Database.Entities
{
    internal interface ICustomTypeConverterFactory
    {
        CustomTypeConverter? GetTypeConverter(Type type);
    }
}
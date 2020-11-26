#nullable enable

using System;

namespace HB.FullStack.Database.Entities
{
    public interface IDatabaseTypeConverterFactory
    {
        DatabaseTypeConverter? GetTypeConverter(Type type);
    }
}
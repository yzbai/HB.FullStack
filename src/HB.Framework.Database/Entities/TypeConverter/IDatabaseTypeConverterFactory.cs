#nullable enable

using System;

namespace HB.Framework.Database.Entities
{
    public interface IDatabaseTypeConverterFactory
    {
        DatabaseTypeConverter? GetTypeConverter(Type type);
    }
}
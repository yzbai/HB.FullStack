#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    public interface ITypeConverter
    {
        DbType DbType { get; }

        string Statement { get; }

        object TypeValueToDbValue(object typeValue, Type propertyType);

        object DbValueToTypeValue(object dbValue, Type propertyType);
    }
}
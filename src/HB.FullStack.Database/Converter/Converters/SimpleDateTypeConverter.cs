#nullable enable

using HB.FullStack.Common;

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    public class SimpleDateTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "VARCHAR(14)";

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            return typeValue;
        }

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            return SimpleDate.ParseExactly(dbValue.ToString()!);
        }
    }
}
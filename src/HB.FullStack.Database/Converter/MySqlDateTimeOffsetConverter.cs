#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    internal class MySqlDateTimeOffsetConverter : ITypeConverter
    {
        public DbType DbType => DbType.Int64;

        public string Statement => "BIGINT";

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            return ((DateTimeOffset)typeValue).Ticks;
        }

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            return new DateTimeOffset((long)dbValue, TimeSpan.Zero);
        }
    }
}
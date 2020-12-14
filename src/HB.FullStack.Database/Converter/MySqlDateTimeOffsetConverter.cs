#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    internal class MySqlDateTimeOffsetConverter : ITypeConverter
    {
        public DbType DbType => DbType.Int64;

        public string Statement => "BIGINT";

        public object TypeValueToDbValue(object? typeValue, Type propertyType)
        {
            if (typeValue == null)
            {
                return DBNull.Value;
            }

            return ((DateTimeOffset)typeValue).Ticks;
        }

        public object? DbValueToTypeValue(object dbValue, Type dbValueType, Type propertyType)
        {
            if (dbValueType == typeof(DBNull))
            {
                return default(DateTimeOffset);
            }

            return new DateTimeOffset((long)dbValue, TimeSpan.Zero);
        }
    }
}


using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    internal class MySqlDateTimeOffsetTypeConverter : IDbValueConverter
    {
        public DbType DbType => DbType.Int64;

        public string DbTypeStatement => "BIGINT";

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
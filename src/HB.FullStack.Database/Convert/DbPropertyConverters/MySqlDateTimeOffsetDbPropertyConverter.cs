
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    internal class MySqlDateTimeOffsetDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.Int64;

        public string DbTypeStatement => "BIGINT";

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            return ((DateTimeOffset)typeValue).Ticks;
        }

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            return new DateTimeOffset((long)dbValue, TimeSpan.Zero);
        }
    }
}
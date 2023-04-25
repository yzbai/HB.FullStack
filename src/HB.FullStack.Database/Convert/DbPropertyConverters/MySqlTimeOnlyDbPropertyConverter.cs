
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    internal class MySqlTimeOnlyDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.Int64;

        public string DbTypeStatement => "BIGINT";

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            return ((TimeOnly)typeValue).Ticks;
        }

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            return new TimeOnly((long)dbValue);
        }
    }
}
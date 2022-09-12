using System;
using System.Data;

using HB.FullStack.Common;

namespace HB.FullStack.Database.Convert
{
    public class Time24HourTypeConverter : IDbValueConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR(10)";

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            return new Time24Hour(dbValue.ToString()!);
        }

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            return ((Time24Hour)typeValue).ToString();
        }
    }
}
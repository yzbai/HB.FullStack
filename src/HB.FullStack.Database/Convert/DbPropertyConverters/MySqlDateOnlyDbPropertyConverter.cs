
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    internal class MySqlDateOnlyDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.StringFixedLength;

        public string DbTypeStatement => "CHAR(10)";

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            return ((DateOnly)typeValue).ToString();
        }

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            return DateOnly.Parse(dbValue.ToString()!);
        }
    }
}
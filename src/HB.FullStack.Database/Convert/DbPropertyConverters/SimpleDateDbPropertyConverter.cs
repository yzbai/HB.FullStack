
using System;
using System.Data;

using HB.FullStack.Common;

namespace HB.FullStack.Database.Convert
{
    public class SimpleDateDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR(14)";

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            return typeValue;
        }

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            return SimpleDate.ParseExactly(dbValue.ToString()!);
        }
    }
}
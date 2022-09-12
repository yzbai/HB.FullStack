

using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    public class JsonTypeConverter : IDbValueConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR";

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            return SerializeUtil.ToJson(typeValue);
        }

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            return SerializeUtil.FromJson(propertyType, dbValue.ToString())!;
        }
    }
}


using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    public class JsonTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "VARCHAR";

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
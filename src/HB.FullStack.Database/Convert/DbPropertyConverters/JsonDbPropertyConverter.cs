
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    public class JsonDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR";

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            return SerializeUtil.ToJson(typeValue);
        }

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            return SerializeUtil.FromJson(propertyType, dbValue.ToString())!;
        }
    }
}
#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    public class JsonTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "VARCHAR";

        public object TypeValueToDbValue(object? typeValue, Type propertyType)
        {
            if (typeValue == null)
            {
                return DBNull.Value;
            }

            return SerializeUtil.ToJson(typeValue);
        }

        public object? DbValueToTypeValue(object dbValue, Type dbValueType, Type propertyType)
        {
            if (dbValueType == typeof(DBNull))
            {
                return null;
            }

            return SerializeUtil.FromJson(propertyType, dbValue.ToString());
        }
    }
}
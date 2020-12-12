#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Entities
{
    /// <summary>
    /// 把复杂类型变成字符串
    /// </summary>
    public abstract class CustomTypeConverter
    {
        public virtual DbType TypeToDbType(Type type)
        {
            return DbType.String;
        }

        public virtual string TypeToDbTypeStatement(Type type)
        {
            return "VARCHAR";
        }

        public string? TypeValueToDbValue(object? value)
        {
            if (value == null)
            {
                return null;
            }

            return TypeValueToStringDbValue(value);
        }

        public object? DbValueToTypeValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.GetType() == typeof(DBNull))
            {
                return null;
            }

            return StringDbValueToTypeValue(value.ToString());
        }

        protected abstract object? StringDbValueToTypeValue(string stringValue);
        protected abstract string TypeValueToStringDbValue(object typeValue);
    }
}

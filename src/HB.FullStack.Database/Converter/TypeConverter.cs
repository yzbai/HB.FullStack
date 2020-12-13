#nullable enable

using System.Data;

namespace System
{
    public class TypeConverter
    {
        public DbType DbType { get; set; }

        public string Statement { get; set; } = null!;

        /// <summary>
        /// input:typevalue，propertyType 要自己处理DBNull与Null
        /// </summary>
        public Func<object?, Type, object>? TypeValueToDbValue { get; set; }

        /// <summary>
        /// input:dbValue, propertyType 要自己处理DBNull与Null
        /// </summary>
        public Func<object, Type, object?>? DbValueToTypeValue { get; set; }
    }
}
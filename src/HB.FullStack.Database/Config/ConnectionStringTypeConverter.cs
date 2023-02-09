using System;
using System.ComponentModel;
using System.Globalization;

namespace HB.FullStack.Database.Config
{
    public partial class ConnectionString
    {
        /// <summary>
        /// 定义String和ConnectionString之间的相互转换, 用于ConfigurationBinder.TryConvertValue
        /// </summary>
        public class ConnectionStringTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            {
                if (sourceType == typeof(string)) { return true; }
                return base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            {
                if (destinationType == typeof(string)) { return true; }
                return base.CanConvertTo(context, destinationType);
            }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            {
                if (value is string text)
                {
                    return new ConnectionString(text);
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
            {
                if (destinationType == typeof(string) && value != null)
                {
                    return value.ToString();
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
